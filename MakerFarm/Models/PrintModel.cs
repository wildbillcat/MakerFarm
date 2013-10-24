using System;
using System.Data.Entity;

namespace MakerFarm.Models
{
    public class Print
    {
        long ID { get; set; } /* Generic Id for the Print Job */
        string FileName { get; set; } /* The file name of the print submitted */
        string UserID { get; set; } /* User ID of the person whom submitted the print */
        string SMBPath { get; set; } /* This is the UNC path to the File. This will be used to retrieve the file */
        long[] MaterialIDs { get; set; } /* This is an array listing the MaterialIDs of the types of materials requested for the printer job */
        double StaffProcessingFee { get; set; } /* This notes any applicable staff processing fees in addition to print costs */
        double EstPrintCost { get; set; } /* The print cost originally estimated by the print software */
        double FinalPrintCost { get; set; } /* This is the final print cost of the Model (Accumulated Cost of attempts) */
        bool Success { get; set; } /* This denotes if the print job completed successfully or failed. */
        DateTime SubmissionTime { get; set; } /* This is the time the print is submitted to the application */
        DateTime EstToolpathTime { get; set; } /* Estimated amount of time to complete the print job from the processing software */
        DateTime CompletionTime { get; set; } /* The time when the print job was completed */
        Int16 AuthorizedAttempts { get; set; } /* Number of attempts that are requested to be attempted in at cost to the user. */
        Int16 PrinterTypeID { get; set; } /* ID number of the printer  */
        bool StaffAssitedPrint { get; set; }
    }

    public class PrintDBContext : DbContext
    {
        public DbSet<Print> Prints { get; set; }
    }
}