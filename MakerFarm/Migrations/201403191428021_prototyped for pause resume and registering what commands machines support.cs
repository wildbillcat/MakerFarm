namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class prototypedforpauseresumeandregisteringwhatcommandsmachinessupport : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Machines", "PauseMachine", c => c.Int(nullable: false));
            AddColumn("dbo.Machines", "Print_Send", c => c.Boolean(nullable: false));
            AddColumn("dbo.Machines", "Print_Cancel", c => c.Boolean(nullable: false));
            AddColumn("dbo.Machines", "Print_Pause", c => c.Boolean(nullable: false));
            AddColumn("dbo.Machines", "Print_Resume", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Machines", "Print_Resume");
            DropColumn("dbo.Machines", "Print_Pause");
            DropColumn("dbo.Machines", "Print_Cancel");
            DropColumn("dbo.Machines", "Print_Send");
            DropColumn("dbo.Machines", "PauseMachine");
        }
    }
}
