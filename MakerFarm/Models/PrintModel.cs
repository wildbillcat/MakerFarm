using System;
using System.Data.Entity;

namespace MakerFarm.Models
{
    public class Print
    {
        public long Id { get; set; } /* Generic Id for the Print Job */
        public string FileName { get; set; } /* The file name of the print submitted */
        public string UserID { get; set; } /* User ID of the person whom submitted the print */
        public string SMBPath { get; set; } /* This is the UNC path to the File. This will be used to retrieve the file */
        public long[] MaterialIDs { get; set; } /* This is an array listing the MaterialIDs of the types of materials requested for the printer job */
        public double StaffProcessingFee { get; set; } /* This notes any applicable staff processing fees in addition to print costs */
        public double EstPrintCost { get; set; } /* The print cost originally estimated by the print software */
        public double FinalPrintCost { get; set; } /* This is the final print cost of the Model (Accumulated Cost of attempts) */
        public bool Success { get; set; } /* This denotes if the print job completed successfully or failed. */
        public DateTime SubmissionTime { get; set; } /* This is the time the print is submitted to the application */
        public DateTime EstToolpathTime { get; set; } /* Estimated amount of time to complete the print job from the processing software */
        public DateTime CompletionTime { get; set; } /* The time when the print job was completed */
        public int AuthorizedAttempts { get; set; } /* Number of attempts that are requested to be attempted in at cost to the user. */
        public int PrinterTypeID { get; set; } /* ID number of the printer type the file is meant for */
        public bool StaffAssitedPrint { get; set; } /* Denotes if a staff member assited iwth the print submission */
    }

    public class PrintDBContext : DbContext
    {
        public PrintDBContext() : base()
        {
            Database.SetInitializer<PrintDBContext>(new DropCreateDatabaseIfModelChanges<PrintDBContext>());
        }
        public DbSet<Print> Prints { get; set; }
    }
}