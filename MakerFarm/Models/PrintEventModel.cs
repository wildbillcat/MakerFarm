using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web.Mvc;

namespace MakerFarm.Models
{
    public class PrintEvent
    {
        public long Id { get; set; } /* Generic Event ID to distinguish between events */
        public int EventType { get; set; } /* The Event Type ID (One of the Staticly Defined Types) */
        public DateTime EventTimeStamp { get; set; } /* Time the Event was Registered */
        public double MaterialUsed { get; set; } /* Amount of Material Used by event */
        public long PrinterID { get; set; } /* ID number of the printer that event was associated with. */
        public string UserID { get; set; } /* User ID of who registered the event */

        public static int EVENT_PRINT_FAILURE_FILE = 9000; /* Print has failed, possibly due to the */
        public static int EVENT_PRINT_FAILURE_MACHINE = 9001; /* Print has failed by machine error */
        public static int EVENT_PRINT_START = 5000; /* Print has been taken by a machine */
        public static int EVENT_PRINT_COMPLETED = 4000; /* Print has successfully completed */
        public static int EVENT_PRINT_CANCELED = 3000; /* The Print Request has been canceled by the user */
       

        public static List<SelectListItem> getEventTypes()
        {
            List<SelectListItem> EventTypes = new List<SelectListItem>();
            EventTypes.Add(new SelectListItem() { Value = EVENT_PRINT_FAILURE_FILE.ToString(), Text = "Print Failed due to File" });
            EventTypes.Add(new SelectListItem() { Value = EVENT_PRINT_FAILURE_MACHINE.ToString(), Text = "Print Failed due to Machine Error" });
            EventTypes.Add(new SelectListItem() { Value = EVENT_PRINT_START.ToString(), Text = "Print has been Started by a printer" });
            EventTypes.Add(new SelectListItem() { Value = EVENT_PRINT_COMPLETED.ToString(), Text = "Print has been completed by the printer" });
            EventTypes.Add(new SelectListItem() { Value = EVENT_PRINT_CANCELED.ToString(), Text = "Print has been canceled by the user" });
            return EventTypes;
        }
    }

    public class PrintEventDBContext : DbContext
    {
        public PrintEventDBContext() : base()
        {
            Database.SetInitializer<PrintEventDBContext>(new DropCreateDatabaseIfModelChanges<PrintEventDBContext>());
        }
        public DbSet<PrintEvent> Prints { get; set; }
    }
}