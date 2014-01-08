namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedmaterialsprintsandprintertypesforthefutureprintwizardhelper : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Materials", "SuppportMaterial", c => c.Boolean(nullable: false));
            AddColumn("dbo.PrinterTypes", "BuildLength", c => c.Double(nullable: false));
            AddColumn("dbo.PrinterTypes", "BuildWidth", c => c.Double(nullable: false));
            AddColumn("dbo.PrinterTypes", "BuildHeight", c => c.Double(nullable: false));
            AddColumn("dbo.PrinterTypes", "OffersBreakawaySupport", c => c.Boolean(nullable: false));
            AddColumn("dbo.PrinterTypes", "OffersNonBreakAwaySupport", c => c.Boolean(nullable: false));
            AddColumn("dbo.PrinterTypes", "BuildSupportUsesMaterialSlot", c => c.Boolean(nullable: false));
            AddColumn("dbo.PrinterTypes", "OffersFullColorPrinting", c => c.Boolean(nullable: false));
            AddColumn("dbo.PrinterTypes", "FunctionalModelSupport", c => c.Boolean(nullable: false));
            AddColumn("dbo.Prints", "FullColorPrint", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Prints", "FullColorPrint");
            DropColumn("dbo.PrinterTypes", "FunctionalModelSupport");
            DropColumn("dbo.PrinterTypes", "OffersFullColorPrinting");
            DropColumn("dbo.PrinterTypes", "BuildSupportUsesMaterialSlot");
            DropColumn("dbo.PrinterTypes", "OffersNonBreakAwaySupport");
            DropColumn("dbo.PrinterTypes", "OffersBreakawaySupport");
            DropColumn("dbo.PrinterTypes", "BuildHeight");
            DropColumn("dbo.PrinterTypes", "BuildWidth");
            DropColumn("dbo.PrinterTypes", "BuildLength");
            DropColumn("dbo.Materials", "SuppportMaterial");
        }
    }
}
