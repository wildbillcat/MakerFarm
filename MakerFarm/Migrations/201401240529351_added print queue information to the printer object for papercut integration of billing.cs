namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedprintqueueinformationtotheprinterobjectforpapercutintegrationofbilling : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Printers", "PapercutPrintServer", c => c.String());
            AddColumn("dbo.Printers", "PapercutPrintQueue", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Printers", "PapercutPrintQueue");
            DropColumn("dbo.Printers", "PapercutPrintServer");
        }
    }
}
