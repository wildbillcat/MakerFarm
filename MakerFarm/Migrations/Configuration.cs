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
            string CreateAdministratorRoleSQL = "Insert Into dbo.webpages_Roles(dbo.webpages_Roles.RoleName) values('Administrator')";
            string CreateModeratorRoleSQL = "Insert Into dbo.webpages_Roles(dbo.webpages_Roles.RoleName) values('Moderator')";
            try
            {
                context.Database.ExecuteSqlCommand(CreateAdministratorRoleSQL);
            }
            catch (Exception e)
            {

            }
            try
            {
                context.Database.ExecuteSqlCommand(CreateModeratorRoleSQL);
            }
            catch (Exception e)
            {

            }
        }
    }
}
