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
    [Authorize(Roles = "Administrator, Moderator")]
    public class PrinterStatusLogsController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();

        // GET: /PrintStatusLogs/
        public ActionResult Index()
        {
            Dictionary<long, Printer> PrinterNames = db.Printers.ToDictionary(p => p.PrinterId);
            ViewBag.PrinterNames = PrinterNames;
            return View(db.PrinterStatusLogs.ToList());
        }

        // GET: /PrintStatusLogs/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrinterStatusLog printerstatuslog = db.PrinterStatusLogs.Find(id);
            if (printerstatuslog == null)
            {
                return HttpNotFound();
            }
            return View(printerstatuslog);
        }

        // GET: /PrintStatusLogs/Create
        public ActionResult Create(long id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Index", "PrinterTypes");
            }
            ViewData["PrinterID"] = id;
            Printer printer = db.Printers.Find(id);
            if (printer == null)
            {
                return HttpNotFound();
            }
            ViewBag.Title = string.Concat("Update Status: ", printer.PrinterName);
            return View();
        }

        // POST: /PrintStatusLogs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="PrinterStatusLogId,LoggedPrinterStatus,Comment,PrinterId")] PrinterStatusLog printerstatuslog)
        {
            printerstatuslog.LogEntryDate = DateTime.Now;
            printerstatuslog.Comment = string.Concat(User.Identity.Name, " : ", printerstatuslog.Comment);
            Printer PrinterInQuestion = db.Printers.Find(printerstatuslog.PrinterId);
            if (PrinterInQuestion.PrinterName.Equals("Null Printer"))
            {
                ModelState.AddModelError("PrinterId", new Exception("Sorry, this is a Special Internal Name for Makerfarm. Please choose something else."));
            }
            if (ModelState.IsValid)
            {
                db.PrinterStatusLogs.Add(printerstatuslog);
                db.SaveChanges();
                return RedirectToAction("Details", "Printers", new { id = printerstatuslog.PrinterId });
            }

            return View(printerstatuslog);
        }

        // GET: /PrintStatusLogs/Edit/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrinterStatusLog printerstatuslog = db.PrinterStatusLogs.Find(id);
            if (printerstatuslog == null)
            {
                return HttpNotFound();
            }
            return View(printerstatuslog);
        }

        // POST: /PrintStatusLogs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="PrinterStatusLogId,LogEntryDate,LoggedPrinterStatus,Comment,PrinterId")] PrinterStatusLog printerstatuslog)
        {
            Printer PrinterInQuestion = db.Printers.Find(printerstatuslog.PrinterId);
            if (PrinterInQuestion.PrinterName.Equals("Null Printer"))
            {
                ModelState.AddModelError("PrinterId", new Exception("Sorry, this is a Special Internal Printer for Makerfarm. Please choose something else."));
            }
            if (ModelState.IsValid)
            {
                db.Entry(printerstatuslog).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(printerstatuslog);
        }

        // GET: /PrintStatusLogs/Delete/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrinterStatusLog printerstatuslog = db.PrinterStatusLogs.Find(id);
            if (printerstatuslog == null)
            {
                return HttpNotFound();
            }
            return View(printerstatuslog);
        }

        // POST: /PrintStatusLogs/Delete/5
        [Authorize(Roles = "Administrator")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            PrinterStatusLog printerstatuslog = db.PrinterStatusLogs.Find(id);
            db.PrinterStatusLogs.Remove(printerstatuslog);
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
