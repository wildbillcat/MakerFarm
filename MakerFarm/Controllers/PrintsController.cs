using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MakerFarm.Models;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.DirectoryServices.AccountManagement;
using System.Text;
using System.Configuration;
using PaperCutMF;
using PagedList;

namespace MakerFarm.Controllers
{
    [Authorize]
    public class PrintsController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();
        private ServerCommandProxy PapercutServerProxy = new ServerCommandProxy(System.Configuration.ConfigurationManager.AppSettings.Get("PapercutServerDNS"), int.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("PapercutPort")), System.Configuration.ConfigurationManager.AppSettings.Get("PaperCutAuthToken"));
        // set up domain context
        

        //
        // GET: /Prints/
        public ActionResult Index(int id = 0)
        {
            PrinterType PType = db.PrinterTypes.Find(id);
            if (PType == null)
            {
                return HttpNotFound();
            }
            ViewData["PType"] = PType;
            return View();
        }

        //Partial
        public ActionResult PrintQueue(int id = 0)
        {
            PrinterType Type = db.PrinterTypes.Find(id);
            if (Type == null)
            {
                return HttpNotFound();
            }
            string UnstartedPrintsSQL = "Select dbo.Prints.* " +
                "from dbo.Prints " +
                "left outer join PrintEvents " +
                "on dbo.Prints.PrintId = PrintEvents.PrintID " +
                "where dbo.PrintEvents.PrinterID IS NULL and dbo.Prints.TermsAndConditionsAgreement IS NOT NULL ";
            Dictionary<long, Print> UnstartedCancelEligiblePrints = db.Prints.SqlQuery(UnstartedPrintsSQL).ToDictionary(p => p.PrintId);
            ViewData["UnstartedCancelEligiblePrints"] = UnstartedCancelEligiblePrints;
            //Need to edit it to pull prints 
            string PrintStartQuery = "Select dbo.Prints.* " +
            "from dbo.Prints " +
            "inner join (" +
                "Select dbo.PrintEvents.* " +
        "from dbo.PrintEvents " +
        "inner join ( " +
        "select dbo.PrintEvents.PrintID, MAX(dbo.PrintEvents.PrintEventId) as MostReventEvent " +
        "from dbo.PrintEvents " +
        "group by dbo.PrintEvents.PrintID " +
        ") mxe on dbo.PrintEvents.PrintID = mxe.PrintID and dbo.PrintEvents.PrintEventId = mxe.MostReventEvent " +
        "where dbo.PrintEvents.EventType = @PrintingEventStart" +
        ") tde on dbo.Prints.PrintId = tde.PrintID " +
        "where dbo.Prints.PrinterTypeID = @PrinterTypeID and dbo.Prints.TermsAndConditionsAgreement IS NOT NULL " +
        "Order by dbo.Prints.TermsAndConditionsAgreement";
            string WaitingPrintFilesQuery = "Select dbo.Prints.* " +
            "from dbo.Prints " +
            "left outer join " +
            "( " +
            "Select dbo.PrintEvents.PrintID, dbo.PrintEvents.EventType " +
            "from dbo.PrintEvents " +
            "inner join " +
            "( " +
            "select dbo.PrintEvents.PrintID, MAX(dbo.PrintEvents.PrintEventId) as MostReventEvent " +
            "from dbo.PrintEvents " +
            "group by dbo.PrintEvents.PrintID " +
            ") mxe on dbo.PrintEvents.PrintId = mxe.PrintID and dbo.PrintEvents.PrintEventId = mxe.MostReventEvent " +
            ") pnt on dbo.Prints.PrintId = pnt.PrintID " +
            "where (pnt.EventType is null or pnt.EventType = @PrintingEventFile or pnt.EventType = @PrintingEventMachine) and dbo.Prints.PrinterTypeID = @PrinterTypeID and dbo.Prints.TermsAndConditionsAgreement IS NOT NULL " +
            "Order by dbo.Prints.TermsAndConditionsAgreement";
            SqlParameter PrintingEventStart = new SqlParameter("@PrintingEventStart", PrintEventType.PRINT_START);
            SqlParameter PrintingEventStart2 = new SqlParameter("@PrintingEventStart", PrintEventType.PRINT_START);
            SqlParameter PrintingEventFile = new SqlParameter("@PrintingEventFile", PrintEventType.PRINT_FAILURE_FILE);
            SqlParameter PrintingEventMachine = new SqlParameter("@PrintingEventMachine", PrintEventType.PRINT_FAILURE_MACHINE);
            SqlParameter PrinterTypeId = new SqlParameter("@PrinterTypeID", id);
            SqlParameter PrinterTypeId2 = new SqlParameter("@PrinterTypeID", id);
            Dictionary<long, Print> Assigned = db.Prints.SqlQuery(PrintStartQuery, PrintingEventStart2, PrinterTypeId).ToDictionary(p => p.PrintId);
            ViewData["Assigned"] = Assigned;//Print Start Query

            //count up active user jobs
            Dictionary<string, int> ActiveCount = new Dictionary<string, int>();
            foreach (long key in Assigned.Keys)
            {
                if (ActiveCount.ContainsKey(Assigned[key].UserName))
                {
                    ActiveCount[Assigned[key].UserName] = ActiveCount[Assigned[key].UserName] + 1;
                }
                else
                {
                    ActiveCount.Add(Assigned[key].UserName, 1);
                }
            }
            ViewData["ActiveCount"] = ActiveCount;
            Dictionary<long, Material> Materials = db.Materials.Where(P => P.PrinterTypeId == id).ToDictionary(p => p.MaterialId);
            ViewData["Materials"] = Materials;
            List<Print> Waiting = db.Prints.SqlQuery(WaitingPrintFilesQuery, PrintingEventFile, PrintingEventMachine, PrinterTypeId2).ToList();
            ViewData["Waiting"] = Waiting;
            return PartialView("_PrintQueuePartial");
        }

            //Partial
        public ActionResult ActivePrinters(int id = 0)
        {
            if (id == 0)
            {
                //List All Printers
                string PrintStartQuery = "Select dbo.Prints.* " +
                "from dbo.Prints " +
                "inner join (" +
                    "Select dbo.PrintEvents.* " +
            "from dbo.PrintEvents " +
            "inner join ( " +
            "select dbo.PrintEvents.PrintID, MAX(dbo.PrintEvents.PrintEventId) as MostReventEvent " +
            "from dbo.PrintEvents " +
            "group by dbo.PrintEvents.PrintID " +
            ") mxe on dbo.PrintEvents.PrintID = mxe.PrintID and dbo.PrintEvents.PrintEventId = mxe.MostReventEvent " +
            "where dbo.PrintEvents.EventType = @PrintingEventStart" +
            ") tde on dbo.Prints.PrintId = tde.PrintID " +
            "where dbo.Prints.TermsAndConditionsAgreement IS NOT NULL ";
                string PrintAssignmentsQuery = "Select * " +
             "from dbo.PrintEvents " +
             "inner join ( " +
             "select dbo.PrintEvents.PrintID, MAX(dbo.PrintEvents.PrintEventId) as MostReventEvent " +
             "from dbo.PrintEvents " +
             "group by dbo.PrintEvents.PrintID " +
             ") mxe on dbo.PrintEvents.PrintID = mxe.PrintID and dbo.PrintEvents.PrintEventId = mxe.MostReventEvent " +
             "where dbo.PrintEvents.EventType = @PrintingEventStart ";
                SqlParameter PrintingEventStart = new SqlParameter("@PrintingEventStart", PrintEventType.PRINT_START);
                SqlParameter PrintingEventStart2 = new SqlParameter("@PrintingEventStart", PrintEventType.PRINT_START);
                SqlParameter PrintingEventFile = new SqlParameter("@PrintingEventFile", PrintEventType.PRINT_FAILURE_FILE);
                SqlParameter PrintingEventMachine = new SqlParameter("@PrintingEventMachine", PrintEventType.PRINT_FAILURE_MACHINE);
                //
                Dictionary<long, PrintEvent> PrintingAssignments = db.PrintEvents.SqlQuery(PrintAssignmentsQuery, PrintingEventStart).ToDictionary(p => p.PrinterId);
                ViewData["PrintingAssignments"] = PrintingAssignments;
                Dictionary<long, Print> Assigned = db.Prints.SqlQuery(PrintStartQuery, PrintingEventStart2).ToDictionary(p => p.PrintId);
                ViewData["Assigned"] = Assigned;//Print Start Query

                //
                List<Printer> Printers = db.Printers.OrderBy(p => p.PrinterName).ToList();
                ViewData["Printers"] = Printers;

                //
                Dictionary<long, PrinterStatusLog> PrinterStatus = db.PrinterStatusLogs.SqlQuery(
            "Select dbo.PrinterStatusLogs.* " +
            "From dbo.PrinterStatusLogs " +
            "inner join " +
                "(" +
                "select PrinterStatusLogs.PrinterID, MAX(PrinterStatusLogs.PrinterStatusLogID) as MaxEntryTime " +
                "from dbo.PrinterStatusLogs " +
                "group by dbo.PrinterStatusLogs.PrinterID" +
                ") " +
            "mxe ON dbo.PrinterStatusLogs.PrinterStatusLogID = mxe.MaxEntryTime ").ToDictionary(p => p.PrinterId);
                ViewData["PrinterStatus"] = PrinterStatus;

                Dictionary<long, string> PrinterMaterials = new Dictionary<long, string>();
                foreach (Printer p in Printers)
                {
                    string mats = "";
                    foreach (MaterialCheckout s in p.MaterialsInUse)
                    {
                        mats = mats + s.Material.MaterialName;
                    }
                    if (p.MaterialsInUse.Count() != p.PrinterType.SupportedNumberMaterials)
                    {
                        mats = "Load Material!";
                    }
                    PrinterMaterials.Add(p.PrinterId, mats);
                }
                ViewData["PrinterMaterials"] = PrinterMaterials;
                return PartialView("_ActivePrintersPartial");
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
            "select dbo.PrintEvents.PrintID, MAX(dbo.PrintEvents.PrintEventId) as MostReventEvent " +
            "from dbo.PrintEvents " +
            "group by dbo.PrintEvents.PrintID " +
            ") mxe on dbo.PrintEvents.PrintID = mxe.PrintID and dbo.PrintEvents.PrintEventId = mxe.MostReventEvent " +
            "where dbo.PrintEvents.EventType = @PrintingEventStart" +
            ") tde on dbo.Prints.PrintId = tde.PrintID " +
            "where dbo.Prints.PrinterTypeID = @PrinterTypeID and dbo.Prints.TermsAndConditionsAgreement IS NOT NULL " +
            "Order by dbo.Prints.TermsAndConditionsAgreement";
                string PrintAssignmentsQuery = "Select * " +
             "from dbo.PrintEvents " +
             "inner join ( " +
             "select dbo.PrintEvents.PrintID, MAX(dbo.PrintEvents.PrintEventId) as MostReventEvent " +
             "from dbo.PrintEvents " +
             "group by dbo.PrintEvents.PrintID " +
             ") mxe on dbo.PrintEvents.PrintID = mxe.PrintID and dbo.PrintEvents.PrintEventId = mxe.MostReventEvent " +
             "where dbo.PrintEvents.EventType = @PrintingEventStart ";
                SqlParameter PrintingEventStart = new SqlParameter("@PrintingEventStart", PrintEventType.PRINT_START);
                SqlParameter PrintingEventStart2 = new SqlParameter("@PrintingEventStart", PrintEventType.PRINT_START);
                SqlParameter PrintingEventFile = new SqlParameter("@PrintingEventFile", PrintEventType.PRINT_FAILURE_FILE);
                SqlParameter PrintingEventMachine = new SqlParameter("@PrintingEventMachine", PrintEventType.PRINT_FAILURE_MACHINE);
                SqlParameter PrinterTypeId = new SqlParameter("@PrinterTypeID", id);
                SqlParameter PrinterTypeId2 = new SqlParameter("@PrinterTypeID", id);
                ViewData["id"] = id;
                //
                Dictionary<long, PrintEvent> PrintingAssignments = db.PrintEvents.SqlQuery(PrintAssignmentsQuery, PrintingEventStart).ToDictionary(p => p.PrinterId);
                ViewData["PrintingAssignments"] = PrintingAssignments;
                Dictionary<long, Print> Assigned = db.Prints.SqlQuery(PrintStartQuery, PrintingEventStart2, PrinterTypeId).ToDictionary(p => p.PrintId);
                ViewData["Assigned"] = Assigned;//Print Start Query

                //
                List<Printer> Printers = db.Printers.Where(p => p.PrinterTypeId == id).OrderBy(p => p.PrinterName).ToList();
                ViewData["Printers"] = Printers;
                
                //
                Dictionary<long, PrinterStatusLog> PrinterStatus = db.PrinterStatusLogs.SqlQuery(
            "Select dbo.PrinterStatusLogs.* " +
            "From dbo.PrinterStatusLogs " +
            "inner join " +
                "(" +
                "select PrinterStatusLogs.PrinterID, MAX(PrinterStatusLogs.PrinterStatusLogID) as MaxEntryTime " +
                "from dbo.PrinterStatusLogs " +
                "group by dbo.PrinterStatusLogs.PrinterID" +
                ") " +
            "mxe ON dbo.PrinterStatusLogs.PrinterStatusLogID = mxe.MaxEntryTime ").ToDictionary(p => p.PrinterId);
                ViewData["PrinterStatus"] = PrinterStatus;

                Dictionary<long, string> PrinterMaterials = new Dictionary<long, string>();
                foreach (Printer p in Printers)
                {
                    string mats = "";
                    foreach (MaterialCheckout s in p.MaterialsInUse)
                    {
                        mats = mats + s.Material.MaterialName;
                    }
                    if (p.MaterialsInUse.Count() != p.PrinterType.SupportedNumberMaterials)
                    {
                        mats = "Load Material!";
                    }
                    PrinterMaterials.Add(p.PrinterId, mats);
                }
                ViewData["PrinterMaterials"] = PrinterMaterials;
                return PartialView("_ActivePrintersPartial");
            }
        }

        public ActionResult PastPrints(int? id, int? page, string sortOrder, string currentFilter, string searchString)
        {
            if (id == null || id == 0)
            {
                return RedirectToAction("Index", "PrinterTypes");
            }
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = sortOrder == "Name" ? "name_desc" : "Name";
            ViewBag.DateSortParm = String.IsNullOrEmpty(sortOrder) ? "" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            string CompleteFilesQuery = "Select dbo.Prints.* " +
                "from dbo.Prints " +
                "left outer join " +
                "( " +
                "Select dbo.PrintEvents.PrintID, dbo.PrintEvents.EventType, mxe.MostReventEvent " +
                "from dbo.PrintEvents " +
                "inner join " +
                "( " +
                "select dbo.PrintEvents.PrintID, MAX(dbo.PrintEvents.PrintEventId) as MostReventEvent " +
                "from dbo.PrintEvents " +
                "group by dbo.PrintEvents.PrintID " +
                ") mxe on dbo.PrintEvents.PrintId = mxe.PrintID and dbo.PrintEvents.PrintEventId = mxe.MostReventEvent " +
                ") pnt on dbo.Prints.PrintId = pnt.PrintID " +
                "where (pnt.EventType = " + (int)PrintEventType.PRINT_COMPLETED + " or pnt.EventType = " + (int)PrintEventType.PRINT_CANCELED + ") and dbo.Prints.PrinterTypeID = " + id + " " +
                "order by pnt.MostReventEvent DESC";

            ViewBag.Title = db.PrinterTypes.Find(id).TypeName;
            ViewBag.id = id;

            var prints = (IEnumerable<Print>)db.Prints.SqlQuery(CompleteFilesQuery);

            if (!String.IsNullOrEmpty(searchString))
            {
                prints = prints.Where(s => s.UserName.ToUpper().Contains(searchString.ToUpper()));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    prints = prints.OrderByDescending(s => s.UserName);
                    break;
                case "Date":
                    prints = prints.Reverse();
                    break;
                case "Name":
                    prints = prints.OrderByDescending(s => s.UserName);
                    break;
                default:
                    //Query Sorts by this by default
                    break;
            }

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(prints.ToPagedList(pageNumber, pageSize));
            
        }

         //
        // GET: /Prints/UnapprovedAdmin
        [Authorize(Roles = "Administrator, Moderator")]
        public ActionResult UnapprovedAdmin(int id = 0)
        {
            string UnapprovedUnstartedPrintsSQL = "Select dbo.Prints.* " +
                "from dbo.Prints " +
                "left outer join PrintEvents " +
                "on dbo.Prints.PrintId = PrintEvents.PrintID " +
                "where dbo.PrintEvents.PrinterID IS NULL and dbo.Prints.TermsAndConditionsAgreement IS NULL ";
            return View(db.Prints.SqlQuery(UnapprovedUnstartedPrintsSQL).ToList());
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
            ViewData["InactiveJob"] = false;
            PrintEvent CurrentEvent = null;
            Printer CurrentPrinter = null;
            if (db.PrintEvents.Where(P => P.PrintId == id).Count() > 0)
            {
                CurrentEvent = db.PrintEvents.Where(P => P.PrintId == id).ToList().Last();
                CurrentPrinter = CurrentEvent.Printer;
                ViewData["InactiveJob"] = CurrentEvent.EventType == PrintEventType.PRINT_CANCELED || CurrentEvent.EventType == PrintEventType.PRINT_COMPLETED;
            }
            ViewData["AssignedJob"] = null;
            Machine Mach = null;
            if (CurrentPrinter != null)
            {
                Mach = db.Machines.Where(p => p.AffiliatedPrinter.PrinterId == CurrentPrinter.PrinterId).FirstOrDefault();
            }
            if (Mach != null && Mach.AssignedJob != null)
            {
                ViewData["AssignedJob"] = Mach.AssignedJob;
            }
            ViewData["CurrentEvent"] = CurrentEvent;
            ViewData["CurrentPrinter"] = CurrentPrinter;
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
            List<Material> materials = db.Materials.Where(s => (s.PrinterTypeId == id) && !(s.MaterialSpoolQuantity == 0)).OrderBy(p=>p.MaterialName).ToList<Material>();
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
            ViewData["FullColorPrint"] = printerType.OffersFullColorPrinting;
            ViewData["printerType"] = printerType;
            if (printerType.EnhancedGcodeViewerEnabled)
            {
                return View("GCodeSubmission");
            }
            return View();
        }
        
        //
        // POST: /Prints/Create
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection values, HttpPostedFileBase PrintFile)
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, System.Configuration.ConfigurationManager.AppSettings.Get("ADDomain"));
            Print print = new Print();
            string saveAsDirectory = string.Concat(AppDomain.CurrentDomain.GetData("DataDirectory"), "\\3DPrints\\", DateTime.Now.ToString("yyyy-MMM-d"));
            print.Comment = values.Get("Comment");
            print.BilledUser = false;
            print.ProcessingCharge = 0;
            print.UserName = values["UserName"];
            print.FlaggedPrint = false;
            print.FlaggedComment = "";

            print.FullColorPrint = values.Get("FullColorPrint").Contains("true");
            print.InternalUser = false; //By Default Assumed External, perform AD lookup below to verify
            //Check AD Membership            
            try
            {
                UserPrincipal ADUser = UserPrincipal.FindByIdentity(ctx, print.UserName); //Use ID to prevent Parsing issues
                PrincipalSearchResult<Principal> UserGroups = ADUser.GetAuthorizationGroups();
                foreach (string group in System.Configuration.ConfigurationManager.AppSettings.Get("InternalUserGroups").Split(','))
                {
                    try
                    {
                        foreach (GroupPrincipal gTmp in UserGroups)
                        {
                            if (gTmp.Name.Equals(group))
                            {
                                print.InternalUser = true;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        print.Comment = string.Concat(print.Comment, "\n", e.Message); //Write Message in comment if exception is being thrown
                    }
                }
            }
            catch (Exception e)
            {
                print.Comment = string.Concat(print.Comment, "\n", e.Message);
            }
            
            /*Printer Type ID*/
            print.PrinterTypeId = int.Parse(values["PrinterTypeID"]);

            /* File Type Check */
            bool validFile = false;
            string extension = PrintFile.FileName.Split('.').Last();
            foreach (string type in db.PrinterTypes.Find(print.PrinterTypeId).SupportedFileTypes.Split(','))
            {
                if (extension.Equals(type))
                {
                    validFile = true;
                }
            }
            if (!validFile)
            {
                ModelState.AddModelError("PrintFile", "Incorrect File Type!");
            }
            print.FileName = PrintFile.FileName;

            if (PrintFile.ContentLength < 2048)
            {
                ModelState.AddModelError("PrintFile", "File appears to be Empty");
            }

            /* Material ID Parsing */
            string[] tempMaterial = values.GetValues("MaterialIDs");
            string matIds = tempMaterial[0];
            try
            {
                Material Mt = db.Materials.Find(long.Parse(matIds));
                if (Mt.PrinterTypeId != print.PrinterTypeId)
                {
                    ModelState.AddModelError("MaterialIDs", "Invalid material selected");
                }
                for (int i = 1; i < tempMaterial.Length; i++)
                {
                    Mt = db.Materials.Find(long.Parse(matIds));
                    if (Mt.PrinterTypeId != print.PrinterTypeId)
                    {
                        ModelState.AddModelError("MaterialIDs", "Invalid material selected");
                    }
                    matIds = string.Concat(matIds, ",", tempMaterial[i]);
                }
            }
            catch {
                ModelState.AddModelError("MaterialIDs", "Invalid material selected");
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
                string printFileName = string.Concat(saveAsDirectory, "\\", print.PrintId, "_", PrintFile.FileName);
                PrintFile.SaveAs(printFileName);
                ctx.Dispose();
                return RedirectToAction("PrintWaiver", new { id = print.PrintId });
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
                ctx.Dispose();
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
            ViewData["FullColorPrint"] = printerType.OffersFullColorPrinting;
            ctx.Dispose();
            ViewData["printerType"] = printerType;
            if (printerType.EnhancedGcodeViewerEnabled)
            {
                return View("GCodeSubmission");
            }
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
            ViewData["FullColorPrintCapable"] = print.PrinterType.OffersFullColorPrinting;
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

            print.FullColorPrint = values.Get("FullColorPrint").Contains("true");

            print.InternalUser = values.Get("InternalUser").Contains("true");

            print.BilledUser = values.Get("BilledUser").Contains("true");
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

            print.ProcessingCharge = double.Parse(values["ProcessingCharge"]);

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
            //Invalid!
            List<Material> materials = db.Materials.Where(s => s.PrinterTypeId.Equals(print.PrinterTypeId) && !s.MaterialSpoolQuantity.Equals(0)).ToList<Material>();
            List<SelectList> MaterialsList = new List<SelectList>();
            foreach (string matID in print.MaterialIds.Split(','))
            {
                MaterialsList.Add(new SelectList(materials, "MaterialId", "MaterialName", materials.Find(p => p.MaterialId.Equals(long.Parse(matID)))));
            }
            ViewData["MaterialsList"] = MaterialsList;

            List<string> MNUA = new List<string>();
            for (int i = 1; i <= print.PrinterType.MaxNumberUserAttempts; i++)
            {
                MNUA.Add(i.ToString());
            }
            ViewData["MaxNumberUserAttempts"] = new SelectList(MNUA, print.AuthorizedAttempts.ToString());
            ViewData["FullColorPrintCapable"] = print.PrinterType.OffersFullColorPrinting;
            return View(print);
        }

        //
        // GET: /Prints/Delete/5
        public ActionResult Cancel(long id = 0)
        {
            Print print = db.Prints.Find(id);
            if (print == null)
            {
                return HttpNotFound();
            }
            if (User.IsInRole("Administrator") || User.IsInRole("Moderator") || User.Identity.Name.Equals(print.UserName))
            {
                return View(print); 
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public ActionResult CancelConfirmed(long id)
        {
            Print print = db.Prints.Find(id);
            
            //if print has not been started or acted upon by the staff:
            if (db.PrintEvents.Where(p => p.PrintId == id).ToList().Count() == 0)
            {
                //Cancel the Print!
                PrintEvent Cancelation = new PrintEvent();
                Cancelation.PrintId = id;
                Cancelation.MaterialUsed = 0;
                Cancelation.Comment = "Canceled by: " + User.Identity.Name;
                Cancelation.EventTimeStamp = DateTime.Now;
                Cancelation.EventType = PrintEventType.PRINT_CANCELED;
                Cancelation.PrinterId = db.Printers.Where(P => P.PrinterName.Equals("Null Printer")).First().PrinterId;
                Cancelation.UserName = User.Identity.Name;
                db.PrintEvents.Add(Cancelation);
                db.SaveChanges();
                DispatchCancelationEmail(print, true);
            }
            else
            {
                //Could not cancel the print! See a DM Staff member!
                DispatchCancelationEmail(print, false);
            }
            if (User.Identity.Name.Equals(print.FileName))
            {
                return RedirectToAction("Manage", "Account");
            }
            else if (print.TermsAndConditionsAgreement == null)
            {
                return RedirectToAction("UnapprovedAdmin");
            }
            return RedirectToAction("Index", "Prints", new { id = print.PrinterTypeId });
        }

        //Returns true if e-mail is successfully sent
        private bool DispatchCancelationEmail(Print userPrint, bool Success)
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, System.Configuration.ConfigurationManager.AppSettings.Get("ADDomain"));
            // find the user in question
            try
            {
                UserPrincipal user = UserPrincipal.FindByIdentity(ctx, userPrint.UserName);
                if (user != null)
                {
                    StringBuilder emailAgreement = new StringBuilder();
                    if (Success)
                    {
                        emailAgreement.Append("This is a confirmation that the following file previously you submitted to the DM Office has been canceled: \n");
                    }
                    else
                    {
                        emailAgreement.Append("This file previously you submitted to the DM Office could not be canceled!\n");
                        emailAgreement.Append("This is likely due to having been started by the DM Office. Please see a staff member to have your job canceled: \n");
                    }
                    emailAgreement.Append(string.Concat("File Name: ", userPrint.FileName, "\n"));
                    emailAgreement.Append(string.Concat("NetID: ", userPrint.UserName, "\n"));
                    emailAgreement.Append(string.Concat("Submission Time: ", userPrint.SubmissionTime.ToString(), "\n"));
                    emailAgreement.Append(string.Concat("Authorized Number of Attempts: ", userPrint.AuthorizedAttempts, "\n"));
                    emailAgreement.Append(string.Concat("Printer Type: ", userPrint.PrinterType.TypeName, "\n"));
                    emailAgreement.Append("\n");
                    

                    MailMessage msg = new MailMessage();
                    msg.To.Add(user.EmailAddress);
                    msg.CC.Add(System.Configuration.ConfigurationManager.AppSettings.Get("EmailCCAddress"));
                    msg.From = new MailAddress(System.Configuration.ConfigurationManager.AppSettings.Get("EmailCCAddress"));
                    if (Success)
                    {
                        msg.Subject = string.Concat("Your print of ", userPrint.FileName, " was Canceled.");
                    }
                    else
                    {
                        msg.Subject = string.Concat("Your Cancelation of ", userPrint.FileName, " Failed!");
                    }
                    msg.Body = emailAgreement.ToString();
                    NetworkCredential cred = new NetworkCredential(System.Configuration.ConfigurationManager.AppSettings.Get("SMTPUser"), System.Configuration.ConfigurationManager.AppSettings.Get("SMTPPassword"));

                    SmtpClient client = new SmtpClient(System.Configuration.ConfigurationManager.AppSettings.Get("SMTPServer"), int.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("SMTPPort")));
                    client.Credentials = cred;
                    client.EnableSsl = bool.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("SSLEnable"));
                    client.Send(msg);
                    ctx.Dispose();
                    return true;
                }
            }
            finally { }
            ctx.Dispose();
            return false;
        }

        //Partial View
        public ActionResult GCodeViewer(long id)
        {
            return PartialView("_GCodeViewerPartial", db.Prints.Find(id));
            //return View(db.Prints.Find(id));
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

        //
        // GET: /Prints/
        
        public ActionResult PrintWaiver(long id = 0)
        {
            Print print = db.Prints.Find(id);
            if (print == null)
            {
                return HttpNotFound();
            }
            if (print.InternalUser)
            {
                ViewData["PrintSubmissionWaiverTerms"] = db.PrintSubmissionWaiverTerms.Where(p => p.Enabled.Equals(true) && p.ShowInternalUsers).ToList();
            }
            else
            {
                ViewData["PrintSubmissionWaiverTerms"] = db.PrintSubmissionWaiverTerms.Where(p => p.Enabled.Equals(true) && p.ShowExternalUsers).ToList();
            }
            return View(print);
        }

        //
        // POST: 

        [HttpPost, ActionName("PrintWaiver")]
        [ValidateAntiForgeryToken]
        public ActionResult PrintWaiverConfirmed(FormCollection values)
        {
            long id = long.Parse(values["id"]);
            Print print = db.Prints.Find(id);
            ViewData["PrintSubmissionWaiverTerms"] = db.PrintSubmissionWaiverTerms.Where(p => p.Enabled.Equals(true)).ToList();
            try
            {
                int TotalWaiverConditions = int.Parse(values["PrintSubmissionWaiverTermQt"]);
                int AcceptedWaiverConditions = values.GetValues("PrintSubmissionWaiverTerm").Where(p => p.Equals("I agree")).Count();
                if (TotalWaiverConditions != AcceptedWaiverConditions)
                {
                    ViewData["Waiver"] = true;
                    return View(print);
                }
            }
            catch {
                ViewData["Waiver"] = true;
                return View(print);
            }


            ///Agreement Valid if made this far:
            ///
            print.TermsAndConditionsAgreement = DateTime.Now;
            db.Entry(print).State = EntityState.Modified;
            db.SaveChanges();
            if (!DispatchAgreementEmail(print))
            {
                return HttpNotFound();
            }
            return RedirectToAction("Details", new { id = id });
        }

        //Returns true if e-mail is successfully sent
        private bool DispatchAgreementEmail(Print userPrint)
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, System.Configuration.ConfigurationManager.AppSettings.Get("ADDomain"));
            // find the user in question
            try{
                UserPrincipal user = UserPrincipal.FindByIdentity(ctx, User.Identity.Name);
                if (user != null)
                {
                    StringBuilder emailAgreement = new StringBuilder();
                    emailAgreement.Append("You have submitted the following file to the DM Office to be printed: \n");
                    emailAgreement.Append(string.Concat("File Name: ", userPrint.FileName, "\n"));
                    emailAgreement.Append(string.Concat("NetID: ", userPrint.UserName, "\n"));
                    emailAgreement.Append(string.Concat("Submission Time: ", userPrint.SubmissionTime.ToString(), "\n"));
                    emailAgreement.Append(string.Concat("Authorized Number of Attempts: ", userPrint.AuthorizedAttempts, "\n"));
                    emailAgreement.Append(string.Concat("Printer Type: ", userPrint.PrinterType.TypeName, "\n"));
                    emailAgreement.Append("\n");
                    emailAgreement.Append(string.Concat("I, ", user.GivenName, " ", user.Surname, " have consented to the following terms and conditions with regard to the above print.", "\n\n"));
                    foreach (PrintSubmissionWaiverTerm term in db.PrintSubmissionWaiverTerms.Where(P => P.Enabled == true).ToList())
                    {
                        emailAgreement.Append(string.Concat("I agree: ", term.WaiverText, "\n\n"));
                    }
                    MailMessage msg = new MailMessage();
                    msg.To.Add(user.EmailAddress);
                    string CC = System.Configuration.ConfigurationManager.AppSettings.Get("EmailCCAddress");
                    if (CC != null && !CC.Equals(""))
                    {
                        msg.CC.Add(CC);
                    }
                    msg.From = new MailAddress(System.Configuration.ConfigurationManager.AppSettings.Get("EmailFromAddress"));
                    msg.Subject = string.Concat("Your Print Submission of ", userPrint.FileName, " at ", userPrint.SubmissionTime.ToString());
                    msg.Body = emailAgreement.ToString();
                    NetworkCredential cred = new NetworkCredential(System.Configuration.ConfigurationManager.AppSettings.Get("SMTPUser"), System.Configuration.ConfigurationManager.AppSettings.Get("SMTPPassword"));

                    SmtpClient client = new SmtpClient(System.Configuration.ConfigurationManager.AppSettings.Get("SMTPServer"), int.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("SMTPPort")));
                    client.Credentials = cred;
                    client.EnableSsl = bool.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("SSLEnable"));
                    client.Send(msg);
                    ctx.Dispose();
                    return true;
                }
            }finally{}
            ctx.Dispose();
            return false;
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
            if (System.IO.File.Exists(path))
            {
                return File(path, contentType, string.Concat(print.PrintId, "_", print.FileName));
            }
            else if (System.IO.File.Exists(flaggedPath))
            {
                return File(flaggedPath, contentType, string.Concat(print.PrintId, "_", print.FileName));
            }
            return HttpNotFound();
            
        }
    }
}