using System;
using System.Data.Entity;

namespace MakerFarm.Models
{
    public class PrinterType
    {
        public int Id { get; set; } /* Generic Id for the Printer Type/Model */
        public string TypeName { get; set; } /* The printer model/type name */
        public int SupportedNumberMaterials { get; set; } /* The number of materials supported for sumultanious use. (IE, how many extrution heads available on the printer) */
        public string PrinterIcon { get; set; } /*This is the icon that was uploaded to represent the printer. */
        public int MaxNumberUserAttempts { get; set; } /* This notes the maximum number of attempts allowed for this printer */
        public string SupportedFileTypes { get; set; } /* This is a comma separated list of the printers file extensions */
    }
        
}