namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedprocessingchargestoprints : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Prints", "ProcessingCharge", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Prints", "ProcessingCharge");
        }
    }
}
