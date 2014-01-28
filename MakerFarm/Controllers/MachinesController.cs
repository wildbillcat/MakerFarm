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
    public class MachinesController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();

        // GET: /Machines/
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

            var machines = from m in db.Machines
                           select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                machines = machines.Where(s => s.MachineName.ToUpper().Contains(searchString.ToUpper()));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    machines = machines.OrderByDescending(s => s.MachineName);
                    break;
                case "Date":
                    machines = machines.OrderBy(s => s.LastUpdated);
                    break;
                case "Name":
                    machines = machines.OrderByDescending(s => s.MachineName);
                    break;
                default:
                    machines = machines.OrderByDescending(s => s.LastUpdated);
                    break;
            }

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(machines.ToPagedList(pageNumber, pageSize));
        }

        // GET: /Machines/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Machine machine = db.Machines.Find(id);
            if (machine == null)
            {
                return HttpNotFound();
            }
            return View(machine);
        }

        // GET: /Machines/Create
        public ActionResult Create()
        {
            ViewBag.PrinterId = new SelectList(db.Printers, "PrinterId", "PrinterName");
            return View();
        }

        // POST: /Machines/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="MachineId,PrinterId,Status,idle,LastUpdated,ClientJobSupport,Enabled")] Machine machine)
        {
            if (ModelState.IsValid)
            {
                db.Machines.Add(machine);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.PrinterId = new SelectList(db.Printers, "PrinterId", "PrinterName", machine.PrinterId);
            return View(machine);
        }

        // GET: /Machines/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Machine machine = db.Machines.Find(id);
            if (machine == null)
            {
                return HttpNotFound();
            }
            ViewBag.PrinterId = new SelectList(db.Printers, "PrinterId", "PrinterName", machine.PrinterId);
            return View(machine);
        }

        // POST: /Machines/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="MachineId,PrinterId,Status,idle,LastUpdated,ClientJobSupport,Enabled")] Machine machine)
        {
            if (ModelState.IsValid)
            {
                db.Entry(machine).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.PrinterId = new SelectList(db.Printers, "PrinterId", "PrinterName", machine.PrinterId);
            return View(machine);
        }

        // GET: /Machines/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Machine machine = db.Machines.Find(id);
            if (machine == null)
            {
                return HttpNotFound();
            }
            return View(machine);
        }

        // POST: /Machines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Machine machine = db.Machines.Find(id);
            db.Machines.Remove(machine);
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
