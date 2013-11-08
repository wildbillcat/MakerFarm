using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace MakerFarm.Models
{
    public class MakerfarmDBContext : DbContext
    {
        public MakerfarmDBContext() : base("DefaultConnection")
        {
            //Database.SetInitializer<MakerfarmDBContext>(new DropCreateDatabaseIfModelChanges<MakerfarmDBContext>());
        }

        public DbSet<PrinterType> PrinterTypes { get; set; }
        public DbSet<PrintEvent> PrintEvents { get; set; }
        public DbSet<Print> Prints { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<PrintErrorType> PrintErrorTypes { get; set; }

    }
}