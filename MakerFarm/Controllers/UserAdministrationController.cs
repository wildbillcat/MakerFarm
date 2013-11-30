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
    [Authorize]
    public class UserAdministrationController : Controller
    {
        private MakerfarmDBContext db = new MakerfarmDBContext();

        // GET: /UserAdministration/
        public ActionResult Index()
        {
            return View(db.UserProfiles.ToList());
        }

        // GET: /UserAdministration/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserProfile userprofile = db.UserProfiles.Find(id);
            if (userprofile == null)
            {
                return HttpNotFound();
            }
            List<webpages_Role> Roles = db.Database.SqlQuery<webpages_Role>("Select * From dbo.webpages_Roles", id).ToList();
            ViewData["Roles"] = Roles;
            List<webpages_UsersInRole> UserRoles = db.Database.SqlQuery<webpages_UsersInRole>("Select * From dbo.webpages_UsersInRoles Where dbo.webpages_UsersInRoles.RoleId = {0}", id).ToList();
            ViewData["UserRoles"] = UserRoles;
            return View(userprofile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUserToRole(FormCollection values)
        {
            int UserId;
            string Group;
            List<webpages_Role> Roles = db.Database.SqlQuery<webpages_Role>("Select * From dbo.webpage_Roles", null).ToList();
            return View();
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
