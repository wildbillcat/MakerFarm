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
    [Authorize]
    public class PrintersController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();
        ServerCommandProxy PapercutServerProxy = new ServerCommandProxy(System.Configuration.ConfigurationManager.AppSettings.Get("PapercutServerDNS"), int.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("PapercutPort")), System.Configuration.ConfigurationManager.AppSettings.Get("PaperCutAuthToken"));

        // GET: /Printers/
        [Authorize(Roles = "Administrator, Moderator")]
        public ActionResult Index()
        {
            return View();
        }

        // GET: /Printers/Details/5
        [Authorize(Roles = "Administrator, Moderator")]
        public ActionResult Details(long? id, int? page, string sortOrder, int? page2, string sortOrder2)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = sortOrder == "Name" ? "name_desc" : "Name";
            ViewBag.DateSortParm = String.IsNullOrEmpty(sortOrder) ? "Date" : "";

            ViewBag.CurrentSort2 = sortOrder2;
            ViewBag.NameSortParm2 = sortOrder2 == "Name" ? "name_desc" : "Name";
            ViewBag.DateSortParm2 = String.IsNullOrEmpty(sortOrder2) ? "Date" : "";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Printer printer = db.Printers.Find(id);
            if (printer == null)
            {
                return HttpNotFound();
            }

            ViewBag.Title = String.Concat("Details: ", printer.PrinterName);
            SqlParameter[] Params = { new SqlParameter("@PrinterTypeID", printer.PrinterTypeId) };
            List<Material> Materials = db.Materials.SqlQuery(
                "Select dbo.Materials.* " + 
                "from dbo.Materials inner join ( " +
                "Select dbo.Materials.MaterialId, dbo.Materials.MaterialName, dbo.Materials.MaterialSpoolQuantity, dbo.Materials.PrinterTypeId " +
                "FROM dbo.Materials LEFT JOIN dbo.MaterialCheckouts " +
                "ON dbo.Materials.MaterialId = dbo.MaterialCheckouts.MaterialId " +
                "GROUP BY dbo.Materials.MaterialId, dbo.Materials.MaterialName, dbo.Materials.MaterialSpoolQuantity, dbo.Materials.PrinterTypeId " +
                "HAVING ((Count(dbo.MaterialCheckouts.MaterialId) < dbo.Materials.MaterialSpoolQuantity) or dbo.Materials.MaterialSpoolQuantity < 0) and (dbo.Materials.PrinterTypeId = @PrinterTypeID) " +
                ") mxe on dbo.Materials.MaterialId = mxe.MaterialId", Params).ToList();
            ViewData["Materials"] = new SelectList(Materials, "MaterialId", "MaterialName");
            
            string status = "Unknown";
            var PrinterHistory = db.PrinterStatusLogs.Where(P => P.PrinterId == id);

            switch (sortOrder)
            {
                case "name_desc":
                    PrinterHistory = PrinterHistory.OrderByDescending(s => s.LoggedPrinterStatus);
                    break;
                case "Date":
                    PrinterHistory = PrinterHistory.OrderBy(s => s.LogEntryDate);
                    break;
                case "Name":
                    PrinterHistory = PrinterHistory.OrderBy(s => s.LoggedPrinterStatus);
                    break;
                default:
                    PrinterHistory = PrinterHistory.OrderByDescending(s => s.LogEntryDate);
                    break;
            }
            int pageSize = 20;
            int pageNumber = (page ?? 1);

            ViewData["PrinterHistory"] = PrinterHistory.ToPagedList(pageNumber, pageSize);

            PrinterStatusLog PStat = null;
            foreach (PrinterStatusLog pntsts in PrinterHistory)
            {
                if (PStat == null || PStat.LogEntryDate < pntsts.LogEntryDate)
                {
                    PStat = pntsts;
                }
            }

            if (PStat != null)
            {
                status = string.Concat(PStat.LoggedPrinterStatus.ToString(), " : ", PStat.LogEntryDate.ToString("F"));
            }
            
            ViewData["Status"] = status;

            var PrintHistory = db.PrintEvents.Where(P => P.PrinterId == id);

            switch (sortOrder2)
            {
                case "name_desc":
                    PrintHistory = PrintHistory.OrderByDescending(s => s.UserName);
                    break;
                case "Date":
                    PrintHistory = PrintHistory.OrderBy(s => s.EventTimeStamp);
                    break;
                case "Name":
                    PrintHistory = PrintHistory.OrderBy(s => s.UserName);
                    break;
                default:
                    PrintHistory = PrintHistory.OrderByDescending(s => s.EventTimeStamp);
                    break;
            }
            int pageSize2 = 20;
            int pageNumber2 = (page2 ?? 1);

            ViewData["PrintHistory"] = PrintHistory.ToPagedList(pageNumber2, pageSize2);

            string AssignedPrintQuery = "Select dbo.Prints.* " +
                "from dbo.Prints " +
                "left outer join " +
                "( " +
                "Select dbo.PrintEvents.PrintID, dbo.PrintEvents.EventType, dbo.PrintEvents.PrinterID " +
                "from dbo.PrintEvents " +
                "inner join " +
                "( " +
                "select dbo.PrintEvents.PrintID, MAX(dbo.PrintEvents.PrintEventId) as MostReventEvent " +
                "from dbo.PrintEvents " +
                "group by dbo.PrintEvents.PrintID " +
                ") mxe on dbo.PrintEvents.PrintId = mxe.PrintID and dbo.PrintEvents.PrintEventId = mxe.MostReventEvent " +
                ") pnt on dbo.Prints.PrintId = pnt.PrintID " +
                "where EventType = @PrintingEventStart and pnt.PrinterID = @PrinterId";
            SqlParameter PrintingEventStart = new SqlParameter("@PrintingEventStart", PrintEventType.PRINT_START);
            SqlParameter PrinterId = new SqlParameter("@PrinterId", id);
            Print AssignedPrint = null;
            try
            {
                AssignedPrint = db.Prints.SqlQuery(AssignedPrintQuery, PrintingEventStart, PrinterId).Single();
            }
            catch 
            {

            }

            ViewBag.AssignedPrint = AssignedPrint;

            return View(printer);
        }

        // GET: /Printers/Create
        [Authorize(Roles = "Administrator")]
        public ActionResult Create()
        {
            ViewBag.Title = "Create Printer";
            List<PrinterType> printerType = db.PrinterTypes.ToList();
            ViewData["PrintersList"] = new SelectList(printerType, "PrinterTypeId", "TypeName");
            if (printerType == null)
            {
                //The printer you attempted to use does not exist in the database!
                return RedirectToAction("Index", "PrinterTypes");
            }
            return View();
        }

        // POST: /Printers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Create([Bind(Include = "PrinterId,PrinterName,PrinterTypeId,InternalName,PapercutPrintServer,PapercutPrintQueue")] Printer printer)
        {

            if (printer.PrinterName.Equals("Null Printer"))
            {
                ModelState.AddModelError("TypeName", new Exception("Sorry, this is a Special Internal Name for Makerfarm. Please choose something else."));
            }
            if (ModelState.IsValid)
            {
                db.Printers.Add(printer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(printer);
        }

        // GET: /Printers/Edit/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.Title = "Create Printer";
            List<PrinterType> printerType = db.PrinterTypes.ToList();
            ViewData["PrintersList"] = new SelectList(printerType, "PrinterTypeId", "TypeName");
            if (printerType == null)
            {
                //The printer you attempted to use does not exist in the database!
                return RedirectToAction("Index", "PrinterTypes");
            }
            Printer printer = db.Printers.Find(id);
            if (printer == null)
            {
                return HttpNotFound();
            }
            return View(printer);
        }

        // POST: /Printers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit([Bind(Include = "PrinterId,PrinterName,PrinterTypeId,InternalName,PapercutPrintServer,PapercutPrintQueue")] Printer printer)
        {
            if (printer.PrinterName.Equals("Null Printer"))
            {
                ModelState.AddModelError("TypeName", new Exception("Sorry, this is a Special Internal Name for Makerfarm. Please choose something else."));
            }
            if (ModelState.IsValid)
            {
                db.Entry(printer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(printer);
        }

        // GET: /Printers/Delete/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Printer printer = db.Printers.Find(id);
            if (printer == null)
            {
                return HttpNotFound();
            }
            if (printer.PrinterName.Equals("Null Printer"))
            {
                return RedirectToAction("Index");
            }
            return View(printer);
        }

        // POST: /Printers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult DeleteConfirmed(long id)
        {
            Printer printer = db.Printers.Find(id);

            //Delete all print events
            List<PrintEvent> Events = db.PrintEvents.Where(P => P.PrinterId == printer.PrinterId).ToList();
            foreach (PrintEvent e in Events)
            {
                db.PrintEvents.Remove(e);
            }

            //Delete all print statuses
            List<PrinterStatusLog> Status = db.PrinterStatusLogs.Where(p => p.PrinterId == printer.PrinterId).ToList();
            foreach (PrinterStatusLog p in Status)
            {
                db.PrinterStatusLogs.Remove(p);
            }

            //Now remove the printer
            db.Printers.Remove(printer);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        
        public ActionResult PhysicalPrinterStatus(long id, bool compressed)
        {
            Machine M = db.Machines.Where(p => p.PrinterId == id).FirstOrDefault();
            ViewData["M"] = M;
            ViewData["compressed"] = compressed;
            return PartialView("_PhysicalPrinterStatusPartial");
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
