namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedRepRancherClientClientPermissionMachineJob : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClientPermissions",
                c => new
                    {
                        ClientPermissionId = c.Long(nullable: false, identity: true),
                        ClientId = c.Int(nullable: false),
                        GetInformation = c.Boolean(nullable: false),
                        SetInformation = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ClientPermissionId);
            
            CreateTable(
                "dbo.Clients",
                c => new
                    {
                        ClientId = c.Int(nullable: false, identity: true),
                        ClientName = c.String(),
                        ClientAPIKey = c.String(),
                        LastUpdated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ClientId);
            
            CreateTable(
                "dbo.Jobs",
                c => new
                    {
                        JobId = c.Int(nullable: false, identity: true),
                        Status = c.String(),
                        started = c.Boolean(nullable: false),
                        complete = c.Boolean(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                        AffiliatedMachine_PrinterId = c.Long(),
                        AffiliatedPrint_PrinterId = c.Long(),
                    })
                .PrimaryKey(t => t.JobId)
                .ForeignKey("dbo.Printers", t => t.AffiliatedMachine_PrinterId)
                .ForeignKey("dbo.Printers", t => t.AffiliatedPrint_PrinterId)
                .Index(t => t.AffiliatedMachine_PrinterId)
                .Index(t => t.AffiliatedPrint_PrinterId);
            
            CreateTable(
                "dbo.Machines",
                c => new
                    {
                        MachineId = c.Long(nullable: false, identity: true),
                        PrinterId = c.Long(),
                        Status = c.String(),
                        idle = c.Boolean(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                        ClientJobSupport = c.Boolean(nullable: false),
                        AssignedJob_JobId = c.Int(),
                    })
                .PrimaryKey(t => t.MachineId)
                .ForeignKey("dbo.Printers", t => t.PrinterId)
                .ForeignKey("dbo.Jobs", t => t.AssignedJob_JobId)
                .Index(t => t.PrinterId)
                .Index(t => t.AssignedJob_JobId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Machines", "AssignedJob_JobId", "dbo.Jobs");
            DropForeignKey("dbo.Machines", "PrinterId", "dbo.Printers");
            DropForeignKey("dbo.Jobs", "AffiliatedPrint_PrinterId", "dbo.Printers");
            DropForeignKey("dbo.Jobs", "AffiliatedMachine_PrinterId", "dbo.Printers");
            DropIndex("dbo.Machines", new[] { "AssignedJob_JobId" });
            DropIndex("dbo.Machines", new[] { "PrinterId" });
            DropIndex("dbo.Jobs", new[] { "AffiliatedPrint_PrinterId" });
            DropIndex("dbo.Jobs", new[] { "AffiliatedMachine_PrinterId" });
            DropTable("dbo.Machines");
            DropTable("dbo.Jobs");
            DropTable("dbo.Clients");
            DropTable("dbo.ClientPermissions");
        }
    }
}
