namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCommentfieldtothePrinterType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PrinterTypes", "CommentField", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PrinterTypes", "CommentField");
        }
    }
}
