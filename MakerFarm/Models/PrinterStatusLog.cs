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


        public long PrinterID { set; get; }
    }
}