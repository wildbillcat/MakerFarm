namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedvirtualpropertiestothebillmodel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Bills", "Print_PrintId", c => c.Long());
            AddColumn("dbo.Bills", "PrintEvent_PrintEventId", c => c.Long());
            CreateIndex("dbo.Bills", "Print_PrintId");
            CreateIndex("dbo.Bills", "PrintEvent_PrintEventId");
            AddForeignKey("dbo.Bills", "Print_PrintId", "dbo.Prints", "PrintId");
            AddForeignKey("dbo.Bills", "PrintEvent_PrintEventId", "dbo.PrintEvents", "PrintEventId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Bills", "PrintEvent_PrintEventId", "dbo.PrintEvents");
            DropForeignKey("dbo.Bills", "Print_PrintId", "dbo.Prints");
            DropIndex("dbo.Bills", new[] { "PrintEvent_PrintEventId" });
            DropIndex("dbo.Bills", new[] { "Print_PrintId" });
            DropColumn("dbo.Bills", "PrintEvent_PrintEventId");
            DropColumn("dbo.Bills", "Print_PrintId");
        }
    }
}
