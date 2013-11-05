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
        private PrinterTypeDBContext pdb = new PrinterTypeDBContext();
        private MaterialDBContext mdb = new MaterialDBContext();
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

        public ActionResult Create(int id = 0)
        {
            if (id == 0)
            {
                //The create function requires that a printer type be passed to it.
                return RedirectToAction("Index", "PrinterTypes");
            }
            PrinterType printerType = pdb.PrinterTypes.Find(id);
            if(printerType == null)
            {
                //The printer you attempted to use does not exist in the database!
                return RedirectToAction("Index", "PrinterTypes");
            }
            List<Material> materials = mdb.Materials.Where(s => s.PrinterTypeId.Equals(id) && !s.MaterialSpoolQuantity.Equals(0)).ToList<Material>();
            if(materials.Count() == 0)
            {
                //The printer you attempted to use does not have any materials available
                return RedirectToAction("Index", "Materials");
            }
            ViewData["MaterialsList"] = new SelectList(materials, "Id", "MaterialName");
            List<string> MNUA = new List<string>();
            for(int i = 1; i <= printerType.MaxNumberUserAttempts; i++)
            {
                MNUA.Add(i.ToString());
            }
            ViewData["MaxNumberUserAttempts"] = new SelectList(MNUA);
            ViewData["SupportedMaterials"] = printerType.SupportedNumberMaterials;
            ViewBag.SupportedFileTypes = printerType.SupportedFileTypes;
            ViewData["CurrentUser"] = User.Identity.Name;
            return View();
        }
        
        //
        // POST: /Prints/Create
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection values, HttpPostedFileBase PrintFile)
        {
            Print print = new Print();

            print.FileName = PrintFile.FileName;
            print.UserID = values["UserID"];
            print.SMBPath = "path";

            /* Material ID Parsing */
            string[] tempMaterial = values.GetValues("MaterialIds");
            long[] matIds = new long[tempMaterial.Length];
            for (int i = 0; i < tempMaterial.Length; i++ )
            {
                matIds[i] = long.Parse(tempMaterial[i]);
            }
            print.MaterialIDs = matIds;

            /*Estimated Material Usage*/
            print.EstMaterialUse = double.Parse(values["EstMaterialUse"]);

            /*Submission Time*/
            print.SubmissionTime = DateTime.Now;

            /*Estimated Toolpath Time*/
            print.EstToolpathTime = int.Parse(values["EstToolpathTime"]);

            /*Authorized number of attempts*/
            print.AuthorizedAttempts = int.Parse(values["AuthorizedAttempts"]);

            /*Printer Type ID*/
            print.PrinterTypeID = int.Parse(values["PrinterTypeID"]);

            /*Staff Assistance*/
            print.StaffAssitedPrint = false;


            if (ModelState.IsValid)
            {
                db.Prints.Add(print);
                db.SaveChanges();
                PrintFile.SaveAs(string.Concat(System.Configuration.ConfigurationManager.AppSettings["RedirectURL"], print.Id, "_", PrintFile.FileName));
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