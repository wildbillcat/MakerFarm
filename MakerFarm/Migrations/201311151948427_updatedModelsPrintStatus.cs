namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedModelsPrintStatus : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Materials", "PrinterTypeId");
            CreateIndex("dbo.Printers", "PrinterTypeId");
            CreateIndex("dbo.PrinterStatusLogs", "PrinterId");
            AddForeignKey("dbo.Materials", "PrinterTypeId", "dbo.PrinterTypes", "PrinterTypeId", cascadeDelete: false);
            AddForeignKey("dbo.Printers", "PrinterTypeId", "dbo.PrinterTypes", "PrinterTypeId", cascadeDelete: false);
            AddForeignKey("dbo.PrinterStatusLogs", "PrinterId", "dbo.Printers", "PrinterId", cascadeDelete: false);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PrinterStatusLogs", "PrinterId", "dbo.Printers");
            DropForeignKey("dbo.Printers", "PrinterTypeId", "dbo.PrinterTypes");
            DropForeignKey("dbo.Materials", "PrinterTypeId", "dbo.PrinterTypes");
            DropIndex("dbo.PrinterStatusLogs", new[] { "PrinterId" });
            DropIndex("dbo.Printers", new[] { "PrinterTypeId" });
            DropIndex("dbo.Materials", new[] { "PrinterTypeId" });
        }
    }
}
