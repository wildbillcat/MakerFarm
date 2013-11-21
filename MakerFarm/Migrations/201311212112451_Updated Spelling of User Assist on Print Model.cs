namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedSpellingofUserAssistonPrintModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Prints", "StaffAssistedPrint", c => c.Boolean(nullable: false));
            DropColumn("dbo.Prints", "StaffAssitedPrint");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Prints", "StaffAssitedPrint", c => c.Boolean(nullable: false));
            DropColumn("dbo.Prints", "StaffAssistedPrint");
        }
    }
}
