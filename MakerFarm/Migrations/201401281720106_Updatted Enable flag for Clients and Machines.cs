namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdattedEnableflagforClientsandMachines : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Clients", "Enabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.Machines", "Enabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Machines", "Enabled");
            DropColumn("dbo.Clients", "Enabled");
        }
    }
}
