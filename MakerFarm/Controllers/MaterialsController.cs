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
    [Authorize(Roles = "Administrator, Moderator")]
    public class MaterialsController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();

        // GET: /Materials/
        public ActionResult Index()
        {
            Dictionary<int, PrinterType> PrinterTypes = db.PrinterTypes.ToDictionary(p => p.PrinterTypeId);
            ViewBag.PrinterNames = PrinterTypes;
            return View(db.Materials.ToList());
        }

        // GET: /Materials/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Material material = db.Materials.Find(id);
            if (material == null)
            {
                return HttpNotFound();
            }
            return View(material);
        }

        // GET: /Materials/Create
        [Authorize(Roles = "Administrator")]
        public ActionResult Create()
        {
            List<PrinterType> printerType = db.PrinterTypes.ToList();
            ViewData["PrintersList"] = new SelectList(printerType, "PrinterTypeId", "TypeName");
            if (printerType == null)
            {
                //The printer you attempted to use does not exist in the database!
                return RedirectToAction("Index", "PrinterTypes");
            }
            return View();
        }

        // POST: /Materials/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Create([Bind(Include="MaterialId,MaterialName,PrinterTypeId,MaterialSpoolQuantity")] Material material)
        {
            if (ModelState.IsValid)
            {
                db.Materials.Add(material);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(material);
        }

        // GET: /Materials/Edit/5
        public ActionResult UpdateQuantity(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Material material = db.Materials.Find(id);
            if (material == null)
            {
                return HttpNotFound();
            }
            return View(material);
        }

        // POST: /Materials/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(FormCollection values)
        {
            Material material = db.Materials.Find(int.Parse(values["MaterialId"]));
            try
            {
                material.MaterialSpoolQuantity = int.Parse(values["MaterialSpoolQuantity"]);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("You didn't enter a bloody INTEGER!", new Exception("You didn't enter a bloody INTEGER!"));
            }
            if (ModelState.IsValid)
            {
                db.Entry(material).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(material);
        }

        // GET: /Materials/Edit/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Material material = db.Materials.Find(id);
            if (material == null)
            {
                return HttpNotFound();
            }
            List<PrinterType> printerType = db.PrinterTypes.ToList();
            ViewData["PrintersList"] = new SelectList(printerType, "PrinterTypeId", "TypeName");
            return View(material);
        }

        // POST: /Materials/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit([Bind(Include = "MaterialId,MaterialName,PrinterTypeId,MaterialSpoolQuantity")] Material material)
        {
            if (ModelState.IsValid)
            {
                db.Entry(material).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(material);
        }

        // GET: /Materials/Delete/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Material material = db.Materials.Find(id);
            if (material == null)
            {
                return HttpNotFound();
            }
            return View(material);
        }

        // POST: /Materials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult DeleteConfirmed(long id)
        {
            Material material = db.Materials.Find(id);
            db.Materials.Remove(material);
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
