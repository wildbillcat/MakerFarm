namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPrinterAndLogs : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Printers",
                c => new
                    {
                        PrinterID = c.Long(nullable: false, identity: true),
                        PrinterName = c.String(),
                        PrinterTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PrinterID);
            
            CreateTable(
                "dbo.PrinterStatusLogs",
                c => new
                    {
                        PrinterStatusLogID = c.Long(nullable: false, identity: true),
                        LogEntryDate = c.DateTime(nullable: false),
                        LoggedPrinterStatus = c.Int(nullable: false),
                        Comment = c.String(),
                        PrinterID = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.PrinterStatusLogID);
            
            AddColumn("dbo.PrintErrorTypes", "PrintErrorName", c => c.String());
            AddColumn("dbo.PrintErrorTypes", "UserError", c => c.Boolean(nullable: false));
            AddColumn("dbo.PrintEvents", "PrintID", c => c.Long(nullable: false));
            AddColumn("dbo.Prints", "UserName", c => c.String());
            DropColumn("dbo.Prints", "UserID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Prints", "UserID", c => c.String());
            DropColumn("dbo.Prints", "UserName");
            DropColumn("dbo.PrintEvents", "PrintID");
            DropColumn("dbo.PrintErrorTypes", "UserError");
            DropColumn("dbo.PrintErrorTypes", "PrintErrorName");
            DropTable("dbo.PrinterStatusLogs");
            DropTable("dbo.Printers");
        }
    }
}
