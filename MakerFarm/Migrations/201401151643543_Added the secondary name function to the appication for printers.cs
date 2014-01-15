namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addedthesecondarynamefunctiontotheappicationforprinters : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Printers", "InternalName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Printers", "InternalName");
        }
    }
}
