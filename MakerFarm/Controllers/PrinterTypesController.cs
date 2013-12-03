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
    [Authorize]
    public class PrinterTypesController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();

        // GET: /PrinterTypes/
        public ActionResult Index()
        {
            return View(db.PrinterTypes.ToList());
        }

        // GET: /PrinterTypes/Administration
        [Authorize(Roles = "Administrator")]
        public ActionResult Administration()
        {
            return View(db.PrinterTypes.ToList());
        }

        // GET: /PrinterTypes/Details/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrinterType printertype = db.PrinterTypes.Find(id);
            if (printertype == null)
            {
                return HttpNotFound();
            }
            return View(printertype);
        }

        // GET: /PrinterTypes/Create
        [Authorize(Roles = "Administrator")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: /PrinterTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]

        public ActionResult Create([Bind(Include = "PrinterTypeId,TypeName,SupportedNumberMaterials,MaterialUseUnit,MaxNumberUserAttempts,SupportedFileTypes,CommentField")] PrinterType printertype, HttpPostedFileBase IconFile)
        {
            string saveAsDirectory = "~/Content/3DPrinterIcons/";
            if (0 == IconFile.ContentLength)
            {
                ModelState.AddModelError("PrinterIcon", "An Icon must be attached for Display");
            }
            printertype.PrinterIcon = "TempPath";
            if (ModelState.IsValid)
            {
                if (!System.IO.Directory.Exists(Server.MapPath(saveAsDirectory)))
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath(saveAsDirectory));
                }
                db.PrinterTypes.Add(printertype);
                db.SaveChanges();
                string printerIconPath = string.Concat(saveAsDirectory, printertype.PrinterTypeId, "-", IconFile.FileName);
                IconFile.SaveAs(Server.MapPath(printerIconPath));
                printertype.PrinterIcon = printerIconPath;
                db.Entry(printertype).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Administration");
            }

            return View(printertype);
        }

        // GET: /PrinterTypes/Edit/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrinterType printertype = db.PrinterTypes.Find(id);
            if (printertype == null)
            {
                return HttpNotFound();
            }
            return View(printertype);
        }

        // POST: /PrinterTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit([Bind(Include = "PrinterTypeId,TypeName,SupportedNumberMaterials,MaterialUseUnit,PrinterIcon,MaxNumberUserAttempts,SupportedFileTypes,CommentField")] PrinterType printertype)
        {
            if (ModelState.IsValid)
            {
                db.Entry(printertype).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Administration");
            }
            return View(printertype);
        }

        // GET: /PrinterTypes/Delete/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrinterType printertype = db.PrinterTypes.Find(id);
            if (printertype == null)
            {
                return HttpNotFound();
            }
            return View(printertype);
        }

        // POST: /PrinterTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult DeleteConfirmed(int id)
        {
            PrinterType printertype = db.PrinterTypes.Find(id);
            db.PrinterTypes.Remove(printertype);
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
