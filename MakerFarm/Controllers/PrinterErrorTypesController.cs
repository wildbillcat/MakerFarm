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
    public class PrinterErrorTypesController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();

        // GET: /PrinterErrorTypes/
        public ActionResult Index()
        {
            return View(db.PrintErrorTypes.ToList());
        }

        // GET: /PrinterErrorTypes/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrintErrorType printerrortype = db.PrintErrorTypes.Find(id);
            if (printerrortype == null)
            {
                return HttpNotFound();
            }
            return View(printerrortype);
        }

        // GET: /PrinterErrorTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /PrinterErrorTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="PrintErrorTypeId,PrintErrorName,UserError")] PrintErrorType printerrortype)
        {
            if (ModelState.IsValid)
            {
                db.PrintErrorTypes.Add(printerrortype);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(printerrortype);
        }

        // GET: /PrinterErrorTypes/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrintErrorType printerrortype = db.PrintErrorTypes.Find(id);
            if (printerrortype == null)
            {
                return HttpNotFound();
            }
            return View(printerrortype);
        }

        // POST: /PrinterErrorTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="PrintErrorTypeId,PrintErrorName,UserError")] PrintErrorType printerrortype)
        {
            if (ModelState.IsValid)
            {
                db.Entry(printerrortype).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(printerrortype);
        }

        // GET: /PrinterErrorTypes/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrintErrorType printerrortype = db.PrintErrorTypes.Find(id);
            if (printerrortype == null)
            {
                return HttpNotFound();
            }
            return View(printerrortype);
        }

        // POST: /PrinterErrorTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            PrintErrorType printerrortype = db.PrintErrorTypes.Find(id);
            db.PrintErrorTypes.Remove(printerrortype);
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
