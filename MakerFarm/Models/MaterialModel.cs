using System;
using System.ComponentModel.DataAnnotations;

namespace MakerFarm.Models
{
    public class Material
    {
        public long MaterialId { get; set; } /* Generic Id for the Material */

        [Display(Name = "Material Name", Description = "The name of the print material. (IE: Navy Blue ABS)")]
        public string MaterialName { get; set; } /* The Material Name (IE, Navy Blue ABS) */

        [Display(Name = "Printer Type", Description = "The printer type associated with the Material")]
        public int PrinterTypeId { get; set; } /* The ID of the Printer Type this Material is compatible with  */

        [Display(Name = "Material Quantity", Description = "This is the number of Physical Rolls of Material kept in inventory")]
        public long MaterialSpoolQuantity { get; set; } /* This denotes how many spools of the material are on hand, and thus the maximum number of simultanious jobs using said material */

        public virtual PrinterType PrinterType { get; set; } /*Provides easier access to the Associated Printer! */
    }

}