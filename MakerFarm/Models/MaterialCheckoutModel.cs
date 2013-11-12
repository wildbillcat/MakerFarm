using System;
using System.ComponentModel.DataAnnotations;

namespace MakerFarm.Models
{
    public class MaterialCheckout
    {
        public long MaterialCheckoutId { get; set; }

        [Display(Name = "Printer", Description = "Printer a unit of material (Roll, Cartridge, etc) is assigned to")]
        public long PrinterId { set; get; }

        [Display(Name = "Material", Description = "The Type of Material Assigned to the printer")]
        public long MaterialId { set; get; }
    }
}