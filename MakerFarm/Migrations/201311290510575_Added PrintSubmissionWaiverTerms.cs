namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPrintSubmissionWaiverTerms : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PrintSubmissionWaiverTerms",
                c => new
                    {
                        PrintSubmissionWaiverTermId = c.Int(nullable: false, identity: true),
                        Enabled = c.Boolean(nullable: false),
                        WaiverText = c.String(),
                    })
                .PrimaryKey(t => t.PrintSubmissionWaiverTermId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PrintSubmissionWaiverTerms");
        }
    }
}
