namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVirtualReferencesUpdatedModels : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MaterialCheckouts",
                c => new
                    {
                        MaterialCheckoutId = c.Long(nullable: false, identity: true),
                        PrinterId = c.Long(nullable: false),
                        MaterialId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.MaterialCheckoutId)
                .ForeignKey("dbo.Materials", t => t.MaterialId, cascadeDelete: true)
                .ForeignKey("dbo.Printers", t => t.PrinterId, cascadeDelete: true)
                .Index(t => t.MaterialId)
                .Index(t => t.PrinterId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MaterialCheckouts", "PrinterId", "dbo.Printers");
            DropForeignKey("dbo.MaterialCheckouts", "MaterialId", "dbo.Materials");
            DropIndex("dbo.MaterialCheckouts", new[] { "PrinterId" });
            DropIndex("dbo.MaterialCheckouts", new[] { "MaterialId" });
            DropTable("dbo.MaterialCheckouts");
        }
    }
}
