using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MakerFarm.Models;

namespace MakerFarm.Controllers
{
    public class PrinterTypesController : Controller
    {
        private PrinterTypeDBContext db = new PrinterTypeDBContext();

        //
        // GET: /PrinterTypes/

        public ActionResult Index()
        {
            return View(db.PrinterTypes.ToList());
        }

        //
        // GET: /PrinterTypes/Details/5

        public ActionResult Details(int id = 0)
        {
            PrinterType printertype = db.PrinterTypes.Find(id);
            if (printertype == null)
            {
                return HttpNotFound();
            }
            return View(printertype);
        }

        //
        // GET: /PrinterTypes/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /PrinterTypes/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PrinterType printertype)
        {
            if (ModelState.IsValid)
            {
                db.PrinterTypes.Add(printertype);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(printertype);
        }

        //
        // GET: /PrinterTypes/Edit/5

        public ActionResult Edit(int id = 0)
        {
            PrinterType printertype = db.PrinterTypes.Find(id);
            if (printertype == null)
            {
                return HttpNotFound();
            }
            return View(printertype);
        }

        //
        // POST: /PrinterTypes/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PrinterType printertype)
        {
            if (ModelState.IsValid)
            {
                db.Entry(printertype).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(printertype);
        }

        //
        // GET: /PrinterTypes/Delete/5

        public ActionResult Delete(int id = 0)
        {
            PrinterType printertype = db.PrinterTypes.Find(id);
            if (printertype == null)
            {
                return HttpNotFound();
            }
            return View(printertype);
        }

        //
        // POST: /PrinterTypes/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PrinterType printertype = db.PrinterTypes.Find(id);
            db.PrinterTypes.Remove(printertype);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}