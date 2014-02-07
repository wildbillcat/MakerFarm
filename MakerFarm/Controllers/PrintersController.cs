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
        public ActionResult Details(long? id)
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
            List<PrinterStatusLog> PrinterHistory = db.PrinterStatusLogs.Where(P => P.PrinterId == id).OrderByDescending(E => E.PrinterStatusLogId).ToList();
            if (PrinterHistory.Count > 0)
            {
                PrinterStatusLog P = PrinterHistory.First();
                status = string.Concat(P.LoggedPrinterStatus.ToString(), " : ", P.LogEntryDate.ToString("F"));
            }
            ViewData["PrinterHistory"] = PrinterHistory;
            
            
            ViewData["Status"] = status;
            
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
