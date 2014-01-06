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
    [Authorize(Roles = "Administrator, Moderator")]
    public class PrintersController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();

        // GET: /Printers/
        public ActionResult Index()
        {
            ViewBag.Title = "Printers";
            Dictionary<int, PrinterType> PrinterTypes = db.PrinterTypes.ToDictionary(p => p.PrinterTypeId);
            ViewBag.PrinterNames = PrinterTypes;
            Dictionary<long, PrinterStatusLog> PrinterStatus = db.PrinterStatusLogs.SqlQuery(
            "Select dbo.PrinterStatusLogs.* " +
            "From dbo.PrinterStatusLogs " +
            "inner join " +
                "(" +
                "select PrinterStatusLogs.PrinterID, MAX(PrinterStatusLogs.LogEntryDate) as MaxEntryTime " +
                "from dbo.PrinterStatusLogs " +
                "group by dbo.PrinterStatusLogs.PrinterID" +
                ") " +
            "mxe ON dbo.PrinterStatusLogs.LogEntryDate = mxe.MaxEntryTime ").ToDictionary(p => p.PrinterId);
            ViewBag.PrinterStatus = PrinterStatus;
            
            List<Printer> ValidMaterialStatus = db.Printers.SqlQuery(
                "Select dbo.Printers.* " +
                "From dbo.Printers " +
                "inner join " +
                "( " +
                    "select dbo.MaterialCheckouts.PrinterId, Count(Distinct dbo.MaterialCheckouts.MaterialCheckoutId) as MaterialsAssigned " +
                    "from dbo.MaterialCheckouts " +
                    "group by dbo.MaterialCheckouts.PrinterId " +
                    ") cnt ON dbo.Printers.PrinterID = cnt.PrinterId " +
                "inner join dbo.PrinterTypes on dbo.Printers.PrinterTypeId = dbo.PrinterTypes.PrinterTypeId " +
                "where MaterialsAssigned = SupportedNumberMaterials").ToList();

            ViewBag.ValidMaterialStatus = ValidMaterialStatus;

            string PrintAssignmentsQuery = "Select * " +
             "from dbo.PrintEvents " +
             "inner join ( " +
             "select dbo.PrintEvents.PrinterID, MAX(dbo.PrintEvents.EventTimeStamp) as MostReventEvent " +
             "from dbo.PrintEvents " +
             "group by dbo.PrintEvents.PrinterID " +
             ") mxe on dbo.PrintEvents.PrinterID = mxe.PrinterID and dbo.PrintEvents.EventTimeStamp = mxe.MostReventEvent " +
             "where dbo.PrintEvents.EventType = @PrintingEventStart ";

            string PrintStartQuery = "Select dbo.Prints.* " +
                "from dbo.Prints " +
                "inner join (" +
                    "Select dbo.PrintEvents.* " +
            "from dbo.PrintEvents " +
            "inner join ( " +
            "select dbo.PrintEvents.PrintID, MAX(dbo.PrintEvents.EventTimeStamp) as MostReventEvent " +
            "from dbo.PrintEvents " +
            "group by dbo.PrintEvents.PrintID " +
            ") mxe on dbo.PrintEvents.PrintID = mxe.PrintID and dbo.PrintEvents.EventTimeStamp = mxe.MostReventEvent " +
            "where dbo.PrintEvents.EventType = @PrintingEventStart" +
            ") tde on dbo.Prints.PrintId = tde.PrintID ";

            SqlParameter PrintingEventStart = new SqlParameter("@PrintingEventStart", PrintEventType.PRINT_START);
            SqlParameter PrintingEventStart2 = new SqlParameter("@PrintingEventStart", PrintEventType.PRINT_START);
            Dictionary<long, PrintEvent> PrintingAssignments = db.PrintEvents.SqlQuery(PrintAssignmentsQuery, PrintingEventStart).ToDictionary(p => p.PrinterId);
            List<Print> Assigned = db.Prints.SqlQuery(PrintStartQuery, PrintingEventStart2).ToList();

            ViewData["PrintingAssignments"] = PrintingAssignments;
            ViewData["Assigned"] = Assigned;


            return View(db.Printers.ToList());
        }

        // GET: /Printers/Details/5
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
                "Select dbo.Materials.MaterialId, dbo.Materials.MaterialName, dbo.Materials.MaterialSpoolQuantity, dbo.Materials.PrinterTypeId " +
                "FROM dbo.Materials LEFT JOIN dbo.MaterialCheckouts " +
                "ON dbo.Materials.MaterialId = dbo.MaterialCheckouts.MaterialId " +
                "GROUP BY dbo.Materials.MaterialId, dbo.Materials.MaterialName, dbo.Materials.MaterialSpoolQuantity, dbo.Materials.PrinterTypeId " +
                "HAVING ((Count(dbo.MaterialCheckouts.MaterialId) < dbo.Materials.MaterialSpoolQuantity) or dbo.Materials.MaterialSpoolQuantity < 0) and (dbo.Materials.PrinterTypeId = @PrinterTypeID)", Params).ToList();
            ViewData["Materials"] = new SelectList(Materials, "MaterialId", "MaterialName");
            
            string status = "Unknown";
            try
            {
                SqlParameter[] Params1 = { new SqlParameter("@PrinterID", printer.PrinterId) };
                PrinterStatusLog P = db.PrinterStatusLogs.SqlQuery(
                "Select dbo.PrinterStatusLogs.* " +
                "From dbo.PrinterStatusLogs " +
                "inner join " +
                    "(" +
                    "select PrinterStatusLogs.PrinterID, MAX(PrinterStatusLogs.LogEntryDate) as MaxEntryTime " +
                    "from dbo.PrinterStatusLogs " +
                    "group by dbo.PrinterStatusLogs.PrinterID" +
                    ") " +
                "mxe ON dbo.PrinterStatusLogs.LogEntryDate = mxe.MaxEntryTime " +
                "where (dbo.PrinterStatusLogs.PrinterID = @PrinterID)", Params1).First();
                status = string.Concat(P.LoggedPrinterStatus.ToString(), " : ", P.LogEntryDate.ToString("F"));
            }
            catch (Exception e)
            {
                //oh myyyyy
            }
            
            ViewData["Status"] = status;
            
            string AssignedPrintQuery = "Select dbo.Prints.* " +
                "from dbo.Prints " +
                "left outer join " +
                "( " +
                "Select dbo.PrintEvents.PrintID, dbo.PrintEvents.EventType, dbo.PrintEvents.PrinterID " +
                "from dbo.PrintEvents " +
                "inner join " +
                "( " +
                "select dbo.PrintEvents.PrintID, MAX(dbo.PrintEvents.EventTimeStamp) as MostReventEvent " +
                "from dbo.PrintEvents " +
                "group by dbo.PrintEvents.PrintID " +
                ") mxe on dbo.PrintEvents.PrintId = mxe.PrintID and dbo.PrintEvents.EventTimeStamp = mxe.MostReventEvent " +
                ") pnt on dbo.Prints.PrintId = pnt.PrintID " +
                "where EventType = @PrintingEventStart and pnt.PrinterID = @PrinterId";
            SqlParameter PrintingEventStart = new SqlParameter("@PrintingEventStart", PrintEventType.PRINT_START);
            SqlParameter PrinterId = new SqlParameter("@PrinterId", id);
            Print AssignedPrint = null;
            try
            {
                AssignedPrint = db.Prints.SqlQuery(AssignedPrintQuery, PrintingEventStart, PrinterId).Single();
            }
            catch (Exception e)
            {

            }

            ViewBag.AssignedPrint = AssignedPrint;

            return View(printer);
        }

        // GET: /Printers/Create
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
        public ActionResult Create([Bind(Include="PrinterId,PrinterName,PrinterTypeId")] Printer printer)
        {
            if (ModelState.IsValid)
            {
                db.Printers.Add(printer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(printer);
        }

        // GET: /Printers/Edit/5
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
        public ActionResult Edit([Bind(Include="PrinterId,PrinterName,PrinterTypeId")] Printer printer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(printer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(printer);
        }

        // GET: /Printers/Delete/5
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
            return View(printer);
        }

        // POST: /Printers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Printer printer = db.Printers.Find(id);
            db.Printers.Remove(printer);
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
