using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
       
        // GET odata/ClientsAPI
        [Queryable]
        public IQueryable<Client> GetClientsAPI()
        {
            //Narrow by authenticated user's access
            return db.Clients.Where(client => client.ClientUserName.Equals(User.Identity.Name));
        }

        // GET odata/ClientsAPI(5)
        [Queryable]
        public SingleResult<Client> GetClient([FromODataUri] int key)
        {
            //Narrow by authenticated user's access
            return SingleResult.Create(db.Clients.Where(client => client.ClientId == key && client.ClientUserName.Equals(User.Identity.Name)));
        }

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

        // POST odata/ClientsAPI(5)/ISpy
        /*
         * This method is used to allow a client to report what machines it sees to Makerfarm, 
         * populating the printers and improving the ease of configuration. 
         */
        [HttpPost]
        public  IHttpActionResult ISpy([FromODataUri] int key, ODataActionParameters parameters)
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
            if (!Client.ClientAPIKey.Equals(ClientAPIKey))
            {
                //API Key is invalid reject request
                return BadRequest();
            }
            Client.LastUpdated = DateTime.Now;
            db.Entry(Client).State = EntityState.Modified;


            //List of the Machines 
            IEnumerable<string> machlist = (IEnumerable<string>)parameters["Machines"];
            string[] Machines = machlist.ToArray();
            bool[] exists = new bool[Machines.Length];
            for(int i = 0; i < Machines.Length; i++)
            {
                string Name = Machines[i];
                exists[i] = db.Machines.Any(p => p.MachineName.Equals(Name));               
            }
            for(int i = 0; i < Machines.Length; i++)
            {
                bool existence = exists[i];
                if (!existence) //If the machine isn't known to Makerfarm, 
                {
                    Machine Mach = new Machine();
                    Mach.MachineName = Machines[i];
                    Mach.PrinterId = null;
                    Mach.Status = "Unknown\nReported By: " + Client.ClientName;
                    Mach.idle = true;
                    Mach.LastUpdated = DateTime.Now;
                    Mach.ClientJobSupport = false;
                    Mach.Enabled = false;
                    Mach.CurrentTaskProgress = null;
                    Mach.PauseMachine = MachinePause.ActivePrinting;
                    Mach.Print_Cancel = false;
                    Mach.Print_Pause = false;
                    Mach.Print_Resume = false;
                    Mach.Print_Send = false;
                    db.Machines.Add(Mach);
                }
            }                        
            db.SaveChanges();
            return Ok();
        }

        // POST odata/ClientsAPI(5)/DoTell
        /*
         * This method is used to have MakerFarm tell the Client what it is interested in (Jobs, Machines)
         */
        public MachineInterest[] DoTell([FromODataUri] int key, ODataActionParameters parameters)
        {
            Client Me =  db.Clients.Find(key);
            if (Me == null || !Me.Enabled) //Client Doesnt exist or isnt enabled
            {
                return new List<MachineInterest>().ToArray();
            }
            string ClientAPIKey = (string)parameters["ClientAPIKey"];
            if (!Me.ClientAPIKey.Equals(ClientAPIKey))
            {
                //API Key is invalid reject request
                return new MachineInterest[0];
            }
            List<MachineInterest> Machines = new List<MachineInterest>();
            foreach (ClientPermission P in Me.ClientPermissions)
            {
                if (P.Machine.Enabled)
                {
                    if (db.Entry(P).Reference(p => p.Machine).IsLoaded == false)
                    {
                        db.Entry(P).Reference(p => p.Machine).Load();
                    }
                    
                    if (db.Entry(P.Machine).Reference(p => p.AssignedJob).IsLoaded == false)
                    {
                        db.Entry(P.Machine).Reference(p => p.AssignedJob).Load();
                    }
                    if (P.Machine.AssignedJob != null && db.Entry(P.Machine.AssignedJob).Reference(p => p.AffiliatedPrint).IsLoaded == false)
                    {
                        db.Entry(P.Machine.AssignedJob).Reference(p => p.AffiliatedPrint).Load();
                    }
                    Machines.Add(P.Machine.GetMachineInterest());
                }
            }
            return Machines.ToArray();
        }

        // POST odata/ClientsAPI(5)/ISay
        /*
         * This method is used to have MakerFarm tell the Client what it is interested in (Jobs, Machines)
         */
        public IHttpActionResult ISay([FromODataUri] int key, ODataActionParameters parameters)
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
            if (!Client.ClientAPIKey.Equals(ClientAPIKey))
            {
                //API Key is invalid reject request
                return BadRequest();
            }
            Client.LastUpdated = DateTime.Now;
            db.Entry(Client).State = EntityState.Modified;
            MachineStatusUpdate MachineUpdate = (MachineStatusUpdate)parameters["MachineUpdate"];
            JobStatusUpdate JobUpdate = (JobStatusUpdate)parameters["JobUpdate"];
            ClientPermission P = Client.ClientPermissions.FirstOrDefault(p => p.Machine.MachineName.Equals(MachineUpdate.MachineName) && p.SetInformation);
            if (P != null || P.Machine != null)
            {
                Machine M = P.Machine;
                M.Status = MachineUpdate.MachineStatus;
                M.LastUpdated = DateTime.Now;
                M.CurrentTaskProgress = MachineUpdate.CurrentTaskProgress;
                if (M.AssignedJob != null && JobUpdate.JobId == M.AssignedJob.JobId)
                {
                    Job J = M.AssignedJob;
                    J.Status = JobUpdate.Status;
                    J.started = JobUpdate.started;
                    J.complete = JobUpdate.complete;
                    J.LastUpdated = DateTime.Now;
                    db.Entry(J).State = EntityState.Modified;
                }
                db.Entry(M).State = EntityState.Modified;
            }
            db.SaveChanges();
            return Ok();
        }

        public HttpResponseMessage TakeThis([FromODataUri] int key, ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return null;
                //return BadRequest();
           } 
            Client Client = db.Clients.Find(key);
            if (Client == null || !Client.Enabled) //Client Doesnt exist or isnt enabled
            {
                return null;
                //return NotFound();
            }
            string ClientAPIKey = (string)parameters["ClientAPIKey"];
            if (!Client.ClientAPIKey.Equals(ClientAPIKey))
            {
                return null;
                //API Key is invalid reject request
                //return BadRequest();
            }
            Client.LastUpdated = DateTime.Now;
            db.Entry(Client).State = EntityState.Modified;
            int JobId = (int)parameters["JobId"];
            string MachineName = (string)parameters["MachineName"];
            try{
                Job jobrequested = db.Jobs.Find(JobId);
                if (db.Entry(jobrequested).Reference(p => p.AffiliatedPrint).IsLoaded == false)
                {
                    db.Entry(jobrequested).Reference(p => p.AffiliatedPrint).Load();
                }
                string fileLocation = jobrequested.AffiliatedPrint.GetPath();
                //string fileLocation = db.Prints.Find(53).GetPath();
                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                System.IO.FileStream stream = new System.IO.FileStream(fileLocation, System.IO.FileMode.Open);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                return result;
            }
            catch { 
                return null;
                //return NotFound(); 
            }
        }

        // POST odata/ClientsAPI(5)/CanDo
        /*
         * This method is used to allow a client to report what machines it sees to Makerfarm, 
         * populating the printers and improving the ease of configuration. 
         */
        [HttpPost]
        public IHttpActionResult CanDo([FromODataUri] int key, ODataActionParameters parameters)
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
            if (!Client.ClientAPIKey.Equals(ClientAPIKey))
            {
                //API Key is invalid reject request
                return BadRequest();
            }
            //List of the Machines 
            IEnumerable<RancherCommandGlossary> machlist = (IEnumerable<RancherCommandGlossary>)parameters["Machines"];
            RancherCommandGlossary[] MachineAbilities = machlist.ToArray();

            return Ok();
        }

    }
}
