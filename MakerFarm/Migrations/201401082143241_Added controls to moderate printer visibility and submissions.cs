namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addedcontrolstomoderateprintervisibilityandsubmissions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PrinterTypes", "QueueVisible", c => c.Boolean(nullable: false));
            AddColumn("dbo.PrinterTypes", "SubmissionEnabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PrinterTypes", "SubmissionEnabled");
            DropColumn("dbo.PrinterTypes", "QueueVisible");
        }
    }
}
