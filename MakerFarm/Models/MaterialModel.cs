using System;
using System.Data.Entity;

namespace MakerFarm.Models
{
    public class Material
    {
        public long Id { get; set; } /* Generic Id for the Material */
        public string MaterialName { get; set; } /* The Material Name (IE, Navy Blue ABS) */
        public Int16 PrinterTypeId { get; set; } /* The ID of the Printer Type this Material is compatible with  */
    }

    public class MaterialDBContext : DbContext
    {
        public DbSet<Material> Materials { get; set; }
    }
}