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
    public class PrintEventsController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();

        // GET: /PrintEvents/
        public ActionResult Index()
        {
            var printevents = db.PrintEvents.Include(p => p.Print).Include(p => p.Printer);
            return View(printevents.ToList());
        }

        // GET: /PrintEvents/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrintEvent printevent = db.PrintEvents.Find(id);
            if (printevent == null)
            {
                return HttpNotFound();
            }
            return View(printevent);
        }

        // GET: /PrintEvents/Create
        public ActionResult Create(long id = 0)
        {
            Print Print = db.Prints.Find(id);
            List<PrintEvent> LastStatus = db.PrintEvents.Where(p => p.PrintId.Equals(id)).ToList();
            ViewBag.PrintId = id;
            ViewBag.Print = Print;
            List<PrintEventType> evts = new List<PrintEventType>();
            evts.Add(PrintEventType.PRINT_START);
            SelectList PrinterIds;
            if (0 == LastStatus.Count() || !LastStatus.Last().Equals(PrintEventType.PRINT_START)) //Print Needs to be Sent!
            {
                //This is in need of some query optimization!
                List<Printer> PrinterList = new List<Printer>();
                List<Printer> PrinterList2 = db.Printers.Where(p => p.PrinterTypeId.Equals(Print.PrinterTypeId)).ToList();
                foreach (Printer Pter in PrinterList2)
                {
                    try
                    {
                        PrinterStatusLog status = db.PrinterStatusLogs.Last(p => p.PrinterId.Equals(Pter.PrinterId));
                        if (status.LoggedPrinterStatus.Equals(PrinterStatus.Online))
                        {
                            PrinterList.Add(Pter);
                        }
                    }
                    catch (Exception e) { }
                }
                PrinterIds = new SelectList(PrinterList, "PrinterId", "PrinterName");
            }
            else //Print was sent, updating status of print
            {
                List<SelectListItem> Printerlist = new List<SelectListItem>();
                Printerlist.Add(new SelectListItem() { Text = LastStatus.Last().Printer.PrinterName, Value = LastStatus.Last().Printer.PrinterId.ToString(), Selected = true });
                PrinterIds = new SelectList(Printerlist, "Value", "Text");
                evts = Enum.GetValues(typeof(MakerFarm.Models.PrintEventType)).Cast<MakerFarm.Models.PrintEventType>().ToList();
                evts.Remove(PrintEventType.PRINT_START);
            }

            ViewBag.PrinterId = PrinterIds;
            ViewBag.EventTypes = evts;
            return View();
        }

        // POST: /PrintEvents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="PrintEventId,EventType,MaterialUsed,PrinterId,UserName,PrintId")] PrintEvent printevent)
        {
            printevent.EventTimeStamp = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.PrintEvents.Add(printevent);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.PrintId = new SelectList(db.Prints, "PrintId", "FileName", printevent.PrintId);
            ViewBag.PrinterId = new SelectList(db.Printers, "PrinterId", "PrinterName", printevent.PrinterId);
            return View(printevent);
        }

        // GET: /PrintEvents/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrintEvent printevent = db.PrintEvents.Find(id);
            if (printevent == null)
            {
                return HttpNotFound();
            }
            ViewBag.PrintId = new SelectList(db.Prints, "PrintId", "FileName", printevent.PrintId);
            ViewBag.PrinterId = new SelectList(db.Printers, "PrinterId", "PrinterName", printevent.PrinterId);
            return View(printevent);
        }

        // POST: /PrintEvents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="PrintEventId,EventType,EventTimeStamp,MaterialUsed,PrinterId,UserName,PrintId")] PrintEvent printevent)
        {
            if (ModelState.IsValid)
            {
                db.Entry(printevent).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.PrintId = new SelectList(db.Prints, "PrintId", "FileName", printevent.PrintId);
            ViewBag.PrinterId = new SelectList(db.Printers, "PrinterId", "PrinterName", printevent.PrinterId);
            return View(printevent);
        }

        // GET: /PrintEvents/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrintEvent printevent = db.PrintEvents.Find(id);
            if (printevent == null)
            {
                return HttpNotFound();
            }
            return View(printevent);
        }

        // POST: /PrintEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            PrintEvent printevent = db.PrintEvents.Find(id);
            db.PrintEvents.Remove(printevent);
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
