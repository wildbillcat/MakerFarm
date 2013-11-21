namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class correctedthePrintmodeltostoreMaterialIdsofprintsinastringthatcanbeparsed : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Prints", "MaterialIds", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Prints", "MaterialIds");
        }
    }
}
