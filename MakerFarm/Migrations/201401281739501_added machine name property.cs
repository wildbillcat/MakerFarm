namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedmachinenameproperty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Machines", "MachineName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Machines", "MachineName");
        }
    }
}
