using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MakerFarm.Models;

namespace MakerFarm.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ClientPermissionsController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();

        // GET: /ClientPermissions/
        public ActionResult Index()
        {
            return View(db.ClientPermissions.Where(p => p.Client == null || p.Machine == null).ToList());
        }

        // GET: /ClientPermissions/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClientPermission clientpermission = db.ClientPermissions.Find(id);
            if (clientpermission == null)
            {
                return HttpNotFound();
            }
            return View(clientpermission);
        }

        // GET: /ClientPermissions/Create
        public ActionResult Create(int id)
        {
            Client C = db.Clients.Find(id);
            if (C == null)
            {
                return HttpNotFound();
            }
            string sql = "select * " +
                    "from dbo.Machines left outer join ( " + 
                    "select * " +
                    "from dbo.ClientPermissions " +
                    ") " +
                    "dur on dbo.Machines.MachineId = dur.Machine_MachineId " +
                    "where dur.Machine_MachineId is null";
            ViewBag.PrinterSelect = new SelectList(db.Machines.SqlQuery(sql), "MachineId", "MachineName");
            ViewBag.Client = C;
            return View();
        }

        // POST: /ClientPermissions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="ClientPermissionId,GetInformation,SetInformation")] ClientPermission clientpermission, int ClientId, long MachineId)
        {
            clientpermission.Machine = db.Machines.Find(MachineId);
            if (clientpermission.Machine == null)
            {
                ModelState.AddModelError("MachineId", "Invalid Machine was selected");
            }
            clientpermission.Client = db.Clients.Find(ClientId);
            if (clientpermission.Client == null)
            {
                ModelState.AddModelError("ClientId", "Invalid Client was selected");
            }
            if (ModelState.IsValid)
            {
                db.ClientPermissions.Add(clientpermission);
                db.SaveChanges();
                return RedirectToAction("Details", "Clients", new { id = ClientId});
            }
            Client C = clientpermission.Client;
            if (C == null)
            {
                return HttpNotFound();
            }
            string sql = "select dbo.Machines.* " +
                    "from dbo.Machines left outer join ( " +
                    "select * " +
                    "from dbo.ClientPermissions " +
                    "where dbo.ClientPermissions.Client_ClientId = " + C.ClientId + ") " +
                    "dur on dbo.Machines.MachineId = dur.Machine_MachineId " +
                    "where dur.Machine_MachineId is null";
            ViewBag.PrinterSelect = new SelectList(db.Machines.SqlQuery(sql), "MachineId", "MachineName");
            ViewBag.Client = C;
            return View(clientpermission);
        }

        // GET: /ClientPermissions/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClientPermission clientpermission = db.ClientPermissions.Find(id);
            if (clientpermission == null)
            {
                return HttpNotFound();
            }
            return View(clientpermission);
        }

        // POST: /ClientPermissions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="ClientPermissionId,GetInformation,SetInformation")] ClientPermission clientpermission, int ClientId, long MachineId)
        {
            clientpermission.Machine = db.Machines.Find(MachineId);
            clientpermission.Client = db.Clients.Find(ClientId);
            if (clientpermission.Machine == null)
            {
                ModelState.AddModelError("MachineId", "Invalid Machine was selected");
            }
            if (clientpermission.Client == null)
            {
                ModelState.AddModelError("ClientId", "Invalid Client was selected");
            }
            if (ModelState.IsValid)
            {
                db.Entry(clientpermission).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", "Clients", new { id = ClientId });
            }
            return View(clientpermission);
        }

        // GET: /ClientPermissions/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClientPermission clientpermission = db.ClientPermissions.Find(id);
            if (clientpermission == null)
            {
                return HttpNotFound();
            }
            return View(clientpermission);
        }

        // POST: /ClientPermissions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            ClientPermission clientpermission = db.ClientPermissions.Find(id);
            int ClientId = clientpermission.Client.ClientId;
            db.ClientPermissions.Remove(clientpermission);
            db.SaveChanges();
            return RedirectToAction("Details", "Clients", new { id = ClientId });
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
