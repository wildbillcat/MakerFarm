namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Formattedexternaltohumanfriendlycolumnname : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PrintSubmissionWaiverTerms", "ShowExternalUsers", c => c.Boolean(nullable: false, defaultValue: false));
            DropColumn("dbo.PrintSubmissionWaiverTerms", "External");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PrintSubmissionWaiverTerms", "External", c => c.Boolean(nullable: false));
            DropColumn("dbo.PrintSubmissionWaiverTerms", "ShowExternalUsers");
        }
    }
}
