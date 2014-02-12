using MakerFarm.Models;
using System.Web.Http;
using System.Web.Http.OData.Builder;

namespace MakerFarm
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Client>("ClientsAPI");
            
             // New code: Add an action to the EDM, and define the parameter and return type.
            ActionConfiguration ISpy = builder.Entity<Client>().Action("ISpy");
            ISpy.Parameter<string>("ClientAPIKey");
            ISpy.CollectionParameter<string>("Machines");

            ActionConfiguration DoTell = builder.Entity<Client>().Action("DoTell");
            DoTell.Parameter<string>("ClientAPIKey");
            DoTell.ReturnsCollection<MachineInterest>();

            ActionConfiguration ISay = builder.Entity<Client>().Action("ISay");
            ISay.Parameter<string>("ClientAPIKey");
            ISay.Parameter<MachineStatusUpdate>("MachineUpdate");
            ISay.Parameter<JobStatusUpdate>("JobUpdate");

            ActionConfiguration TakeThis = builder.Entity<Client>().Action("TakeThis");
            TakeThis.Parameter<string>("ClientAPIKey");
            TakeThis.Parameter<string>("MachineName");
            TakeThis.Parameter<int>("JobId");
            TakeThis.Returns<System.Net.Http.StreamContent>();

            config.Routes.MapODataRoute("odata", "odata", builder.GetEdmModel());
            // Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
            // To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
            // For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
            //config.EnableQuerySupport();
        }
    }
}