namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedanenableflagtoPrinterErrorTypes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PrintErrorTypes", "Enabled", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PrintErrorTypes", "Enabled");
        }
    }
}
