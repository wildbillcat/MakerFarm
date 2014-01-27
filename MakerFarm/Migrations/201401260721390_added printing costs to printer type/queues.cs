namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedprintingcoststoprintertypequeues : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PrinterTypes", "InternalCostPerUnit", c => c.Double(nullable: false));
            AddColumn("dbo.PrinterTypes", "ExternalCostPerUnit", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PrinterTypes", "ExternalCostPerUnit");
            DropColumn("dbo.PrinterTypes", "InternalCostPerUnit");
        }
    }
}
