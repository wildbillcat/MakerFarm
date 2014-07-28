namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Removedextraniousclientpermissionattributes : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ClientPermissions", "GetInformation");
            DropColumn("dbo.ClientPermissions", "SetInformation");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ClientPermissions", "SetInformation", c => c.Boolean(nullable: false));
            AddColumn("dbo.ClientPermissions", "GetInformation", c => c.Boolean(nullable: false));
        }
    }
}
