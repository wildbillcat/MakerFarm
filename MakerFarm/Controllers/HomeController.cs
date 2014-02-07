using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MakerFarm.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("SubmissionSelection", "PrinterTypes");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
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
