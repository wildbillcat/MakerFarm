using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MakerFarm.Models;
using PagedList;

namespace MakerFarm.Controllers
{
    [Authorize(Roles = "Administrator, Moderator")]
    public class MachinesController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();
        private static string UnassigndPrintersSQL = "select dbo.Printers.* " +
            "from dbo.Printers left outer join dbo.Machines " +
            "on dbo.Printers.PrinterID = dbo.Machines.PrinterId " +
            "where dbo.Machines.PrinterId is null and dbo.Printers.PrinterName != 'Null Printer'";
        // GET: /Machines/
        [Authorize(Roles = "Administrator")]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QueueJob(long MId = 0)
        {
            Machine machine = db.Machines.Find(MId);
            if (machine != null && machine.AssignedJob == null && machine.AffiliatedPrinter != null)
            {
                //Printer does not have an assigned job. Lets Create one!
                Job JobAssignment = new Job();
                JobAssignment.AffiliatedPrinter = machine.AffiliatedPrinter;
                JobAssignment.AffiliatedPrint = AssignedPrint(machine.AffiliatedPrinter);
                if (JobAssignment.AffiliatedPrint == null)
                {
                    //invalid - No Print is assigned to the machines Printer! pay no need
                }else
                {
                    JobAssignment.LastUpdated = DateTime.Now;
                    JobAssignment.Status = "Print has been Assigned, waiting for Printer.";
                    JobAssignment.complete = false;
                    JobAssignment.started = false;
                    db.Jobs.Add(JobAssignment);
                    db.SaveChanges();
                    machine.AssignedJob = JobAssignment;
                    db.Entry(machine).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("Details", "Printers", new { id = machine.AffiliatedPrinter.PrinterId });
            }
            else if (machine != null && machine.AffiliatedPrinter != null)
            {
                return RedirectToAction("Details", "Printers", new { id = machine.AffiliatedPrinter.PrinterId });
            }
            else if (machine == null)
            {
                return HttpNotFound();
            }
            return RedirectToAction("Details", "Machines", new { id = machine.MachineId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelJob(long MId = 0)
        {
            Machine machine = db.Machines.Find(MId);
            if (machine != null)
            {
                //Machine isn't null, so lets operate!
                if (!machine.PoisonJobs && (machine.AssignedJob == null || !machine.AssignedJob.complete))
                {//If machine hasn't been told to poison jobs yet, so lets mark the machine so that it will cancel jobs
                    machine.PoisonJobs = true;
                    db.Entry(machine).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {//Machine was already told to poison jobs or the job was completed so unpoison the printer. Clear any assigned jobs, and be redirected to create a new Print Event if a print is assigned to the printer.
                    if (machine.AssignedJob != null)
                    {
                        machine.AssignedJob = null;
                    }
                    machine.PoisonJobs = false;
                    db.Entry(machine).State = EntityState.Modified;
                    db.SaveChanges();
                }
                //Let's Poison the Machine
                return RedirectToAction("Details", "Printers", new { id = machine.AffiliatedPrinter.PrinterId });
            }
            return RedirectToAction("Details", "Machines", new { id = machine.MachineId });
        }

        //Internal Method
        private Print AssignedPrint(Printer P)
        {
            string AssignedPrintQuery = "Select dbo.Prints.* " +
                "from dbo.Prints " +
                "left outer join " +
                "( " +
                "Select dbo.PrintEvents.PrintID, dbo.PrintEvents.EventType, dbo.PrintEvents.PrinterID " +
                "from dbo.PrintEvents " +
                "inner join " +
                "( " +
                "select dbo.PrintEvents.PrintID, MAX(dbo.PrintEvents.PrintEventId) as MostReventEvent " +
                "from dbo.PrintEvents " +
                "group by dbo.PrintEvents.PrintID " +
                ") mxe on dbo.PrintEvents.PrintId = mxe.PrintID and dbo.PrintEvents.PrintEventId = mxe.MostReventEvent " +
                ") pnt on dbo.Prints.PrintId = pnt.PrintID " +
                "where EventType = @PrintingEventStart and pnt.PrinterID = @PrinterId";
            SqlParameter PrintingEventStart = new SqlParameter("@PrintingEventStart", PrintEventType.PRINT_START);
            SqlParameter PId = new SqlParameter("@PrinterId", P.PrinterId);
            Print AssignedPrint = null;
            try
            {
                AssignedPrint = db.Prints.SqlQuery(AssignedPrintQuery, PrintingEventStart, PId).Single();
            }
            catch
            {

            }
            return AssignedPrint;
        }

        //Partial Method
        public ActionResult MachineControlPanel(long id = 0, bool MachineID = true, bool Compressed = false)
        {
            ViewData["Compressed"] = Compressed;
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Machine machine;
            if(MachineID){
                machine = db.Machines.Find(id);
            }
            else
            {
                machine = db.Machines.FirstOrDefault(p => p.AffiliatedPrinter.PrinterId == id);
            }
            if (machine == null || !machine.ClientJobSupport)
            {
                Printer PrinterAssigned = null;
                Print PrintAssigned = null;
                if (!MachineID)
                {
                    PrinterAssigned = db.Printers.Find(id);
                    PrintAssigned = AssignedPrint(PrinterAssigned);
                }
                ViewData["PrintAssigned"] = PrintAssigned;
                //return a button with the current print to update status!
                return PartialView("_ControlPanel_NoMachineAffiliated");                
            }
            if (db.Entry(machine).Reference(p => p.AssignedJob).IsLoaded == false)
            {
                db.Entry(machine).Reference(p => p.AssignedJob).Load();
            }
            if (machine.AssignedJob != null && db.Entry(machine.AssignedJob).Reference(p => p.AffiliatedPrint).IsLoaded == false)
            {
                db.Entry(machine.AssignedJob).Reference(p => p.AffiliatedPrint).Load();
            }
            ViewData["MId"] = machine.MachineId;
            ViewData["AssignedJob"] = machine.AssignedJob;
            ViewData["AssignedPrint?"] = false;
            
            if (machine.AffiliatedPrinter != null)
            {
                Print P = AssignedPrint(machine.AffiliatedPrinter);
                if (P != null)
                {
                    ViewData["AssignedPrint?"] = true;
                }
            }
            if (machine.PoisonJobs)
            {
                //Poison flag is set, so the printer is currently trying to cancel jobs
                if (machine.CurrentTaskProgress != null)
                {
                    /*
                     * Machine has been set to cancel all jobs associated with it, but they have not yet been canceled.
                     * Let user know that the machine is still in the midst of canceling jobs.
                     */
                    return PartialView("_ControlPanel_CancelingPartial");
                }
                else
                {
                    /*
                     * Machine is set to cancel jobs but it isn't working on anything and no job is assigned.
                     * Offer user ability to clear the poison flag (ie. Enable Printing)
                     */
                    /*
                     * The Printer was told to cancel all jobs, and is now no longer working on any. 
                     * There is a Job assigned to the printer however, so offer user the ability to clear the job and reset the poison flag.
                     * IE. machine.poison = false and machine.job = null
                     */
                    return PartialView("_ControlPanel_CanceledPartial");
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
                        return PartialView("_ControlPanel_IdleMachineAssignedJobPartial");
                    }
                }
                else
                {
                    /*
                     * The Machine is currently working on something, but hasn't been told to cancel it.
                     * Offer the user the option of canceling the Job on the Printer.
                     */
                    return PartialView("_ControlPanel_IdleMachineAssignedJobPartial");
                }
            }


        }

        // GET: /Machines/Details/5
        [Authorize(Roles = "Administrator")]
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

        // GET: /Machines/Edit/5
        [Authorize(Roles = "Administrator")]
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
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit([Bind(Include = "MachineName,MachineId,PrinterId,Status,idle,ClientJobSupport,Enabled,PoisonJobs")] Machine machine)
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
        [Authorize(Roles = "Administrator")]
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
        [Authorize(Roles = "Administrator")]
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
