using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MakerFarm.Models;
using System.Data.SqlClient;

namespace MakerFarm.Controllers
{
    public class HomeController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();

        public ActionResult Index()
        {
            return RedirectToAction("SubmissionSelection", "PrinterTypes");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult SystemStatus()
        {
            return View();
        }

        public ActionResult CompactWaitingPrints()
        {
            List<PrinterType> TypeList = db.PrinterTypes.Where(p => p.QueueVisible).ToList();
            ViewData["TypeList"] = TypeList;
            Dictionary<int, Print[]> PrintQueues = new Dictionary<int,Print[]>();
            long LongestQueue = 0;
            foreach(PrinterType T in TypeList){
                int id = T.PrinterTypeId;
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
                SqlParameter PrintingEventFile = new SqlParameter("@PrintingEventFile", PrintEventType.PRINT_FAILURE_FILE);
                SqlParameter PrintingEventMachine = new SqlParameter("@PrintingEventMachine", PrintEventType.PRINT_FAILURE_MACHINE);
                SqlParameter PrinterTypeId2 = new SqlParameter("@PrinterTypeID", id);
                PrintQueues.Add(id,db.Prints.SqlQuery(WaitingPrintFilesQuery, PrintingEventFile, PrintingEventMachine, PrinterTypeId2).ToArray());
                if (LongestQueue < PrintQueues[id].Length)
                {
                    LongestQueue = PrintQueues[id].Length;
                }
            }
            ViewData["PrintQueues"] = PrintQueues;
            ViewData["LongestQueue"] = LongestQueue;

            Dictionary<long, Material> Materials = db.Materials.ToDictionary(p => p.MaterialId);
            ViewData["Materials"] = Materials;

            return PartialView("_CompactWaitingPrintsPartial");
        }

        public ActionResult CompactActivePrinters(int id = 0)
        {
            List<PrinterType> MasterTypes = db.PrinterTypes.Where(p => p.QueueVisible).ToList();
            ViewData["MasterTypes"] = MasterTypes;
            List<PrinterType> TypeList;
            if (id == 0)
            {
                TypeList = db.PrinterTypes.Where(p => p.QueueVisible).ToList();
            }
            else
            {
                TypeList = db.PrinterTypes.Where(p => p.QueueVisible && p.PrinterTypeId == id).ToList();
            }
            ViewData["TypeList"] = TypeList;
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
            Dictionary<int, Printer[]> PrintersByType = new Dictionary<int, Printer[]>();
            long GreatestNumberOfPrinters = 0;
            foreach (PrinterType p in TypeList)
            {
                PrintersByType.Add(p.PrinterTypeId, db.Printers.Where(j=>j.PrinterTypeId == p.PrinterTypeId).ToArray());
                if (PrintersByType[p.PrinterTypeId].Length > GreatestNumberOfPrinters)
                {
                    GreatestNumberOfPrinters = PrintersByType[p.PrinterTypeId].Length;
                }
            }
            ViewData["PrintersByType"] = PrintersByType;
            ViewData["GreatestNumberOfPrinters"] = GreatestNumberOfPrinters;
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

            Dictionary<long?, Machine> Machines = db.Machines.Where(p => p.PrinterId != null).ToDictionary(p => p.PrinterId);
            ViewData["Machines"] = Machines;
            return PartialView("_CompactPrintQueueStatusPartial");
        }

        [Authorize(Roles = "Moderator, Administrator")]
        public ActionResult Administration()
        {
            return View();
        }

        public ActionResult Denied()
        {
            ViewBag.Message = "You do not have access to this page.";

            return View();
        }
    }
}
