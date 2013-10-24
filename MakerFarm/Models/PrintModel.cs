using System;
using System.Data.Entity;

namespace MakerFarm.Models
{
    public class Print
    {
        long ID { get; set; }
        string FileName { get; set; }
        string UserID { get; set; }
        string YDCAssetID { get; set; }
        string MaterialColor { get; set; }
        double StaffProcessingFee { get; set; }
        double PrintCost { get; set; }
        DateTime SubmissionTime { get; set; }
        DateTime ToolpathTime { get; set; }
        DateTime CompletionTime { get; set; }
        Int16 AuthorizedAttempts { get; set; }
        Int16 PrinterType { get; set; }
    }

    public class PrintDBContext : DbContext
    {
        public DbSet<Print> Prints { get; set; }
    }
}