namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedcommentFieldsandErrorTracking : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PrintEvents", "PrintErrorTypeId", c => c.Long());
            AddColumn("dbo.PrintEvents", "Comment", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PrintEvents", "Comment");
            DropColumn("dbo.PrintEvents", "PrintErrorTypeId");
        }
    }
}
