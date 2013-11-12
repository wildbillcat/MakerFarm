using System;
using System.ComponentModel.DataAnnotations;

namespace MakerFarm.Models
{
    
    public enum PrinterStatus
    {
        Online, Offline, RequiresMaintenance
    }

    public class PrinterStatusLog
    {
        public long PrinterStatusLogID { set; get; }

        [Display(Name = "LogEntryDate", Description = "The entry Date of the printer Log")]
        public DateTime LogEntryDate { set; get; }

        [Display(Name = "Printer Status", Description = "Status of the Printer")]
        public PrinterStatus LoggedPrinterStatus { set; get; }

        [Display(Name = "Status Comment", Description = "Comment giving additional detail about the status")]
        public string Comment { set; get; }

        [Display(Name = "Printer", Description = "Printer associated with the status")]
        public long PrinterID { set; get; }
    }
}