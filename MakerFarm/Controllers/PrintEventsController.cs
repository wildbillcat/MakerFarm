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

namespace MakerFarm.Controllers
{
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
            ViewBag.PrintId = id;
            ViewBag.Print = Print;
            List<PrintEventType> evts = new List<PrintEventType>();
            evts.Add(PrintEventType.PRINT_START);
            SelectList PrinterIds;
            string PrintMaterials = "";
            if (0 == LastStatus.Count() || !LastStatus.Last().EventType.Equals(PrintEventType.PRINT_START)) //Print Needs to be Sent!
            {
                //This is in need of some query optimization!
                SqlParameter[] Params = {new SqlParameter("@PrinterTypeId", Print.PrinterTypeId), new SqlParameter("@PrinterStatus", PrinterStatus.Online)};
                //Optimised the Query! Not sure why EF isn't following my models naming syntax for Id vs ID sporatically, will have to review at a later date.
                List<Printer> PrinterList = db.Printers.SqlQuery(
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
            List<PrintErrorType> HumanError = db.PrintErrorTypes.Where(p => p.UserError.Equals(true)).ToList();
            string HumanHTML = "";
            if (HumanError.Count > 0)
            {
                foreach (PrintErrorType P in HumanError)
                {
                    HumanHTML = string.Concat(HumanHTML, "<option value=\"", P.PrintErrorTypeId, "\">", P.PrintErrorName, "</option>");
                }
            }
            
            
            List<PrintErrorType> MachineError = db.PrintErrorTypes.Where(p => p.UserError.Equals(false)).ToList();
            string MachineHTML = "";
            if (MachineError.Count > 0)
            {
                foreach (PrintErrorType P in MachineError)
                {
                    MachineHTML = string.Concat(MachineHTML, "<option value=\"", P.PrintErrorTypeId, "\">", P.PrintErrorName, "</option>");
                }
            }
            ViewBag.FileErrors = new SelectList(HumanError, "PrintErrorTypeId", "PrintErrorName");
            ViewBag.HumanHTML = HumanHTML;
            ViewBag.MachineHTML = MachineHTML;
            ViewBag.PrinterId = PrinterIds;
            ViewBag.EventTypes = evts;
            ViewBag.Send = evts.Count() == 1;
            return View();
        }

        // POST: /PrintEvents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PrintEventId,EventType,MaterialUsed,PrinterId,UserName,PrintId,PrintErrorTypeId,Comment")] PrintEvent printevent)
        {
            printevent.EventTimeStamp = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.PrintEvents.Add(printevent);
                db.SaveChanges();
                int printerID = db.Printers.Find(printevent.PrinterId).PrinterTypeId;
                if(!printevent.EventType.Equals(PrintEventType.PRINT_START))
                {
                    PrinterStatusLog StatusUpdate = new PrinterStatusLog();
                    StatusUpdate.Comment = string.Concat("AutoGenerated by Print Event Id:", printevent.PrintEventId);
                    StatusUpdate.LogEntryDate = DateTime.Now;
                    StatusUpdate.LoggedPrinterStatus = PrinterStatus.RequiresMaintenance;
                    StatusUpdate.PrinterId = printevent.PrinterId;
                    db.PrinterStatusLogs.Add(StatusUpdate);
                    db.SaveChanges();
                }
                if (printevent.EventType.Equals(PrintEventType.PRINT_FAILURE_FILE))
                {
                    Print print = db.Prints.Find(printevent.PrintId);
                    List<PrintEvent> Log = print.PrintEvents.Where(p => p.PrintId.Equals(printevent.PrintId)).ToList();
                    if (Log.Count() >= print.AuthorizedAttempts)
                    {
                        PrintEvent AutoCancel = new PrintEvent();
                        AutoCancel.PrintId = printevent.PrintId;
                        AutoCancel.Comment = "Automatic Cancelation: Authorized Number of Attempts Reached";
                        AutoCancel.EventTimeStamp = DateTime.Now;
                        AutoCancel.EventType = PrintEventType.PRINT_CANCELED;
                        AutoCancel.MaterialUsed = 0;
                        AutoCancel.PrinterId = printevent.PrinterId;
                        AutoCancel.UserName = printevent.UserName;
                        db.PrintEvents.Add(AutoCancel);
                        db.SaveChanges();
                    }
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
            ViewBag.PrintId = new SelectList(db.Prints, "PrintId", "FileName", printevent.PrintId);
            ViewBag.PrinterId = new SelectList(db.Printers, "PrinterId", "PrinterName", printevent.PrinterId);
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
                return RedirectToAction("Index");
            }
            ViewBag.PrintId = new SelectList(db.Prints, "PrintId", "FileName", printevent.PrintId);
            ViewBag.PrinterId = new SelectList(db.Printers, "PrinterId", "PrinterName", printevent.PrinterId);
            return View(printevent);
        }

        // GET: /PrintEvents/Delete/5
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
        public ActionResult DeleteConfirmed(long id)
        {
            PrintEvent printevent = db.PrintEvents.Find(id);
            db.PrintEvents.Remove(printevent);
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
