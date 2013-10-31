using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace MakerFarm.Models
{
    public class PrintEvent
    {
        public long Id { get; set; }
        public int EventType { get; set; }
        public DateTime EventTimeStamp { get; set; }
        public double MaterialUsed { get; set; }


        public static int EVENT_PRINT_FAILURE_FILE = 9000; /* Print has failed, possibly due to the */
        public static int EVENT_PRINT_FAILURE_MACHINE = 9001; /* Print has failed by machine error */
        public static int EVENT_PRINT_START = 5000; /* Print has been taken by a machine */
        public static int EVENT_PRINT_COMPLETED = 4000; /* Print has successfully completed */

        public static List<string> getEventTypes()
        {
            List<string> EventTypes = new List<string>();
            EventTypes.Add(EVENT_PRINT_FAILURE_FILE.ToString());
            EventTypes.Add(EVENT_PRINT_FAILURE_MACHINE.ToString());
            EventTypes.Add(EVENT_PRINT_START.ToString());
            EventTypes.Add(EVENT_PRINT_COMPLETED.ToString());
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