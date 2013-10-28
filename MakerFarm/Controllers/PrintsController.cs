﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MakerFarm.Models;

namespace MakerFarm.Controllers
{
    public class PrintsController : Controller
    {
        private PrintDBContext db = new PrintDBContext();

        //
        // GET: /Prints/

        public ActionResult Index()
        {
            return View(db.Prints.ToList());
        }

        //
        // GET: /Prints/Details/5

        public ActionResult Details(long id = 0)
        {
            Print print = db.Prints.Find(id);
            if (print == null)
            {
                return HttpNotFound();
            }
            return View(print);
        }

        //
        // GET: /Prints/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Prints/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Print print)
        {
            if (ModelState.IsValid)
            {
                db.Prints.Add(print);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(print);
        }

        //
        // GET: /Prints/Edit/5

        public ActionResult Edit(long id = 0)
        {
            Print print = db.Prints.Find(id);
            if (print == null)
            {
                return HttpNotFound();
            }
            return View(print);
        }

        //
        // POST: /Prints/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Print print)
        {
            if (ModelState.IsValid)
            {
                db.Entry(print).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(print);
        }

        //
        // GET: /Prints/Delete/5

        public ActionResult Delete(long id = 0)
        {
            Print print = db.Prints.Find(id);
            if (print == null)
            {
                return HttpNotFound();
            }
            return View(print);
        }

        //
        // POST: /Prints/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Print print = db.Prints.Find(id);
            db.Prints.Remove(print);
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