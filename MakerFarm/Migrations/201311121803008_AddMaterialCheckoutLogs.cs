namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMaterialCheckoutLogs : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PrinterTypes", "MaterialUseUnit", c => c.String());
            AddColumn("dbo.PrinterTypes", "PrintErrorType_PrintErrorTypeId", c => c.Long());
            AddColumn("dbo.PrintEvents", "UserName", c => c.String());
            AlterColumn("dbo.Printers", "PrinterId", c => c.Long(nullable: false, identity: true));
            AlterColumn("dbo.PrinterStatusLogs", "PrinterStatusLogId", c => c.Long(nullable: false, identity: true));
            AlterColumn("dbo.PrinterStatusLogs", "PrinterId", c => c.Long(nullable: false));
            AlterColumn("dbo.PrintEvents", "PrinterId", c => c.Long(nullable: false));
            AlterColumn("dbo.PrintEvents", "PrintId", c => c.Long(nullable: false));
            AlterColumn("dbo.Prints", "PrinterTypeId", c => c.Int(nullable: false));
            DropPrimaryKey("dbo.Printers");
            AddPrimaryKey("dbo.Printers", "PrinterId");
            DropPrimaryKey("dbo.PrinterStatusLogs");
            AddPrimaryKey("dbo.PrinterStatusLogs", "PrinterStatusLogId");
            CreateIndex("dbo.PrinterTypes", "PrintErrorType_PrintErrorTypeId");
            CreateIndex("dbo.Prints", "PrinterTypeId");
            CreateIndex("dbo.PrintEvents", "PrintId");
            CreateIndex("dbo.PrintEvents", "PrinterId");
            AddForeignKey("dbo.PrinterTypes", "PrintErrorType_PrintErrorTypeId", "dbo.PrintErrorTypes", "PrintErrorTypeId");
            AddForeignKey("dbo.Prints", "PrinterTypeId", "dbo.PrinterTypes", "PrinterTypeId", cascadeDelete: true);
            AddForeignKey("dbo.PrintEvents", "PrintId", "dbo.Prints", "PrintId", cascadeDelete: true);
            AddForeignKey("dbo.PrintEvents", "PrinterId", "dbo.Printers", "PrinterId", cascadeDelete: true);
            DropColumn("dbo.PrintEvents", "UserID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PrintEvents", "UserID", c => c.String());
            DropForeignKey("dbo.PrintEvents", "PrinterId", "dbo.Printers");
            DropForeignKey("dbo.PrintEvents", "PrintId", "dbo.Prints");
            DropForeignKey("dbo.Prints", "PrinterTypeId", "dbo.PrinterTypes");
            DropForeignKey("dbo.PrinterTypes", "PrintErrorType_PrintErrorTypeId", "dbo.PrintErrorTypes");
            DropIndex("dbo.PrintEvents", new[] { "PrinterId" });
            DropIndex("dbo.PrintEvents", new[] { "PrintId" });
            DropIndex("dbo.Prints", new[] { "PrinterTypeId" });
            DropIndex("dbo.PrinterTypes", new[] { "PrintErrorType_PrintErrorTypeId" });
            DropPrimaryKey("dbo.PrinterStatusLogs");
            AddPrimaryKey("dbo.PrinterStatusLogs", "PrinterStatusLogID");
            DropPrimaryKey("dbo.Printers");
            AddPrimaryKey("dbo.Printers", "PrinterID");
            AlterColumn("dbo.Prints", "PrinterTypeId", c => c.Int(nullable: false));
            AlterColumn("dbo.PrintEvents", "PrintId", c => c.Long(nullable: false));
            AlterColumn("dbo.PrintEvents", "PrinterId", c => c.Long(nullable: false));
            AlterColumn("dbo.PrinterStatusLogs", "PrinterId", c => c.Long(nullable: false));
            AlterColumn("dbo.PrinterStatusLogs", "PrinterStatusLogId", c => c.Long(nullable: false, identity: true));
            AlterColumn("dbo.Printers", "PrinterId", c => c.Long(nullable: false, identity: true));
            DropColumn("dbo.PrintEvents", "UserName");
            DropColumn("dbo.PrinterTypes", "PrintErrorType_PrintErrorTypeId");
            DropColumn("dbo.PrinterTypes", "MaterialUseUnit");
        }
    }
}
