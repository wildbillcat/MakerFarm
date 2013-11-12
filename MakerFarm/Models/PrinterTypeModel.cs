using System;
using System.ComponentModel.DataAnnotations;

namespace MakerFarm.Models
{
    public class PrinterType
    {
        
        public int PrinterTypeId { get; set; } /* Generic Id for the Printer Type/Model */

        [Display(Name = "Printer Type Name", Description = "Name of the Printer Type, such as Ultrabot or Replicator 2")]
        public string TypeName { get; set; } /* The printer model/type name */

        [Display(Name = "Maximum Number of Supported Materials", Description = "This is the maximum number of materials ")]
        public int SupportedNumberMaterials { get; set; } /* The number of materials supported for sumultanious use. (IE, how many extrution heads available on the printer) */

        [Display(Name = "Printer Unit of Measure", Description = "This defines what unit of measure it used for the printer. (Gram, Ounce, Cubic Inch)")]
        public string MaterialUseUnit { get; set; } /* Printers use a varying units of measure, this allows the operator to make sure the appropriate chage is made */

        [Display(Name = "Printer Icon", Description = "This should be a image to double at the printer Icon when users select what printer they would like to submit to")]
        public string PrinterIcon { get; set; } /*This is the icon that was uploaded to represent the printer. */

        [Display(Name = "Maximum Authorized Attempts", Description = "This is the Maximum number of attempts a user can authorize on this printer")]
        public int MaxNumberUserAttempts { get; set; } /* This notes the maximum number of attempts allowed for this printer */

        [Display(Name = "Supported File Types", Description = "A comma separated list of file types accepted by this printer. Ie: gcode,x3g,s3g,etc")]
        public string SupportedFileTypes { get; set; } /* This is a comma separated list of the printers file extensions */
    }
        
}