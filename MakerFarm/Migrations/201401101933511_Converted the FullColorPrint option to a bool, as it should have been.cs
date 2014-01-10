namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConvertedtheFullColorPrintoptiontoaboolasitshouldhavebeen : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Prints", "FullColorPrint");
            AddColumn("dbo.Prints", "FullColorPrint", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Prints", "FullColorPrint");
            AddColumn("dbo.Prints", "FullColorPrint", c => c.String());
        }
    }
}
