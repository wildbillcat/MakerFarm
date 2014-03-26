using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace MakerFarm.Models
{
    public class MakerfarmDBContext : DbContext
    {
        
        public DbSet<PrinterType> PrinterTypes { get; set; }
        public DbSet<PrintEvent> PrintEvents { get; set; }
        public DbSet<Print> Prints { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<PrintErrorType> PrintErrorTypes { get; set; }
        public DbSet<Printer> Printers { get; set; }
        public DbSet<PrinterStatusLog> PrinterStatusLogs { set; get; }
        public DbSet<MaterialCheckout> MaterialCheckouts { get; set; }
        public DbSet<PrintSubmissionWaiverTerm> PrintSubmissionWaiverTerms { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientPermission> ClientPermissions { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Machine> Machines { get; set; }

        public MakerfarmDBContext() : base("DefaultConnection")
        {

        }

    }
}