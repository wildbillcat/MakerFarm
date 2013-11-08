using System;
using System.ComponentModel.DataAnnotations;

namespace MakerFarm.Models
{
    
    public class Printer
    {
        public long PrinterID { set; get; }

        [Display(Name = "Printer Name", Description = "This should be a friendly name for the Printer/Queue")]
        public string PrinterName { set; get; }

        [Display(Name = "Printer Type", Description = "The printer type associated with the printer")]
        public int PrinterTypeId { set; get; }

        [Display(Name = "Loaded Material", Description = "This the the current type of material loaded (Or thought to be) in the printer")]
        public long MaterialID { set; get; }
    }
}