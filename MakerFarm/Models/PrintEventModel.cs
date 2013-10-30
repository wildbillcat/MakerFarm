using System;
using System.Data.Entity;

namespace MakerFarm.Models
{
    public class PrintEvent
    {
        public long Id { get; set; }
        public int EventType { get; set; }
        public DateTime EventTimeStamp { get; set; }

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