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
        private static string UnassigndPrintersSQL = "select dbo.Printers.* " +
            "from dbo.Printers left outer join dbo.Machines " +
            "on dbo.Printers.PrinterID = dbo.Machines.PrinterId " +
            "where dbo.Machines.PrinterId is null and dbo.Printers.PrinterName != 'Null Printer'";
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

        public ActionResult MachineControlPanel(long id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Machine machine = db.Machines.Find(id);
            if (machine == null)
            {
                return HttpNotFound();
            }
            ViewData["MachineId"] = machine.MachineId;
            if (machine.PoisonJobs)
            {
                //Poison flag is set, so the printer is currently trying to cancel jobs
                if (machine.CurrentTaskProgress != null)
                {
                    /*
                     * Machine has been set to cancel all jobs associated with it, but they have not yet been canceled.
                     * Let user know that the machine is still in the midst of canceling jobs.
                     */

                }
                else if (machine.AssignedJob == null)
                {
                    /*
                     * Machine is set to cancel jobs but it isn't working on anything and no job is assigned.
                     * Offer user ability to clear the poison flag (ie. Enable Printing)
                     */
                }
                else
                {
                    /*
                     * The Printer was told to cancel all jobs, and is now no longer working on any. 
                     * There is a Job assigned to the printer however, so offer user the ability to clear the job and reset the poison flag.
                     * IE. machine.poison = false and machine.job = null
                     */
                }
            }else{
                //Poison flag has not been set on the printer, so jobs can be sent.
                if (machine.CurrentTaskProgress == null)
                {
                    if (machine.AssignedJob == null)
                    {
                        /*
                         * The Machine is currently Idle
                         * Offer the User the ability to send a job
                         */
                        return PartialView("_ControlPanel_IdleMachineUnassignedJobPartial");
                    }
                    else
                    {
                        /*
                         * The Machine is Idle, but a Job was assigned.
                         * Machine should be starting the Job shortly. Allow user to cancel Job if they so wish.
                         */
                        ViewData["AssignedJob"] = machine.AssignedJob;
                        return PartialView("_ControlPanel_IdleMachineAssignedJobPartial");
                    }
                }
                else
                {
                    /*
                     * The Machine is currently working on something, but hasn't been told to cancel it.
                     * Offer the user the option of canceling the Job on the Printer.
                     */
                }
            }


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

        /*
        // GET: /Machines/Create
        public ActionResult Create()
        {
            ViewBag.PrinterId = new SelectList(db.Printers.SqlQuery(UnassigndPrintersSQL), "PrinterId", "PrinterName");
            return View();
        }

        // POST: /Machines/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MachineId,MachineName,PrinterId,Status,idle,ClientJobSupport,Enabled")] Machine machine)
        {
            machine.LastUpdated = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.Machines.Add(machine);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.PrinterId = new SelectList(db.Printers.SqlQuery(UnassigndPrintersSQL), "PrinterId", "PrinterName", machine.PrinterId);
            return View(machine);
        }*/

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
            string psql = UnassigndPrintersSQL;
            if(machine.PrinterId != null){
                psql = psql + "  or dbo.Printers.PrinterID = " + machine.PrinterId;
            }
            ViewBag.PrinterId = new SelectList(db.Printers.SqlQuery(psql), "PrinterId", "PrinterName", machine.PrinterId);
            return View(machine);
        }

        // POST: /Machines/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MachineName,MachineId,PrinterId,Status,idle,ClientJobSupport,Enabled")] Machine machine)
        {
            machine.LastUpdated = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.Entry(machine).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            string psql = UnassigndPrintersSQL;
            if (machine.PrinterId != null)
            {
                psql = psql + "  or dbo.Printers.PrinterID = " + machine.PrinterId;
            }
            ViewBag.PrinterId = new SelectList(db.Printers.SqlQuery(psql), "PrinterId", "PrinterName", machine.PrinterId);
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
