using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace MakerFarm.Models
{
    public class MakerfarmContext : DbContext
    {
        public MakerfarmContext() : base()
        {
            Database.SetInitializer<PrinterTypeDBContext>(new DropCreateDatabaseIfModelChanges<PrinterTypeDBContext>());
        }

        public DbSet<PrinterType> PrinterTypes { get; set; }
        public DbSet<PrintEvent> PrintEvents { get; set; }
        public DbSet<Print> Prints { get; set; }
        public DbSet<Material> Materials { get; set; }
    }
}