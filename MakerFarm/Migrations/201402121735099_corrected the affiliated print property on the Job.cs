namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class correctedtheaffiliatedprintpropertyontheJob : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Jobs", "AffiliatedPrint_PrinterId", "dbo.Printers");
            DropIndex("dbo.Jobs", new[] { "AffiliatedPrint_PrinterId" });
            RenameColumn(table: "dbo.Jobs", name: "AffiliatedPrint_PrinterId", newName: "AffiliatedPrint_PrintId");
            CreateIndex("dbo.Jobs", "AffiliatedPrint_PrintId");
            AddForeignKey("dbo.Jobs", "AffiliatedPrint_PrintId", "dbo.Prints", "PrintId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Jobs", "AffiliatedPrint_PrintId", "dbo.Prints");
            DropIndex("dbo.Jobs", new[] { "AffiliatedPrint_PrintId" });
            RenameColumn(table: "dbo.Jobs", name: "AffiliatedPrint_PrintId", newName: "AffiliatedPrint_PrinterId");
            CreateIndex("dbo.Jobs", "AffiliatedPrint_PrinterId");
            AddForeignKey("dbo.Jobs", "AffiliatedPrint_PrinterId", "dbo.Printers", "PrinterId");
        }
    }
}
