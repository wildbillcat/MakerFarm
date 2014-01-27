namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedBillstotheDB : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Bills",
                c => new
                    {
                        BillId = c.Long(nullable: false, identity: true),
                        UserName = c.String(),
                        BillingTime = c.DateTime(nullable: false),
                        TotalBillingAmount = c.Double(nullable: false),
                        PrintEventId = c.Int(nullable: false),
                        PrintId = c.Int(nullable: false),
                        Comment = c.String(),
                    })
                .PrimaryKey(t => t.BillId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Bills");
        }
    }
}
