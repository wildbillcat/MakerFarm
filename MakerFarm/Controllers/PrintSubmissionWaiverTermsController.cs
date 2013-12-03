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
    public class PrintSubmissionWaiverTermsController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();

        // GET: /PrintSubmissionWaiverTerm/
        public ActionResult Index()
        {
            return View(db.PrintSubmissionWaiverTerms.ToList());
        }

        // GET: /PrintSubmissionWaiverTerm/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrintSubmissionWaiverTerm printsubmissionwaiverterm = db.PrintSubmissionWaiverTerms.Find(id);
            if (printsubmissionwaiverterm == null)
            {
                return HttpNotFound();
            }
            return View(printsubmissionwaiverterm);
        }

        // GET: /PrintSubmissionWaiverTerm/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /PrintSubmissionWaiverTerm/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="PrintSubmissionWaiverTermId,Enabled,WaiverText")] PrintSubmissionWaiverTerm printsubmissionwaiverterm)
        {
            if (ModelState.IsValid)
            {
                db.PrintSubmissionWaiverTerms.Add(printsubmissionwaiverterm);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(printsubmissionwaiverterm);
        }

        // GET: /PrintSubmissionWaiverTerm/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrintSubmissionWaiverTerm printsubmissionwaiverterm = db.PrintSubmissionWaiverTerms.Find(id);
            if (printsubmissionwaiverterm == null)
            {
                return HttpNotFound();
            }
            return View(printsubmissionwaiverterm);
        }

        // POST: /PrintSubmissionWaiverTerm/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="PrintSubmissionWaiverTermId,Enabled,WaiverText")] PrintSubmissionWaiverTerm printsubmissionwaiverterm)
        {
            if (ModelState.IsValid)
            {
                db.Entry(printsubmissionwaiverterm).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(printsubmissionwaiverterm);
        }

        // GET: /PrintSubmissionWaiverTerm/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrintSubmissionWaiverTerm printsubmissionwaiverterm = db.PrintSubmissionWaiverTerms.Find(id);
            if (printsubmissionwaiverterm == null)
            {
                return HttpNotFound();
            }
            return View(printsubmissionwaiverterm);
        }

        // POST: /PrintSubmissionWaiverTerm/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PrintSubmissionWaiverTerm printsubmissionwaiverterm = db.PrintSubmissionWaiverTerms.Find(id);
            db.PrintSubmissionWaiverTerms.Remove(printsubmissionwaiverterm);
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
