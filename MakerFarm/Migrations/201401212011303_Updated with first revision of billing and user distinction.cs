namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Updatedwithfirstrevisionofbillinganduserdistinction : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Prints", "InternalUser", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("dbo.Prints", "BilledUser", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("dbo.PrintSubmissionWaiverTerms", "ShowInternalUsers", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("dbo.PrintSubmissionWaiverTerms", "External", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PrintSubmissionWaiverTerms", "External");
            DropColumn("dbo.PrintSubmissionWaiverTerms", "ShowInternalUsers");
            DropColumn("dbo.Prints", "BilledUser");
            DropColumn("dbo.Prints", "InternalUser");
        }
    }
}
