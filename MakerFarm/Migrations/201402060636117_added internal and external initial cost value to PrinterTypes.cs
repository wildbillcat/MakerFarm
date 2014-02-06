namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedinternalandexternalinitialcostvaluetoPrinterTypes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PrinterTypes", "InternalInitialCost", c => c.Double(nullable: false));
            AddColumn("dbo.PrinterTypes", "ExternalInitialCost", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PrinterTypes", "ExternalInitialCost");
            DropColumn("dbo.PrinterTypes", "InternalInitialCost");
        }
    }
}
