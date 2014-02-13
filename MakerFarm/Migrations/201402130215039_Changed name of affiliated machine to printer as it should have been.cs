namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Changednameofaffiliatedmachinetoprinterasitshouldhavebeen : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Jobs", "AffiliatedMachine_PrinterId", "dbo.Printers");
            DropIndex("dbo.Jobs", new[] { "AffiliatedMachine_PrinterId" });
            AddColumn("dbo.Jobs", "AffiliatedPrinter_PrinterId", c => c.Long());
            CreateIndex("dbo.Jobs", "AffiliatedPrinter_PrinterId");
            AddForeignKey("dbo.Jobs", "AffiliatedPrinter_PrinterId", "dbo.Printers", "PrinterId");
            DropColumn("dbo.Jobs", "AffiliatedMachine_PrinterId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Jobs", "AffiliatedMachine_PrinterId", c => c.Long());
            DropForeignKey("dbo.Jobs", "AffiliatedPrinter_PrinterId", "dbo.Printers");
            DropIndex("dbo.Jobs", new[] { "AffiliatedPrinter_PrinterId" });
            DropColumn("dbo.Jobs", "AffiliatedPrinter_PrinterId");
            CreateIndex("dbo.Jobs", "AffiliatedMachine_PrinterId");
            AddForeignKey("dbo.Jobs", "AffiliatedMachine_PrinterId", "dbo.Printers", "PrinterId");
        }
    }
}
