namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedPrinterModeltosupportabouttheprinteraninformationurlandmultiplecopies : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PrinterTypes", "AboutPrinter", c => c.String());
            AddColumn("dbo.PrinterTypes", "HyperLink", c => c.String());
            AddColumn("dbo.PrinterTypes", "MaximumNumberOfCopies", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PrinterTypes", "MaximumNumberOfCopies");
            DropColumn("dbo.PrinterTypes", "HyperLink");
            DropColumn("dbo.PrinterTypes", "AboutPrinter");
        }
    }
}
