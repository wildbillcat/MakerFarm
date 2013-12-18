namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedtheFlaggedfieldsforPrintswhichwillallowstafftomonitorspecialcasefiles : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Prints", "FlaggedPrint", c => c.Boolean(nullable: false));
            AddColumn("dbo.Prints", "FlaggedComment", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Prints", "FlaggedComment");
            DropColumn("dbo.Prints", "FlaggedPrint");
        }
    }
}
