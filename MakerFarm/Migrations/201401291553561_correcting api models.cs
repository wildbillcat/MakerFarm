namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class correctingapimodels : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ClientPermissions", "Client_ClientId", c => c.Int());
            AddColumn("dbo.ClientPermissions", "Machine_MachineId", c => c.Long());
            AddColumn("dbo.Clients", "ClientUserName", c => c.String());
            CreateIndex("dbo.ClientPermissions", "Client_ClientId");
            CreateIndex("dbo.ClientPermissions", "Machine_MachineId");
            AddForeignKey("dbo.ClientPermissions", "Client_ClientId", "dbo.Clients", "ClientId");
            AddForeignKey("dbo.ClientPermissions", "Machine_MachineId", "dbo.Machines", "MachineId");
            DropColumn("dbo.ClientPermissions", "ClientId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ClientPermissions", "ClientId", c => c.Int(nullable: false));
            DropForeignKey("dbo.ClientPermissions", "Machine_MachineId", "dbo.Machines");
            DropForeignKey("dbo.ClientPermissions", "Client_ClientId", "dbo.Clients");
            DropIndex("dbo.ClientPermissions", new[] { "Machine_MachineId" });
            DropIndex("dbo.ClientPermissions", new[] { "Client_ClientId" });
            DropColumn("dbo.Clients", "ClientUserName");
            DropColumn("dbo.ClientPermissions", "Machine_MachineId");
            DropColumn("dbo.ClientPermissions", "Client_ClientId");
        }
    }
}
