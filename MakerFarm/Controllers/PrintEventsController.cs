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

namespace MakerFarm.Controllers
{
    public class PrintEventsController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();

        // GET: /PrintEvents/
        public ActionResult Index()
        {
            var printevents = db.PrintEvents.Include(p => p.Print).Include(p => p.Printer);
            return View(printevents.ToList());
        }

        // GET: /PrintEvents/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrintEvent printevent = db.PrintEvents.Find(id);
            if (printevent == null)
            {
                return HttpNotFound();
            }
            return View(printevent);
        }

        // GET: /PrintEvents/Create
        public ActionResult Create(long id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Print Print = db.Prints.Find(id);
            if (Print == null)
            {
                return HttpNotFound();
            }
            ViewBag.CurrentUser = User.Identity.Name;
            List<PrintEvent> LastStatus = db.PrintEvents.Where(p => p.PrintId.Equals(id)).ToList();
            ViewBag.PrintId = id;
            ViewBag.Print = Print;
            List<PrintEventType> evts = new List<PrintEventType>();
            evts.Add(PrintEventType.PRINT_START);
            SelectList PrinterIds;
            string PrintMaterials = "";
            if (0 == LastStatus.Count() || !LastStatus.Last().EventType.Equals(PrintEventType.PRINT_START)) //Print Needs to be Sent!
            {
                //This is in need of some query optimization!
                SqlParameter[] Params = {new SqlParameter("@PrinterTypeId", Print.PrinterTypeId), new SqlParameter("@PrinterStatus", PrinterStatus.Online)};
                //Optimised the Query! Not sure why EF isn't following my models naming syntax for Id vs ID sporatically, will have to review at a later date.
                List<Printer> PrinterList = db.Printers.SqlQuery(
                "Select * FROM dbo.Printers INNER JOIN dbo.PrinterStatusLogs " +
                "ON dbo.Printers.PrinterID = dbo.PrinterStatusLogs.PrinterId " +
                "WHERE ((dbo.Printers.PrinterTypeId = @PrinterTypeId) " +
                "AND (dbo.PrinterStatusLogs.LoggedPrinterStatus = @PrinterStatus))", Params).ToList();
                List<Printer> MaterialCompatible = new List<Printer>();
                foreach (Printer P in PrinterList)
                {
                    int materialCompatability = 0;
                    foreach (string MatString in Print.MaterialIds.Split(','))
                    {
                        long M = long.Parse(MatString);
                        foreach (MaterialCheckout Mat in P.MaterialsInUse)
                        {
                            if (Mat.MaterialId == M)
                            {
                                materialCompatability++;
                            }
                        }
                    }
                    if (materialCompatability == Print.MaterialIds.Length)
                    {
                        MaterialCompatible.Add(P);
                    }
                }
                PrinterIds = new SelectList(MaterialCompatible, "PrinterId", "PrinterName");
                int i = 0;
                foreach (string MatString in Print.MaterialIds.Split(','))
                {
                    if (i == 0)
                    {
                        PrintMaterials = db.Materials.Find(long.Parse(MatString)).MaterialName;
                    }
                    else
                    {
                        PrintMaterials = string.Concat(PrintMaterials, ", ", db.Materials.Find(long.Parse(MatString)).MaterialName);
                    }
                }
                ViewBag.PrintMaterials = PrintMaterials;
            }
            else //Print was sent, updating status of print
            {
                List<SelectListItem> Printerlist = new List<SelectListItem>();
                Printerlist.Add(new SelectListItem() { Text = LastStatus.Last().Printer.PrinterName, Value = LastStatus.Last().Printer.PrinterId.ToString(), Selected = true });
                PrinterIds = new SelectList(Printerlist, "Value", "Text");
                evts = Enum.GetValues(typeof(MakerFarm.Models.PrintEventType)).Cast<MakerFarm.Models.PrintEventType>().ToList();
                evts.Remove(PrintEventType.PRINT_START);
            }
            ViewBag.PrintErrorTypeId = new SelectList(db.PrintErrorTypes.ToList(), "PrintErrorTypeId", "PrintErrorName");
            ViewBag.PrinterId = PrinterIds;
            ViewBag.EventTypes = evts;
            ViewBag.Send = evts.Count() == 1;
            return View();
        }

        // POST: /PrintEvents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PrintEventId,EventType,MaterialUsed,PrinterId,UserName,PrintId,PrintErrorTypeId,Comment")] PrintEvent printevent)
        {
            printevent.EventTimeStamp = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.PrintEvents.Add(printevent);
                db.SaveChanges();
                return RedirectToAction("Index", "Prints", new { id = printevent.Printer.PrinterTypeId });
            }

            ViewBag.PrintId = new SelectList(db.Prints, "PrintId", "FileName", printevent.PrintId);
            ViewBag.PrinterId = new SelectList(db.Printers, "PrinterId", "PrinterName", printevent.PrinterId);
            return View(printevent);
        }

        // GET: /PrintEvents/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrintEvent printevent = db.PrintEvents.Find(id);
            if (printevent == null)
            {
                return HttpNotFound();
            }
            ViewBag.PrintId = new SelectList(db.Prints, "PrintId", "FileName", printevent.PrintId);
            ViewBag.PrinterId = new SelectList(db.Printers, "PrinterId", "PrinterName", printevent.PrinterId);
            return View(printevent);
        }

        // POST: /PrintEvents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="PrintEventId,EventType,EventTimeStamp,MaterialUsed,PrinterId,UserName,PrintId")] PrintEvent printevent)
        {
            if (ModelState.IsValid)
            {
                db.Entry(printevent).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.PrintId = new SelectList(db.Prints, "PrintId", "FileName", printevent.PrintId);
            ViewBag.PrinterId = new SelectList(db.Printers, "PrinterId", "PrinterName", printevent.PrinterId);
            return View(printevent);
        }

        // GET: /PrintEvents/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrintEvent printevent = db.PrintEvents.Find(id);
            if (printevent == null)
            {
                return HttpNotFound();
            }
            return View(printevent);
        }

        // POST: /PrintEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            PrintEvent printevent = db.PrintEvents.Find(id);
            db.PrintEvents.Remove(printevent);
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
