namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedatermsandconditionsagreementdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Prints", "TermsAndConditionsAgreement", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Prints", "TermsAndConditionsAgreement");
        }
    }
}
