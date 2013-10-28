using System;
using System.Data.Entity;

namespace MakerFarm.Models
{
    public class PrinterType
    {
        public Int16 Id { get; set; } /* Generic Id for the Printer Type/Model */
        public string TypeName { get; set; } /* The printer model/type name */
        public Int16 SupportedNumberMaterials { get; set; } /* The number of materials supported for sumultanious use. (IE, how many extrution heads available on the printer) */
        public string PrinterIcon { get; set; } /*This is the icon that was uploaded to represent the printer. */
    }

    public class PrinterTypeDBContext : DbContext
    {
        public DbSet<PrinterType> PrinterTypes { get; set; }
    }
}