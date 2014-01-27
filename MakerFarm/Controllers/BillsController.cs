using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MakerFarm.Models;
using System.Data.SqlClient;
using PaperCutMF;
using PagedList;

namespace MakerFarm.Controllers
{
    [Authorize(Roles = "Administrator, Moderator")]
    public class BillsController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();
        private ServerCommandProxy PapercutServerProxy = new ServerCommandProxy(System.Configuration.ConfigurationManager.AppSettings.Get("PapercutServerDNS"), int.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("PapercutPort")), System.Configuration.ConfigurationManager.AppSettings.Get("PaperCutAuthToken"));

        // GET: /Bills/
        public ActionResult Index()
        {
            string CompleteFilesQuery = "Select dbo.Prints.* " +
                "from dbo.Prints " +
                "left outer join " +
                "( " +
                "Select dbo.PrintEvents.PrintID, dbo.PrintEvents.EventType, mxe.MostReventEvent " +
                "from dbo.PrintEvents " +
                "inner join " +
                "( " +
                "select dbo.PrintEvents.PrintID, MAX(dbo.PrintEvents.PrintEventId) as MostReventEvent " +
                "from dbo.PrintEvents " +
                "group by dbo.PrintEvents.PrintID " +
                ") mxe on dbo.PrintEvents.PrintId = mxe.PrintID and dbo.PrintEvents.PrintEventId = mxe.MostReventEvent " +
                ") pnt on dbo.Prints.PrintId = pnt.PrintID " +
                "where (pnt.EventType = @PrintingEventCompleted or pnt.EventType = @PrintingEventCanceled) and dbo.Prints.BilledUser = 0 " +
                "order by pnt.MostReventEvent DESC";
            string PrintAssignmentsQuery = "Select * " +
         "from dbo.PrintEvents " +
         "inner join ( " +
         "select dbo.PrintEvents.PrintID, MAX(dbo.PrintEvents.PrintEventId) as MostReventEvent " +
         "from dbo.PrintEvents " +
         "group by dbo.PrintEvents.PrintID " +
         ") mxe on dbo.PrintEvents.PrintID = mxe.PrintID and dbo.PrintEvents.PrintEventId = mxe.MostReventEvent " +
         "where dbo.PrintEvents.EventType = @PrintingEventCompleted or dbo.PrintEvents.EventType = @PrintingEventCanceled";
            SqlParameter PrintingEventCompleted = new SqlParameter("@PrintingEventCompleted", PrintEventType.PRINT_COMPLETED);
            SqlParameter PrintingEventCanceled = new SqlParameter("@PrintingEventCanceled", PrintEventType.PRINT_CANCELED);
            SqlParameter PrintingEventCompleted2 = new SqlParameter("@PrintingEventCompleted", PrintEventType.PRINT_COMPLETED);
            SqlParameter PrintingEventCanceled2 = new SqlParameter("@PrintingEventCanceled", PrintEventType.PRINT_CANCELED);
            Dictionary<long, PrintEvent> PrintingAssignments = db.PrintEvents.SqlQuery(PrintAssignmentsQuery, PrintingEventCompleted2, PrintingEventCanceled2).ToDictionary(p => p.PrintId);
            ViewData["PrintingAssignments"] = PrintingAssignments;
            List<Print> Waiting = db.Prints.SqlQuery(CompleteFilesQuery, PrintingEventCompleted, PrintingEventCanceled).ToList();
            return View(Waiting);
        }

        // GET: /Bills/
        public ActionResult BillingHistory(int? page, string sortOrder, string currentFilter, string searchString)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = sortOrder == "Name" ? "name_desc" : "Name";
            ViewBag.DateSortParm = String.IsNullOrEmpty(sortOrder) ? "" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var bills = from b in db.Bills
                           select b;

            if (!String.IsNullOrEmpty(searchString))
            {
                bills = bills.Where(s => s.Print.UserName.ToUpper().Contains(searchString.ToUpper()));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    bills = bills.OrderByDescending(s => s.Print.UserName);
                    break;
                case "Date":
                    bills = bills.OrderBy(s => s.PrintEventId);
                    break;
                case "Name":
                    bills = bills.OrderByDescending(s => s.Print.UserName);
                    break;
                default:
                    bills = bills.OrderByDescending(s => s.BillId);
                    break;
            }

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(bills.ToPagedList(pageNumber, pageSize));
        }

        // GET: /Bills/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bill bill = db.Bills.Find(id);
            if (bill == null)
            {
                return HttpNotFound();
            }
            return View(bill);
        }

        // GET: /Bills/Create
        public ActionResult Create(long id)
        {
            PrintEvent BilledEvent = db.PrintEvents.Find(id);
            if (BilledEvent == null)
            {
                return HttpNotFound();
            }
            ViewData["BilledEvent"] = BilledEvent;
            List<PrintEvent> AssociatedEvents = db.PrintEvents.Where(P => P.PrintId == BilledEvent.PrintId).ToList();
            ViewData["AssociatedEvents"] = AssociatedEvents;

            return View();
        }

        // POST: /Bills/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="BillId,UserName,TotalBillingAmount,PrintEventId,PrintId,Comment")] Bill bill)
        {
            PrintEvent BilledEvent = db.PrintEvents.Find(bill.PrintEventId);
            bill.BillingTime = DateTime.Now;
            if (ModelState.IsValid && BilledEvent != null)
            {
                Print print = BilledEvent.Print;
                print.BilledUser = true;
                db.Entry(print).State = EntityState.Modified;
                db.Bills.Add(bill);
                db.SaveChanges();
                if (bool.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("EnablePaperCutIntegration")) && print.InternalUser && PapercutServerProxy.UserExists(print.UserName))
                {//Dispatch Print off to papercut
                    Printer P = print.GetLastPrinter();
                    if (P != null && !P.PapercutPrintQueue.Equals("") && !P.PapercutPrintServer.Equals(""))
                    {
                        string printjob = "document-name=" + print.FileName +",user=" + print.UserName + ",server=" + P.PapercutPrintServer + ",printer=" + P.PapercutPrintQueue + ",time=" + BilledEvent.EventTimeStamp.ToString("yyyyMMddTHHmmss") + ",cost=" + bill.TotalBillingAmount + ",comment=BillID:" + bill.BillId + " EventID:" + bill.PrintEventId + " PrintID: " + bill.PrintId; // This assembles a string of information to submit the printjob 
                        if (System.IO.File.Exists(print.GetPath()))
                        {
                            long size = new System.IO.FileInfo(print.GetPath()).Length/1024;
                            printjob = printjob + ",document-size-kb=" + size;
                        }
                        else if (System.IO.File.Exists(print.GetFlaggedPath()))
                        {
                            long size = new System.IO.FileInfo(print.GetFlaggedPath()).Length / 1024;
                            printjob = printjob + ",document-size-kb=" + size;
                        }
                        PapercutServerProxy.ProcessJob(printjob);
                    }
                }
                return RedirectToAction("Index");
            }

            return View(bill);
        }

        // GET: /Bills/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bill bill = db.Bills.Find(id);
            if (bill == null)
            {
                return HttpNotFound();
            }
            return View(bill);
        }

        // POST: /Bills/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="BillId,UserName,BillingTime,TotalBillingAmount,PrintEventId,PrintId,Comment")] Bill bill)
        {
            if (ModelState.IsValid)
            {
                db.Entry(bill).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(bill);
        }

        // GET: /Bills/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bill bill = db.Bills.Find(id);
            if (bill == null)
            {
                return HttpNotFound();
            }
            return View(bill);
        }

        // POST: /Bills/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Bill bill = db.Bills.Find(id);
            db.Bills.Remove(bill);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
