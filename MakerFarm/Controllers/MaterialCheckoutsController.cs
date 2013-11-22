using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MakerFarm.Models;

namespace MakerFarm.Controllers
{
    public class MaterialCheckoutsController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();

        // GET: /MaterialCheckouts/
        public ActionResult Index()
        {
            var materialcheckouts = db.MaterialCheckouts.Include(m => m.Material).Include(m => m.Printer);
            return View(materialcheckouts.ToList());
        }

        // GET: /MaterialCheckouts/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MaterialCheckout materialcheckout = db.MaterialCheckouts.Find(id);
            if (materialcheckout == null)
            {
                return HttpNotFound();
            }
            return View(materialcheckout);
        }

        // GET: /MaterialCheckouts/Create
        public ActionResult Create()
        {
            ViewBag.MaterialId = new SelectList(db.Materials, "MaterialId", "MaterialName");
            ViewBag.PrinterId = new SelectList(db.Printers, "PrinterId", "PrinterName");
            return View();
        }

        // POST: /MaterialCheckouts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="MaterialCheckoutId,PrinterId,MaterialId")] MaterialCheckout materialcheckout)
        {
            if (ModelState.IsValid)
            {
                db.MaterialCheckouts.Add(materialcheckout);
                db.SaveChanges();
                return RedirectToAction("Details", "Printers", new { id = materialcheckout.PrinterId });
            }

            ViewBag.MaterialId = new SelectList(db.Materials, "MaterialId", "MaterialName", materialcheckout.MaterialId);
            ViewBag.PrinterId = new SelectList(db.Printers, "PrinterId", "PrinterName", materialcheckout.PrinterId);
            return View(materialcheckout);
        }

        // GET: /MaterialCheckouts/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MaterialCheckout materialcheckout = db.MaterialCheckouts.Find(id);
            if (materialcheckout == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaterialId = new SelectList(db.Materials, "MaterialId", "MaterialName", materialcheckout.MaterialId);
            ViewBag.PrinterId = new SelectList(db.Printers, "PrinterId", "PrinterName", materialcheckout.PrinterId);
            return View(materialcheckout);
        }

        // POST: /MaterialCheckouts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="MaterialCheckoutId,PrinterId,MaterialId")] MaterialCheckout materialcheckout)
        {
            if (ModelState.IsValid)
            {
                db.Entry(materialcheckout).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MaterialId = new SelectList(db.Materials, "MaterialId", "MaterialName", materialcheckout.MaterialId);
            ViewBag.PrinterId = new SelectList(db.Printers, "PrinterId", "PrinterName", materialcheckout.PrinterId);
            return View(materialcheckout);
        }

        // GET: /MaterialCheckouts/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MaterialCheckout materialcheckout = db.MaterialCheckouts.Find(id);
            if (materialcheckout == null)
            {
                return HttpNotFound();
            }
            return View(materialcheckout);
        }

        // POST: /MaterialCheckouts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            MaterialCheckout materialcheckout = db.MaterialCheckouts.Find(id);
            db.MaterialCheckouts.Remove(materialcheckout);
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
