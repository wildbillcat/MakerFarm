using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.OData;
using System.Web.Http.OData.Routing;
using MakerFarm.Models;

namespace MakerFarm.Controllers
{
    /*
    To add a route for this controller, merge these statements into the Register method of the WebApiConfig class. Note that OData URLs are case sensitive.

    using System.Web.Http.OData.Builder;
    using MakerFarm.Models;
    ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
    builder.EntitySet<Client>("ClientsAPI");
    config.Routes.MapODataRoute("odata", "odata", builder.GetEdmModel());
    */
    public class ClientsAPIController : ODataController
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();
       // /*
        // GET odata/ClientsAPI
        public List<string> GetClientsAPI()
        {
            List<string> names = new List<string>();
            foreach(Client C in db.Clients){
                names.Add(C.ClientName);
            }
            return names;
        }

        // GET odata/ClientsAPI(5)
        
        public string GetClient([FromODataUri] int key)
        {
            return db.Clients.Find(key).ClientName;
        }
        /*
        // PUT odata/ClientsAPI(5)
        public async Task<IHttpActionResult> Put([FromODataUri] int key, Client client)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != client.ClientId)
            {
                return BadRequest();
            }

            db.Entry(client).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(client);
        }

        // POST odata/ClientsAPI
        public async Task<IHttpActionResult> Post(Client client)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Clients.Add(client);
            await db.SaveChangesAsync();

            return Created(client);
        }

        // PATCH odata/ClientsAPI(5)
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<Client> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Client client = await db.Clients.FindAsync(key);
            if (client == null)
            {
                return NotFound();
            }

            patch.Patch(client);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(client);
        }

        // DELETE odata/ClientsAPI(5)
        public async Task<IHttpActionResult> Delete([FromODataUri] int key)
        {
            Client client = await db.Clients.FindAsync(key);
            if (client == null)
            {
                return NotFound();
            }

            db.Clients.Remove(client);
            await db.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }//*/

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ClientExists(int key)
        {
            return db.Clients.Count(e => e.ClientId == key) > 0;
        }

        [HttpPost]
        public async Task<IHttpActionResult> ISpy([FromODataUri] int key, ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            Client Client = db.Clients.Find(key);
            if (Client == null || !Client.Enabled) //Client Doesnt exist or isnt enabled
            {
                return NotFound();
            }
            string ClientAPIKey = (string)parameters["ClientAPIKey"];
            /*
             * Removed for testing purposes without any authentication
            if (!User.Identity.IsAuthenticated || !User.Identity.Name.Equals(Client.ClientUserName) || !ClientAPIKey.Equals(Client.ClientAPIKey)) //User isn't authenticated or They do not have access to this particular Client or the Client API Key is wrong
            {
                return NotFound();
            }*/

            //List of the Machines 
            IEnumerable<string> machlist = (IEnumerable<string>)parameters["Machines"];
            string[] Machines = machlist.ToArray();
            Task<bool>[] exists = new Task<bool>[Machines.Length];
            for(int i = 0; i < Machines.Length; i++)
            {
                string Name = Machines[i];
                exists[i] = db.Machines.AnyAsync(p => p.MachineName.Equals(Name));               
            }
            bool sync = false;
            for(int i = 0; i < Machines.Length; i++)
            {
                bool existence = await exists[i];
                if (!existence) //If the machine isn't known to Makerfarm, 
                {
                    sync = true;
                    Machine Mach = new Machine();
                    Mach.MachineName = Machines[i];
                    Mach.PrinterId = null;
                    Mach.Status = "Unknown\nReported By: " + Client.ClientName;
                    Mach.idle = true;
                    Mach.LastUpdated = DateTime.Now;
                    Mach.ClientJobSupport = false;
                    Mach.Enabled = false;
                    db.Machines.Add(Mach);
                }
            }

            if (sync)
            {
                await db.SaveChangesAsync();
            }

            /*
            Product product = await db.Products.FindAsync(key);
            if (product == null)
            {
                return NotFound();
            }

            product.Ratings.Add(new ProductRating() { Rating = rating });
            db.SaveChanges();

            double average = product.Ratings.Average(x => x.Rating);

            return Ok(average);
             * */
            return Ok();
        }
    }
}
