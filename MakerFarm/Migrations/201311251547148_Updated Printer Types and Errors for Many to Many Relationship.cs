namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedPrinterTypesandErrorsforManytoManyRelationship : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PrinterTypes", "PrintErrorType_PrintErrorTypeId", "dbo.PrintErrorTypes");
            DropIndex("dbo.PrinterTypes", new[] { "PrintErrorType_PrintErrorTypeId" });
            AddColumn("dbo.Prints", "Comment", c => c.String());
            DropColumn("dbo.PrinterTypes", "PrintErrorType_PrintErrorTypeId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PrinterTypes", "PrintErrorType_PrintErrorTypeId", c => c.Long());
            DropColumn("dbo.Prints", "Comment");
            CreateIndex("dbo.PrinterTypes", "PrintErrorType_PrintErrorTypeId");
            AddForeignKey("dbo.PrinterTypes", "PrintErrorType_PrintErrorTypeId", "dbo.PrintErrorTypes", "PrintErrorTypeId");
        }
    }
}
