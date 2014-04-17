using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MakerFarm.Models;
using PagedList;

namespace MakerFarm.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ClientsController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();

        // GET: /Clients/
        public ActionResult Index(int? page, string sortOrder, string currentFilter, string searchString)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.MachineIdSortParm = sortOrder == "Name" ? "name_desc" : "Name";
            ViewBag.LastUpdatedSortParm = String.IsNullOrEmpty(sortOrder) ? "" : "Date";


            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var clients = from c in db.Clients
                           select c;

            if (!String.IsNullOrEmpty(searchString))
            {
                clients = clients.Where(s => s.ClientName.ToUpper().Contains(searchString.ToUpper()));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    clients = clients.OrderByDescending(s => s.ClientName);
                    break;
                case "Date":
                    clients = clients.OrderBy(s => s.LastUpdated);
                    break;
                case "Name":
                    clients = clients.OrderByDescending(s => s.ClientName);
                    break;
                default:
                    clients = clients.OrderByDescending(s => s.LastUpdated);
                    break;
            }

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(clients.ToPagedList(pageNumber, pageSize));
        }

        // GET: /Clients/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Client client = db.Clients.Find(id);
            if (client == null)
            {
                return HttpNotFound();
            }
            return View(client);
        }

        // GET: /Clients/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Clients/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="ClientId,ClientName,ClientAPIKey,Enabled")] Client client)
        {
            client.LastUpdated = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.Clients.Add(client);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(client);
        }

        // GET: /Clients/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Client client = db.Clients.Find(id);
            if (client == null)
            {
                return HttpNotFound();
            }
            return View(client);
        }

        // POST: /Clients/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="ClientId,ClientName,ClientAPIKey,Enabled")] Client client)
        {
            client.LastUpdated = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.Entry(client).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(client);
        }

        // GET: /Clients/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Client client = db.Clients.Find(id);
            if (client == null)
            {
                return HttpNotFound();
            }
            return View(client);
        }

        // POST: /Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Client client = db.Clients.Find(id);
            foreach (ClientPermission p in client.ClientPermissions)
            {
                db.ClientPermissions.Remove(p);
            }
            db.Clients.Remove(client);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
