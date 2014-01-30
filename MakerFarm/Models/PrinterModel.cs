using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakerFarm.Models
{
    
    public class Printer
    {
        public long PrinterId { set; get; }

        [Display(Name = "Printer Name", Description = "This should be a friendly name for the Printer/Queue")]
        public string PrinterName { set; get; }

        [Display(Name = "Secondary Name", Description = "This is an optional name used internally to refer to the printer")]
        public string InternalName { set; get; }

        [Display(Name = "Printer Type", Description = "The printer type associated with the printer")]
        public int PrinterTypeId { set; get; }

        [Display(Name = "Papercut Print Server", Description = "This is the name of the print server associated w/ the virtual queue")]
        public string PapercutPrintServer { get; set; }

        [Display(Name = "Papercut Print Queue", Description = "This is the name of the virtual papercut queue associated with the printer")]
        public string PapercutPrintQueue { get; set; }

        [Display(Name = "Loaded Material(s)", Description = "This the the current type of material(s) loaded (Or thought to be) in the printer")]
        public virtual ICollection<MaterialCheckout> MaterialsInUse { set; get; }

        public virtual PrinterType PrinterType { get; set; } /*Provides easier access to the Associated Printer! */

        [InverseProperty("AffiliatedPrinter")]
        public virtual Machine AssociatedMachine { get; set; }
    }
}