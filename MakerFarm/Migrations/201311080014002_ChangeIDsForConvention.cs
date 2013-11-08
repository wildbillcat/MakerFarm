namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeIDsForConvention : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PrintErrorTypes",
                c => new
                    {
                        PrintErrorTypeId = c.Long(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.PrintErrorTypeId);


            DropPrimaryKey("dbo.Materials");
            DropPrimaryKey("dbo.PrinterTypes");
            DropPrimaryKey("dbo.PrintEvents");
            DropPrimaryKey("dbo.Prints");
            DropColumn("dbo.Materials", "Id");
            DropColumn("dbo.PrinterTypes", "Id");
            DropColumn("dbo.PrintEvents", "Id");
            DropColumn("dbo.Prints", "Id");
            DropColumn("dbo.Prints", "SMBPath");
            AddColumn("dbo.Materials", "MaterialId", c => c.Long(nullable: false, identity: true));
            AddColumn("dbo.PrinterTypes", "PrinterTypeId", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.PrintEvents", "PrintEventId", c => c.Long(nullable: false, identity: true));
            AddColumn("dbo.Prints", "PrintId", c => c.Long(nullable: false, identity: true));
            
            AddPrimaryKey("dbo.Materials", "MaterialId");
           
            AddPrimaryKey("dbo.PrinterTypes", "PrinterTypeId");
            
            AddPrimaryKey("dbo.PrintEvents", "PrintEventId");
            
            AddPrimaryKey("dbo.Prints", "PrintId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Prints", "SMBPath", c => c.String());
            AddColumn("dbo.Prints", "Id", c => c.Long(nullable: false, identity: true));
            AddColumn("dbo.PrintEvents", "Id", c => c.Long(nullable: false, identity: true));
            AddColumn("dbo.PrinterTypes", "Id", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.Materials", "Id", c => c.Long(nullable: false, identity: true));
            DropPrimaryKey("dbo.Prints");
            AddPrimaryKey("dbo.Prints", "Id");
            DropPrimaryKey("dbo.PrintEvents");
            AddPrimaryKey("dbo.PrintEvents", "Id");
            DropPrimaryKey("dbo.PrinterTypes");
            AddPrimaryKey("dbo.PrinterTypes", "Id");
            DropPrimaryKey("dbo.Materials");
            AddPrimaryKey("dbo.Materials", "Id");
            DropColumn("dbo.Prints", "PrintId");
            DropColumn("dbo.PrintEvents", "PrintEventId");
            DropColumn("dbo.PrinterTypes", "PrinterTypeId");
            DropColumn("dbo.Materials", "MaterialId");
            DropTable("dbo.PrintErrorTypes");
        }
    }
}
