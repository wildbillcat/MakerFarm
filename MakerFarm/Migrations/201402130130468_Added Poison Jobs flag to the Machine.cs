namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPoisonJobsflagtotheMachine : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Machines", "PoisonJobs", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Machines", "PoisonJobs");
        }
    }
}
