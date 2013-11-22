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
    public class PrintersController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();

        // GET: /Printers/
        public ActionResult Index()
        {
            ViewBag.Title = "Printers";
            Dictionary<int, PrinterType> PrinterTypes = db.PrinterTypes.ToDictionary(p => p.PrinterTypeId);
            ViewBag.PrinterNames = PrinterTypes;
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
            ViewBag.Title = String.Concat("Details: ", printer.PrinterName);
            SqlParameter[] Params = { new SqlParameter("@PrinterTypeID", printer.PrinterTypeId) };
            List<Material> Materials = db.Materials.SqlQuery(
                "Select dbo.Materials.MaterialId, dbo.Materials.MaterialName, dbo.Materials.MaterialSpoolQuantity, dbo.Materials.PrinterTypeId " +
                "FROM dbo.Materials LEFT JOIN dbo.MaterialCheckouts " +
                "ON dbo.Materials.MaterialId = dbo.MaterialCheckouts.MaterialId " +
                "GROUP BY dbo.Materials.MaterialId, dbo.Materials.MaterialName, dbo.Materials.MaterialSpoolQuantity, dbo.Materials.PrinterTypeId " +
                "HAVING ((Count(dbo.MaterialCheckouts.MaterialId) < dbo.Materials.MaterialSpoolQuantity) or dbo.Materials.MaterialSpoolQuantity < 0) and (dbo.Materials.PrinterTypeId = @PrinterTypeID)", Params).ToList();
            ViewData["Materials"] = new SelectList(Materials, "MaterialId", "MaterialName");
            if (printer == null)
            {
                return HttpNotFound();
            }
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
