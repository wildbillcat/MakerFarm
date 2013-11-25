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
    public class PrintsController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();
        //
        // GET: /Prints/

        public ActionResult Index(int id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Index", "PrinterTypes");
            }
            else
            {
                ViewBag.Title = db.PrinterTypes.Where(s => s.PrinterTypeId.Equals(id)).First().TypeName;
                ViewBag.id = id;
                return View(db.Prints.Where(s => s.PrinterTypeId.Equals(id)).ToList());
            }
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
            List<string> materials = new List<string>();
            foreach(string S in print.MaterialIds.Split(',')){
                Material M = db.Materials.Find(long.Parse(S));
                materials.Add(M.MaterialName);
            }
            ViewData["MaterialsList"] = materials;

            return View(print);
        }

        //
        // GET: /Prints/Create
        [Authorize]
        public ActionResult Create(int id = 0)
        {
            if (id == 0)
            {
                //The create function requires that a printer type be passed to it.
                return RedirectToAction("Index", "PrinterTypes");
            }
            PrinterType printerType = db.PrinterTypes.Find(id);
            if(printerType == null)
            {
                //The printer you attempted to use does not exist in the database!
                return RedirectToAction("Index", "PrinterTypes");
            }
            List<Material> materials = db.Materials.Where(s => s.PrinterTypeId.Equals(id) && !s.MaterialSpoolQuantity.Equals(0)).ToList<Material>();
            if(materials.Count() == 0)
            {
                //The printer you attempted to use does not have any materials available
                return RedirectToAction("Index", "Materials");
            }
            ViewData["MaterialsList"] = new SelectList(materials, "MaterialId", "MaterialName");
            List<string> MNUA = new List<string>();
            for(int i = 1; i <= printerType.MaxNumberUserAttempts; i++)
            {
                MNUA.Add(i.ToString());
            }
            ViewData["MaxNumberUserAttempts"] = new SelectList(MNUA);
            ViewData["SupportedMaterials"] = printerType.SupportedNumberMaterials;
            ViewBag.SupportedFileTypes = printerType.SupportedFileTypes;
            ViewData["CurrentUser"] = User.Identity.Name;
            ViewData["PrinterMeasurmentUnit"] = printerType.MaterialUseUnit;
            return View();
        }
        
        //
        // POST: /Prints/Create
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create(FormCollection values, HttpPostedFileBase PrintFile)
        {
            Print print = new Print();
            string saveAsDirectory = string.Concat(AppDomain.CurrentDomain.GetData("DataDirectory"), "\\3DPrints\\", DateTime.Now.ToString("yyyy-MMM-d"));
            print.FileName = PrintFile.FileName;
            print.UserName = values["UserName"];

            /* Material ID Parsing */
            string[] tempMaterial = values.GetValues("MaterialIDs");
            string matIds = tempMaterial[0];
            for (int i = 1; i < tempMaterial.Length; i++ )
            {
                matIds = string.Concat(matIds, ",", tempMaterial[i]);
            }
            print.MaterialIds = matIds;

            /*Estimated Material Usage*/
            print.EstMaterialUse = double.Parse(values["EstMaterialUse"]);

            /*Submission Time*/
            print.SubmissionTime = DateTime.Now;

            /*Estimated Toolpath Time*/
            print.EstToolpathTime = int.Parse(values["EstToolpathTime"]);

            /*Authorized number of attempts*/
            print.AuthorizedAttempts = int.Parse(values["AuthorizedAttempts"]);

            /*Printer Type ID*/
            print.PrinterTypeId = int.Parse(values["PrinterTypeID"]);

            /*Staff Assistance*/
            print.StaffAssistedPrint = false;


            if (ModelState.IsValid)
            {
                if (!System.IO.Directory.Exists(saveAsDirectory))
                {
                    System.IO.Directory.CreateDirectory(saveAsDirectory);
                }

                db.Prints.Add(print);
                db.SaveChanges();
                string printFileName = string.Concat(saveAsDirectory, "\\", print.PrinterTypeId, "_", PrintFile.FileName);
                PrintFile.SaveAs(printFileName);
                return RedirectToAction("Index");
            }

            return View(print);
        }

        //
        // GET: /Prints/Edit/5
        [Authorize]
        public ActionResult Edit(long id = 0)
        {
            Print print = db.Prints.Find(id);
            if (print == null)
            {
                return HttpNotFound();
            }
            List<Material> materials = db.Materials.Where(s => s.PrinterTypeId.Equals(print.PrinterTypeId) && !s.MaterialSpoolQuantity.Equals(0)).ToList<Material>();
            List<SelectList> MaterialsList = new List<SelectList>();
            foreach(string matID in print.MaterialIds.Split(',')){
                MaterialsList.Add(new SelectList(materials, "MaterialId", "MaterialName", materials.Find(p => p.MaterialId.Equals(long.Parse(matID)))));
            }
            ViewData["MaterialsList"] = MaterialsList;

            List<string> MNUA = new List<string>();
            for (int i = 1; i <= print.PrinterType.MaxNumberUserAttempts; i++)
            {
                MNUA.Add(i.ToString());
            }
            ViewData["MaxNumberUserAttempts"] = new SelectList(MNUA, print.AuthorizedAttempts.ToString());
            
            return View(print);
        }

        //
        // POST: /Prints/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormCollection values)
        {
            Print print = db.Prints.Find(long.Parse(values["PrintId"]));
            print.UserName = values["UserName"];

            /* Material ID Parsing */
            string[] tempMaterial = values.GetValues("MaterialIDs");
            string matIds = tempMaterial[0];
            for (int i = 1; i < tempMaterial.Length; i++ )
            {
                matIds = string.Concat(matIds, ",", tempMaterial[i]);
            }
            print.MaterialIds = matIds;

            /*Estimated Material Usage*/
            print.EstMaterialUse = double.Parse(values["EstMaterialUse"]);

            /*Estimated Toolpath Time*/
            print.EstToolpathTime = int.Parse(values["EstToolpathTime"]);

            /*Authorized number of attempts*/
            print.AuthorizedAttempts = int.Parse(values["AuthorizedAttempts"]);

            /*Staff Assistance*/
            print.StaffAssistedPrint = values.Get("StaffAssistedPrint").Contains("true");

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