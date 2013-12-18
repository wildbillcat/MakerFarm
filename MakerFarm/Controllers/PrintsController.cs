using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MakerFarm.Models;
using System.Data.SqlClient;

namespace MakerFarm.Controllers
{
    [Authorize]
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
                //Need to edit it to pull prints 
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
            ") tde on dbo.Prints.PrintId = tde.PrintID " +
            "where dbo.Prints.PrinterTypeID = @PrinterTypeID";
                string WaitingPrintFilesQuery = "Select dbo.Prints.* " +
                "from dbo.Prints " +
                "left outer join " +
                "( " +
                "Select dbo.PrintEvents.PrintID, dbo.PrintEvents.EventType " +
                "from dbo.PrintEvents " +
                "inner join " +
                "( " +
                "select dbo.PrintEvents.PrintID, MAX(dbo.PrintEvents.EventTimeStamp) as MostReventEvent " +
                "from dbo.PrintEvents " +
                "group by dbo.PrintEvents.PrintID " +
                ") mxe on dbo.PrintEvents.PrintId = mxe.PrintID and dbo.PrintEvents.EventTimeStamp = mxe.MostReventEvent " +
                ") pnt on dbo.Prints.PrintId = pnt.PrintID " +
                "where (pnt.EventType is null or pnt.EventType = @PrintingEventFile or pnt.EventType = @PrintingEventMachine) and dbo.Prints.PrinterTypeID = @PrinterTypeID";
                string PrintAssignmentsQuery = "Select * " +
             "from dbo.PrintEvents " +
             "inner join ( " +
             "select dbo.PrintEvents.PrintID, MAX(dbo.PrintEvents.EventTimeStamp) as MostReventEvent " +
             "from dbo.PrintEvents " +
             "group by dbo.PrintEvents.PrintID " +
             ") mxe on dbo.PrintEvents.PrintID = mxe.PrintID and dbo.PrintEvents.EventTimeStamp = mxe.MostReventEvent " +
             "where dbo.PrintEvents.EventType = @PrintingEventStart ";
                SqlParameter PrintingEventStart = new SqlParameter("@PrintingEventStart", PrintEventType.PRINT_START);
                SqlParameter PrintingEventStart2 = new SqlParameter("@PrintingEventStart", PrintEventType.PRINT_START);
                SqlParameter PrintingEventFile = new SqlParameter("@PrintingEventFile", PrintEventType.PRINT_FAILURE_FILE);
                SqlParameter PrintingEventMachine = new SqlParameter("@PrintingEventMachine", PrintEventType.PRINT_FAILURE_MACHINE);
                SqlParameter PrinterTypeId = new SqlParameter("@PrinterTypeID", id);
                SqlParameter PrinterTypeId2 = new SqlParameter("@PrinterTypeID", id);
                ViewBag.Title = db.PrinterTypes.Where(s => s.PrinterTypeId.Equals(id)).First().TypeName;
                ViewBag.id = id;
                Dictionary<long, PrintEvent> PrintingAssignments = db.PrintEvents.SqlQuery(PrintAssignmentsQuery, PrintingEventStart).ToDictionary(p => p.PrintId);
                ViewBag.PrintingAssignments = PrintingAssignments;
                List<Print> Assigned = db.Prints.SqlQuery(PrintStartQuery, PrintingEventStart2, PrinterTypeId).ToList();
                Assigned.OrderBy(p => p.SubmissionTime);
                ViewBag.Assigned = Assigned;//Print Start Query
                List<Print> Waiting = db.Prints.SqlQuery(WaitingPrintFilesQuery, PrintingEventFile, PrintingEventMachine, PrinterTypeId2).ToList();
                Waiting.OrderBy(p => p.SubmissionTime);
                return View(Waiting);
            }
        }

        //
        // GET: /Prints/Completed
        public ActionResult Completed(int id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Index", "PrinterTypes");
            }
            else
            {
                //Need to edit it to pull prints 
                string CompleteFilesQuery = "Select dbo.Prints.* " +
                "from dbo.Prints " +
                "left outer join " +
                "( " +
                "Select dbo.PrintEvents.PrintID, dbo.PrintEvents.EventType " +
                "from dbo.PrintEvents " +
                "inner join " +
                "( " +
                "select dbo.PrintEvents.PrintID, MAX(dbo.PrintEvents.EventTimeStamp) as MostReventEvent " +
                "from dbo.PrintEvents " +
                "group by dbo.PrintEvents.PrintID " +
                ") mxe on dbo.PrintEvents.PrintId = mxe.PrintID and dbo.PrintEvents.EventTimeStamp = mxe.MostReventEvent " +
                ") pnt on dbo.Prints.PrintId = pnt.PrintID " +
                "where (pnt.EventType = @PrintingEventCompleted or pnt.EventType = @PrintingEventCanceled) and dbo.Prints.PrinterTypeID = @PrinterTypeID";
                string PrintAssignmentsQuery = "Select * " +
             "from dbo.PrintEvents " +
             "inner join ( " +
             "select dbo.PrintEvents.PrintID, MAX(dbo.PrintEvents.EventTimeStamp) as MostReventEvent " +
             "from dbo.PrintEvents " +
             "group by dbo.PrintEvents.PrintID " +
             ") mxe on dbo.PrintEvents.PrintID = mxe.PrintID and dbo.PrintEvents.EventTimeStamp = mxe.MostReventEvent " +
             "where dbo.PrintEvents.EventType = @PrintingEventCompleted or dbo.PrintEvents.EventType = @PrintingEventCanceled";
                SqlParameter PrintingEventCompleted = new SqlParameter("@PrintingEventCompleted", PrintEventType.PRINT_COMPLETED);
                SqlParameter PrintingEventCanceled = new SqlParameter("@PrintingEventCanceled", PrintEventType.PRINT_CANCELED);
                SqlParameter PrinterTypeId = new SqlParameter("@PrinterTypeID", id);
                SqlParameter PrintingEventCompleted2 = new SqlParameter("@PrintingEventCompleted", PrintEventType.PRINT_COMPLETED);
                SqlParameter PrintingEventCanceled2 = new SqlParameter("@PrintingEventCanceled", PrintEventType.PRINT_CANCELED);
                ViewBag.Title = db.PrinterTypes.Where(s => s.PrinterTypeId.Equals(id)).First().TypeName;
                ViewBag.id = id;
                Dictionary<long, PrintEvent> PrintingAssignments = db.PrintEvents.SqlQuery(PrintAssignmentsQuery, PrintingEventCompleted2, PrintingEventCanceled2).ToDictionary(p => p.PrintId);
                ViewBag.PrintingAssignments = PrintingAssignments;
                List<Print> Waiting = db.Prints.SqlQuery(CompleteFilesQuery, PrintingEventCompleted, PrintingEventCanceled, PrinterTypeId).ToList();
                Waiting.OrderBy(p => p.SubmissionTime);
                return View(Waiting);
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
            ViewData["DownloadAvailable"] = System.IO.File.Exists(string.Concat(AppDomain.CurrentDomain.GetData("DataDirectory"), "\\3DPrints\\", print.SubmissionTime.ToString("yyyy-MMM-d"), "\\", print.PrintId, "_", print.FileName)) || System.IO.File.Exists(string.Concat(AppDomain.CurrentDomain.GetData("DataDirectory"), "\\3DPrints\\", print.SubmissionTime.ToString("yyyy-MMM-d"), "\\", print.PrintId, "_", print.FileName));
            ViewData["MaterialsList"] = materials;

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
            PrinterType printerType = db.PrinterTypes.Find(id);
            if(printerType == null)
            {
                //The printer you attempted to use does not exist in the database!
                return RedirectToAction("Index", "PrinterTypes");
            }
            List<Material> materials = db.Materials.Where(s => (s.PrinterTypeId == id) && !(s.MaterialSpoolQuantity == 0)).ToList<Material>();
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
            if (printerType.CommentField == null)
            {
                ViewData["PrinterComment"] = "";
            }
            else
            {
                ViewData["PrinterComment"] = printerType.CommentField;
            }
            ViewData["MNUA"] = MNUA.Count();
            ViewData["MaxNumberUserAttempts"] = new SelectList(MNUA);
            ViewData["SupportedMaterials"] = printerType.SupportedNumberMaterials;
            ViewBag.SupportedFileTypes = printerType.SupportedFileTypes;
            ViewData["CurrentUser"] = User.Identity.Name;
            ViewData["PrinterMeasurmentUnit"] = printerType.MaterialUseUnit;
            ViewData["PrintSubmissionWaiverTerms"] = db.PrintSubmissionWaiverTerms.Where(p => p.Enabled.Equals(true)).ToList();
            return View();
        }
        
        //
        // POST: /Prints/Create
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection values, HttpPostedFileBase PrintFile)
        {
            Print print = new Print();
            string saveAsDirectory = string.Concat(AppDomain.CurrentDomain.GetData("DataDirectory"), "\\3DPrints\\", DateTime.Now.ToString("yyyy-MMM-d"));
            print.FileName = PrintFile.FileName;
            print.UserName = values["UserName"];
            print.FlaggedPrint = false;
            print.FlaggedComment = "";

            /* Material ID Parsing */
            string[] tempMaterial = values.GetValues("MaterialIDs");
            string matIds = tempMaterial[0];
            for (int i = 1; i < tempMaterial.Length; i++ )
            {
                matIds = string.Concat(matIds, ",", tempMaterial[i]);
            }
            print.MaterialIds = matIds;

            /*Estimated Material Usage*/
            try { 
            print.EstMaterialUse = double.Parse(values["EstMaterialUse"]);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("EstMaterialUse", e);
            }
            /*Submission Time*/
            print.SubmissionTime = DateTime.Now;

            /*Estimated Toolpath Time*/
            try
            {
                print.EstToolpathTime = int.Parse(values["EstToolpathTime"]);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("EstToolpathTime", e);
            }

            /*Authorized number of attempts*/
            print.AuthorizedAttempts = int.Parse(values["AuthorizedAttempts"]);

            /*Printer Type ID*/
            print.PrinterTypeId = int.Parse(values["PrinterTypeID"]);

            /*Staff Assistance*/
            print.StaffAssistedPrint = false;

            print.Comment = values.Get("Comment");

            try { 
                int TotalWaiverConditions = int.Parse(values["PrintSubmissionWaiverTermQt"]);
                int AcceptedWaiverConditions = values.GetValues("PrintSubmissionWaiverTerm").Where(p => p.Equals("I Agree")).Count();
                if (TotalWaiverConditions != AcceptedWaiverConditions)
                {
                    ModelState.AddModelError("PrintSubmissionWaiverTerm", new Exception("You must agree to all of the terms in order to submit a print"));
                    ViewData["Waiver"] = true;
                }
            }
            catch (Exception e) { }
            if (ModelState.IsValid)
            {
                if (!System.IO.Directory.Exists(saveAsDirectory))
                {
                    System.IO.Directory.CreateDirectory(saveAsDirectory);
                }

                db.Prints.Add(print);
                db.SaveChanges();
                string printFileName = string.Concat(saveAsDirectory, "\\", print.PrintId, "_", PrintFile.FileName);
                PrintFile.SaveAs(printFileName);
                return RedirectToAction("Details", new { id = print.PrintId });
            }
            long id = print.PrinterTypeId;
            PrinterType printerType = db.PrinterTypes.Find(id);
            if (printerType == null)
            {
                //The printer you attempted to use does not exist in the database!
                return RedirectToAction("Index", "PrinterTypes");
            }
            List<Material> materials = db.Materials.Where(s => (s.PrinterTypeId == id) && !(s.MaterialSpoolQuantity == 0)).ToList();
            if (materials.Count() == 0)
            {
                //The printer you attempted to use does not have any materials available
                return RedirectToAction("Index", "Materials");
            }
            ViewData["MaterialsList"] = new SelectList(materials, "MaterialId", "MaterialName");
            List<string> MNUA = new List<string>();
            for (int i = 1; i <= printerType.MaxNumberUserAttempts; i++)
            {
                MNUA.Add(i.ToString());
            }
            if (printerType.CommentField == null)
            {
                ViewData["PrinterComment"] = "";
            }
            else
            {
                ViewData["PrinterComment"] = printerType.CommentField;
            }
            ViewData["MNUA"] = MNUA.Count();
            ViewData["MaxNumberUserAttempts"] = new SelectList(MNUA);
            ViewData["SupportedMaterials"] = printerType.SupportedNumberMaterials;
            ViewBag.SupportedFileTypes = printerType.SupportedFileTypes;
            ViewData["CurrentUser"] = User.Identity.Name;
            ViewData["PrinterMeasurmentUnit"] = printerType.MaterialUseUnit;
            ViewData["PrintSubmissionWaiverTerms"] = db.PrintSubmissionWaiverTerms.Where(p => p.Enabled.Equals(true)).ToList();
            
            return View(print);
        }

        //
        // GET: /Prints/Edit/5
        [Authorize(Roles = "Administrator, Moderator")]
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
        [Authorize(Roles = "Administrator, Moderator")]
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

            print.Comment = values.Get("Comment");

            print.FlaggedPrint = values.Get("FlaggedPrint").Contains("true");

            if (print.FlaggedPrint)
            {
                print.FlaggedComment = values.Get("FlaggedComment");
            }
            string OriginalPath = string.Concat(AppDomain.CurrentDomain.GetData("DataDirectory"), "\\3DPrints\\", print.SubmissionTime.ToString("yyyy-MMM-d"), "\\", print.PrintId, "_", print.FileName);
            string FlaggedPath = string.Concat(AppDomain.CurrentDomain.GetData("DataDirectory"), "\\Flagged\\", print.SubmissionTime.ToString("yyyy-MMM-d"), "\\", print.PrintId, "_", print.FileName);
            if (System.IO.File.Exists(OriginalPath) && !System.IO.File.Exists(FlaggedPath) && print.FlaggedPrint)
            {
                string saveAsDirectory = string.Concat(AppDomain.CurrentDomain.GetData("DataDirectory"), "\\Flagged\\", print.SubmissionTime.ToString("yyyy-MMM-d"));
                if (!System.IO.Directory.Exists(saveAsDirectory))
                {
                    System.IO.Directory.CreateDirectory(saveAsDirectory);
                }
                System.IO.File.Copy(OriginalPath, FlaggedPath);
            }
            else if (System.IO.File.Exists(FlaggedPath) && !print.FlaggedPrint)
            {
                System.IO.File.Delete(FlaggedPath);
            }

            if (ModelState.IsValid)
            {
                db.Entry(print).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = print.PrintId });
            }
            return View(print);
        }

        //
        // GET: /Prints/Delete/5
        [Authorize(Roles = "Administrator")]
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
        [Authorize(Roles = "Administrator")]
        public ActionResult DeleteConfirmed(long id)
        {
            Print print = db.Prints.Find(id);
            long pid = print.PrinterTypeId;
            string path = string.Concat(AppDomain.CurrentDomain.GetData("DataDirectory"), "\\3DPrints\\", print.SubmissionTime.ToString("yyyy-MMM-d"), "\\", print.PrintId, "_", print.FileName);
            if(System.IO.File.Exists(path)){
                System.IO.File.Delete(path);
            }
            db.Prints.Remove(print);
            List<PrintEvent> AssociatedEvents = db.PrintEvents.Where(p => p.PrintId.Equals(id)).ToList();
            foreach(PrintEvent AssociatedEvent in AssociatedEvents){
                db.PrintEvents.Remove(AssociatedEvent);
            }
            db.SaveChanges();

            return RedirectToAction("Index", new { id = pid });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [Authorize(Roles = "Administrator, Moderator")]
        public ActionResult Download(long id)
        {
            Print print = db.Prints.Find(id);
            if (print == null)
            {
                return HttpNotFound();
            }
            string path = string.Concat(AppDomain.CurrentDomain.GetData("DataDirectory"), "\\3DPrints\\", print.SubmissionTime.ToString("yyyy-MMM-d"), "\\", print.PrintId, "_", print.FileName);
            string flaggedPath = string.Concat(AppDomain.CurrentDomain.GetData("DataDirectory"), "\\3DPrints\\", print.SubmissionTime.ToString("yyyy-MMM-d"), "\\", print.PrintId, "_", print.FileName);
            var contentType = "text/plain";
            if (!System.IO.File.Exists(path))
            {
                return File(path, contentType, string.Concat(print.PrintId, "_", print.FileName));
            }
            else if (!System.IO.File.Exists(flaggedPath))
            {
                return File(flaggedPath, contentType, string.Concat(print.PrintId, "_", print.FileName));
            }
            return HttpNotFound();
            
        }
    }
}