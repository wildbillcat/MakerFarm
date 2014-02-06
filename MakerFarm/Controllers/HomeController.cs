using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MakerFarm.Models;

namespace MakerFarm.Controllers
{
    public class HomeController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();

        public ActionResult Index()
        {
            return RedirectToAction("SubmissionSelection", "PrinterTypes");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult PublicStatus()
        {
            List<PrinterType> Types = db.PrinterTypes.Where(p => p.QueueVisible).ToList();
            ViewData["Types"] = Types;
            return PartialView();
        }

        [Authorize(Roles = "Moderator, Administrator")]
        public ActionResult Administration()
        {
            return View();
        }

        public ActionResult Denied()
        {
            ViewBag.Message = "You do not have access to this page.";

            return View();
        }
    }
}
