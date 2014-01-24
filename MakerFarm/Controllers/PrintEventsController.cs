using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MakerFarm.Models;
using System.Data.SqlClient;
using System.Net.Mail;
using System.DirectoryServices.AccountManagement;
using System.Text;
using System.Configuration;

namespace MakerFarm.Controllers
{
    [Authorize(Roles = "Administrator, Moderator")]
    public class PrintEventsController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();

        // GET: /PrintEvents/
        public ActionResult Index()
        {
            var printevents = db.PrintEvents.Include(p => p.Print).Include(p => p.Printer);
            return View(printevents.ToList());
        }

        // GET: /PrintEvents/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrintEvent printevent = db.PrintEvents.Find(id);
            if (printevent == null)
            {
                return HttpNotFound();
            }
            return View(printevent);
        }

        // GET: /PrintEvents/Create
        public ActionResult Create(long id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Print Print = db.Prints.Find(id);
            if (Print == null)
            {
                return HttpNotFound();
            }
            ViewBag.CurrentUser = User.Identity.Name;
            List<PrintEvent> LastStatus = db.PrintEvents.Where(p => p.PrintId.Equals(id)).ToList();
            string PrinterAssignmentError = "";
            ViewBag.PrintId = id;
            ViewBag.Print = Print;
            List<PrintEventType> evts = new List<PrintEventType>();
            
            SelectList PrinterIds;
            string PrintMaterials = "";
            if (0 == LastStatus.Count() || !LastStatus.Last().EventType.Equals(PrintEventType.PRINT_START)) //Print Needs to be Sent!
            {
                evts.Add(PrintEventType.PRINT_START);
                evts.Add(PrintEventType.PRINT_CANCELED);
                //This is in need of some query optimization!
                SqlParameter[] Params = {new SqlParameter("@PrinterTypeId", Print.PrinterTypeId), new SqlParameter("@PrinterStatus", PrinterStatus.Online)};
                //Optimised the Query! Not sure why EF isn't following my models naming syntax for Id vs ID sporatically, will have to review at a later date.
                List<Printer> PrinterList2 = db.Printers.SqlQuery(
                    "Select * FROM dbo.Printers " +
				"INNER JOIN "+     
			   "( " +
			   "Select dbo.PrinterStatusLogs.* " +
            "From dbo.PrinterStatusLogs " +
            "inner join " +
                "( " +
                "select PrinterStatusLogs.PrinterID, MAX(PrinterStatusLogs.LogEntryDate) as MaxEntryTime " +
                "from dbo.PrinterStatusLogs " +
                "group by dbo.PrinterStatusLogs.PrinterID " +
                ") " +  
            "mxe ON dbo.PrinterStatusLogs.LogEntryDate = mxe.MaxEntryTime " +
			") pstat on dbo.Printers.PrinterID = pstat.PrinterID " +
			"where pstat.LoggedPrinterStatus = @PrinterStatus and dbo.Printers.PrinterTypeId = @PrinterTypeId", Params).ToList();

                string PrintAssignmentsQuery = "Select * " +
            "from dbo.PrintEvents " +
            "inner join ( " +
            "select dbo.PrintEvents.PrintID, MAX(dbo.PrintEvents.EventTimeStamp) as MostReventEvent " +
            "from dbo.PrintEvents " +
            "group by dbo.PrintEvents.PrintID " +
            ") mxe on dbo.PrintEvents.PrintID = mxe.PrintID and dbo.PrintEvents.EventTimeStamp = mxe.MostReventEvent " +
            "where dbo.PrintEvents.EventType = @PrintingEventStart ";
                SqlParameter PrintingEventStart = new SqlParameter("@PrintingEventStart", PrintEventType.PRINT_START);
                Dictionary<long, PrintEvent> PrintingAssignments = db.PrintEvents.SqlQuery(PrintAssignmentsQuery, PrintingEventStart).ToDictionary(p => p.PrinterId);
                List<Printer> PrinterList = new List<Printer>();
                foreach (Printer P in PrinterList2)
                {
                    if (!PrintingAssignments.ContainsKey(P.PrinterId))
                    {
                        PrinterList.Add(P);
                    }
                }

                List<Printer> MaterialCompatible = new List<Printer>();
                foreach (Printer P in PrinterList)
                {
                    int materialCompatability = 0;
                    string[] MatList = Print.MaterialIds.Split(',');
                    foreach (string MatString in MatList)
                    {
                        long M = long.Parse(MatString);
                        foreach (MaterialCheckout Mat in P.MaterialsInUse)
                        {
                            if (Mat.MaterialId == M)
                            {
                                materialCompatability++;
                            }
                        }
                    }
                    if (materialCompatability >= MatList.Length)
                    {
                        MaterialCompatible.Add(P);
                    }
                }
                if(PrinterList.Count() == 0){
                    PrinterAssignmentError = string.Concat(PrinterAssignmentError, "There are no compatible Printers Online. \n");
                } else if(MaterialCompatible.Count() == 0)
                { 
                    PrinterAssignmentError = string.Concat(PrinterAssignmentError, "No Printers that are Online are loaded with Compatible Material. \n"); 
                }
                PrinterIds = new SelectList(MaterialCompatible, "PrinterId", "PrinterName");
                int i = 0;
                foreach (string MatString in Print.MaterialIds.Split(','))
                {
                    if (i == 0)
                    {
                        PrintMaterials = db.Materials.Find(long.Parse(MatString)).MaterialName;
                    }
                    else
                    {
                        PrintMaterials = string.Concat(PrintMaterials, ", ", db.Materials.Find(long.Parse(MatString)).MaterialName);
                    }
                }
                ViewBag.PrintMaterials = PrintMaterials;
            }
            else //Print was sent, updating status of print
            {
                List<SelectListItem> Printerlist = new List<SelectListItem>();
                Printerlist.Add(new SelectListItem() { Text = LastStatus.Last().Printer.PrinterName, Value = LastStatus.Last().Printer.PrinterId.ToString(), Selected = true });
                PrinterIds = new SelectList(Printerlist, "Value", "Text");
                evts = Enum.GetValues(typeof(MakerFarm.Models.PrintEventType)).Cast<MakerFarm.Models.PrintEventType>().ToList();
                evts.Remove(PrintEventType.PRINT_START);
            }
            List<PrintErrorType> HumanError = db.PrintErrorTypes.Where(p => p.UserError == true && p.Enabled == true).ToList();
            string HumanHTML = "";
            if (HumanError.Count > 0)
            {
                foreach (PrintErrorType P in HumanError)
                {
                    HumanHTML = string.Concat(HumanHTML, "<option value=\"", P.PrintErrorTypeId, "\">", P.PrintErrorName, "</option>");
                }
            }


            List<PrintErrorType> MachineError = db.PrintErrorTypes.Where(p => p.UserError == false && p.Enabled == true).ToList();
            string MachineHTML = "";
            if (MachineError.Count > 0)
            {
                foreach (PrintErrorType P in MachineError)
                {
                    MachineHTML = string.Concat(MachineHTML, "<option value=\"", P.PrintErrorTypeId, "\">", P.PrintErrorName, "</option>");
                }
            }

            //If no compatable printers found, add null printer and remove the print option so that canceling is the only option.
            bool NullP = false;
            if (PrinterIds.Count() == 0)
            {
                Printer NullPrinter = db.Printers.Where(p => p.PrinterName.Equals("Null Printer")).First();
                List<SelectListItem> Printerlist = new List<SelectListItem>();
                Printerlist.Add(new SelectListItem() { Text = NullPrinter.PrinterName, Value = NullPrinter.PrinterId.ToString(), Selected = true });
                PrinterIds = new SelectList(Printerlist, "Value", "Text");
                evts.Remove(PrintEventType.PRINT_START);
                NullP = true;
            }

            ViewBag.PrinterAssignmentError = PrinterAssignmentError;
            ViewBag.FileErrors = new SelectList(HumanError, "PrintErrorTypeId", "PrintErrorName");
            ViewBag.HumanHTML = HumanHTML;
            ViewBag.MachineHTML = MachineHTML;
            ViewBag.PrinterId = PrinterIds;
            ViewBag.EventTypes = evts;
            ViewBag.Send = evts.Count() == 2 || NullP;
            return View();
        }

        // POST: /PrintEvents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PrintEventId,EventType,MaterialUsed,PrinterId,UserName,PrintId,PrintErrorTypeId,Comment")] PrintEvent printevent, FormCollection values)
        {
            printevent.EventTimeStamp = DateTime.Now;
            if (printevent.PrinterId == 0)
            {
                ModelState.AddModelError("Printer ID Invalid", new Exception("Invalid Printer ID!!!!"));
            }
            //Find what type of printer was being used
            int printerID = db.Prints.Find(printevent.PrintId).PrinterTypeId;
            if (ModelState.IsValid)
            {
                List<PrintEvent> LastEvents = db.PrintEvents.Where(p => p.PrintId.Equals(printevent.PrintId)).ToList();
                PrintEvent LastEvent = null;
                //Fetch Last Event before adding the new event
                if (LastEvents.Count() > 0)
                {
                    LastEvent = LastEvents.Last();
                }
                //If this is a cancelation and from an inactive state, change the assigned printer to the built in Null Printer
                if (printevent.EventType == PrintEventType.PRINT_CANCELED && (LastEvent == null || LastEvent.EventType == PrintEventType.PRINT_FAILURE_FILE || LastEvent.EventType == PrintEventType.PRINT_FAILURE_MACHINE))
                {
                    printevent.PrinterId = db.Printers.Where(p => p.PrinterName.Equals("Null Printer")).First().PrinterId;
                }
                db.PrintEvents.Add(printevent);
                db.SaveChanges();

                //Mark printer as the new status recieved by the Event
                if (!db.Printers.Find(printevent.PrinterId).PrinterName.Equals("Null Printer"))
                {
                    PrinterStatusLog StatusUpdate = new PrinterStatusLog();
                    StatusUpdate.Comment = string.Concat(values["PrinterStatusComments"], " Event Id:", printevent.PrintEventId);
                    StatusUpdate.LogEntryDate = DateTime.Now;
                    StatusUpdate.LoggedPrinterStatus = (PrinterStatus)Enum.Parse(typeof(PrinterStatus), values["LoggedPrinterStatus"]);
                    StatusUpdate.PrinterId = printevent.PrinterId;
                    db.PrinterStatusLogs.Add(StatusUpdate);
                    db.SaveChanges();
                }
                

                if (printevent.EventType.Equals(PrintEventType.PRINT_FAILURE_FILE))
                {
                    Print print = db.Prints.Find(printevent.PrintId);
                    List<PrintEvent> Log = print.PrintEvents.Where(p => p.EventType == PrintEventType.PRINT_FAILURE_FILE).ToList();
                    if (Log.Count() >= print.AuthorizedAttempts)
                    {
                        PrintEvent AutoCancel = new PrintEvent();
                        AutoCancel.PrintId = printevent.PrintId;
                        AutoCancel.Comment = "Automatic Cancelation: Authorized Number of Attempts Reached";
                        AutoCancel.EventTimeStamp = DateTime.Now;
                        AutoCancel.EventType = PrintEventType.PRINT_CANCELED;
                        AutoCancel.MaterialUsed = 0;
                        foreach (PrintEvent E in Log)
                        {
                            AutoCancel.MaterialUsed = AutoCancel.MaterialUsed + E.MaterialUsed;
                        }
                        AutoCancel.PrinterId = printevent.PrinterId;
                        AutoCancel.UserName = printevent.UserName;
                        db.PrintEvents.Add(AutoCancel);
                        db.SaveChanges();
                    }
                }else if (printevent.EventType.Equals(PrintEventType.PRINT_CANCELED))
                {
                    Print print = db.Prints.Find(printevent.PrintId);
                    List<PrintEvent> Log = print.PrintEvents.Where(p => p.EventType == PrintEventType.PRINT_FAILURE_FILE).ToList();
                    PrintEvent AutoCancel = new PrintEvent();
                    AutoCancel.PrintId = printevent.PrintId;
                    AutoCancel.Comment = "Automatic Cancelation: Summarization of Cancelation";
                    AutoCancel.EventTimeStamp = DateTime.Now;
                    AutoCancel.EventType = PrintEventType.PRINT_CANCELED;
                    AutoCancel.MaterialUsed = 0;
                    foreach (PrintEvent E in Log)
                    {
                        AutoCancel.MaterialUsed = AutoCancel.MaterialUsed + E.MaterialUsed;
                    }
                    AutoCancel.MaterialUsed = AutoCancel.MaterialUsed + printevent.MaterialUsed;
                    AutoCancel.PrinterId = db.Printers.Where(p => p.PrinterName.Equals("Null Printer")).First().PrinterId;
                    AutoCancel.UserName = printevent.UserName;
                    db.PrintEvents.Add(AutoCancel);
                    db.SaveChanges();
                    DispatchEventEmail(print, false);
                }

                if (printevent.EventType == PrintEventType.PRINT_COMPLETED)
                {
                    Print print = db.Prints.Find(printevent.PrintId);
                    DispatchEventEmail(print, true);
                }
                return RedirectToAction("Index", "Prints", new { id = printerID });
            }
            return HttpNotFound("Sorry an error has occured");
        }

        // GET: /PrintEvents/Edit/5

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrintEvent printevent = db.PrintEvents.Find(id);
            if (printevent == null)
            {
                return HttpNotFound();
            }
            List<PrintEventType> evts = Enum.GetValues(typeof(MakerFarm.Models.PrintEventType)).Cast<MakerFarm.Models.PrintEventType>().ToList();
            ViewBag.EventTypes = evts;
            return View(printevent);
        }

        // POST: /PrintEvents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="PrintEventId,EventType,EventTimeStamp,MaterialUsed,PrinterId,UserName,PrintId")] PrintEvent printevent)
        {
            if (ModelState.IsValid)
            {
                db.Entry(printevent).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", "Prints", new { id = printevent.PrintId});
            }
            List<PrintEventType> evts = Enum.GetValues(typeof(MakerFarm.Models.PrintEventType)).Cast<MakerFarm.Models.PrintEventType>().ToList();
            ViewBag.EventTypes = evts;
            return View(printevent);
        }

        // GET: /PrintEvents/Delete/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrintEvent printevent = db.PrintEvents.Find(id);
            if (printevent == null)
            {
                return HttpNotFound();
            }
            return View(printevent);
        }

        // POST: /PrintEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult DeleteConfirmed(long id)
        {
            PrintEvent printevent = db.PrintEvents.Find(id);
            db.PrintEvents.Remove(printevent);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private bool DispatchEventEmail(Print userPrint, bool Success)
        {
            // set up domain context
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
            // find the user in question
            try
            {
                UserPrincipal user = UserPrincipal.FindByIdentity(ctx, userPrint.UserName);
                if (user != null)
                {
                    StringBuilder emailAgreement = new StringBuilder();
                    if (Success)
                    {
                        emailAgreement.Append("This is a confirmation that the following file previously you submitted to the DM Office has Completed: \n");
                    }
                    else
                    {
                        emailAgreement.Append("This file previously you submitted to the DM Office could not be completed and has been canceled!\n");
                        emailAgreement.Append("This is likely due to having been started by the DM Office and it has exceeded your number of authorized prints.\n");
                    }
                    emailAgreement.Append("Please go to the DM Office to retrieve your model:\n");
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
                        msg.Subject = string.Concat("Your print of ", userPrint.FileName, " has Completed.");
                    }
                    else
                    {
                        msg.Subject = string.Concat("Your print of ", userPrint.FileName, " has failed.");
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
