namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedadditionalsettingsforEnhancedGcodeViewer : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PrinterTypes", "EnhancedGcodeViewerEnabled", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("dbo.PrinterTypes", "GCodePlasticDiameter", c => c.Double(nullable: false, defaultValue: 0));
            AddColumn("dbo.PrinterTypes", "PlasticType", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.PrinterTypes", "NozzleSize", c => c.Double(nullable: false, defaultValue: 0));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PrinterTypes", "NozzleSize");
            DropColumn("dbo.PrinterTypes", "PlasticType");
            DropColumn("dbo.PrinterTypes", "GCodePlasticDiameter");
            DropColumn("dbo.PrinterTypes", "EnhancedGcodeViewerEnabled");
        }
    }
}
