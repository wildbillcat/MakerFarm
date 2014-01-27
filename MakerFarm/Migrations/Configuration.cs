namespace MakerFarm.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<MakerFarm.Models.MakerfarmDBContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;

        }

        protected override void Seed(MakerFarm.Models.MakerfarmDBContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            MakerFarm.Models.PrinterType NullType = new MakerFarm.Models.PrinterType
            {
                TypeName = "Null Printer",
                SupportedNumberMaterials = 1,
                MaterialUseUnit = "Photons",
                PrinterIcon = "/Content/StaticIcons/Question.png",
                MaxNumberUserAttempts = 1,
                SupportedFileTypes = "RadMat",
                CommentField = "This is a Null Type printer, who exists as as an internal data structure for the application. Pay no Heed.",
                AboutPrinter = "This is a Null Type printer, who exists as as an internal data structure for the application. Pay no Heed.",
                HyperLink = "",
                MaximumNumberOfCopies = 1,
                BuildLength = 0,
                BuildWidth = 0,
                BuildHeight = 0,
                OffersBreakawaySupport = false,
                OffersNonBreakAwaySupport = false,
                BuildSupportUsesMaterialSlot = false,
                OffersFullColorPrinting = false,
                FunctionalModelSupport = false,
                QueueVisible = false,
                SubmissionEnabled = false
            };
            context.PrinterTypes.AddOrUpdate(p => p.TypeName, NullType);
            MakerFarm.Models.Printer NullPrinter = new MakerFarm.Models.Printer
            {
                PrinterName = "Null Printer",
                PrinterTypeId = NullType.PrinterTypeId
            };
            context.Printers.AddOrUpdate(p => p.PrinterName, NullPrinter);
            string CreateAdministratorRoleSQL = "Insert Into dbo.webpages_Roles(dbo.webpages_Roles.RoleName) values('Administrator')";
            string CreateModeratorRoleSQL = "Insert Into dbo.webpages_Roles(dbo.webpages_Roles.RoleName) values('Moderator')";
            try
            {
                context.Database.ExecuteSqlCommand(CreateAdministratorRoleSQL);
            }
            catch
            {

            }
            try
            {
                context.Database.ExecuteSqlCommand(CreateModeratorRoleSQL);
            }
            catch
            {

            }
        }
    }
}
