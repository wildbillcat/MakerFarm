namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTaskProgresspropertytoMachinesforpercentileassesmentsofprogress : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Machines", "CurrentTaskProgress", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Machines", "CurrentTaskProgress");
        }
    }
}
