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
            Dictionary<int, webpages_Role> RolesMaster = db.Database.SqlQuery<webpages_Role>("Select * From dbo.webpages_Roles", id).ToDictionary(p => p.RoleId);
            List<webpages_UsersInRole> UserRoles = db.Database.SqlQuery<webpages_UsersInRole>("Select * From dbo.webpages_UsersInRoles Where dbo.webpages_UsersInRoles.UserId = {0}", id).ToList();
            foreach (webpages_UsersInRole UserRole in UserRoles)
            {
                Roles.Remove(Roles.Find(p => p.RoleId == UserRole.RoleId));
            }
            ViewData["UserRoles"] = UserRoles;
            ViewData["Roles"] = Roles;
            ViewData["RolesMaster"] = RolesMaster;
            return View(userprofile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUserToRole(FormCollection values)
        {
            int UserId = int.Parse(values["UserId"]);
            int RoleId = int.Parse(values["RoleId"]);
            db.Database.ExecuteSqlCommand("Insert into dbo.webpages_UsersInRoles (UserId, RoleId) Values({0},{1})", UserId, RoleId);
            return RedirectToAction("Details", "UserAdministration", new { id = UserId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveUserFromRole(FormCollection values)
        {
            int UserId = int.Parse(values["UserId"]);
            int RoleId = int.Parse(values["RoleId"]);
            db.Database.ExecuteSqlCommand("Delete from dbo.webpages_UsersInRoles where UserId={0} and RoleId={1}", UserId, RoleId);
            return RedirectToAction("Details", "UserAdministration", new { id = UserId });
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
