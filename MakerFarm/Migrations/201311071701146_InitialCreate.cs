namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Materials",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        MaterialName = c.String(),
                        PrinterTypeId = c.Int(nullable: false),
                        MaterialSpoolQuantity = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PrinterTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TypeName = c.String(),
                        SupportedNumberMaterials = c.Int(nullable: false),
                        PrinterIcon = c.String(),
                        MaxNumberUserAttempts = c.Int(nullable: false),
                        SupportedFileTypes = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PrintEvents",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        EventType = c.Int(nullable: false),
                        EventTimeStamp = c.DateTime(nullable: false),
                        MaterialUsed = c.Double(nullable: false),
                        PrinterID = c.Long(nullable: false),
                        UserID = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Prints",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        FileName = c.String(),
                        UserID = c.String(),
                        SMBPath = c.String(),
                        EstMaterialUse = c.Double(nullable: false),
                        SubmissionTime = c.DateTime(nullable: false),
                        EstToolpathTime = c.Int(nullable: false),
                        AuthorizedAttempts = c.Int(nullable: false),
                        PrinterTypeID = c.Int(nullable: false),
                        StaffAssitedPrint = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserProfile",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        UserName = c.String(),
                    })
                .PrimaryKey(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UserProfile");
            DropTable("dbo.Prints");
            DropTable("dbo.PrintEvents");
            DropTable("dbo.PrinterTypes");
            DropTable("dbo.Materials");
        }
    }
}
