using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using A1fxCrm.Web.Models;
using A1fxCrm.Web.Framework.Security.Attributes;

namespace Mvc4Bootstrap.Controllers
{
    [Authorization(Group = "", Name = "Admin")]
    public class UserController : Controller
    {
        private A1fxCrmEntities db = new A1fxCrmEntities();

        //
        // GET: /User/

        public ActionResult Index()
        {
            return View(db.User.ToList());
        }

        public ActionResult ActiveUserList()
        {
            return View(db.User.ToList());
        }
        //
        // GET: /User/Details/5

        public ActionResult Details(int id = 0)
        {
            User user = db.User.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //
        // GET: /User/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /User/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(User user)
        {
            if (ModelState.IsValid)
            {
                db.User.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        //
        // GET: /User/Edit/5

        public ActionResult Edit(int id = 0)
        {
            User user = db.User.Include(c => c.Role).Where(r => r.Id == id).FirstOrDefault();
            if (user.Role != null && user.Role.Count > 0)
                ViewBag.Roles = new SelectList(db.Role, "Id", "DisplayName", user.Role.FirstOrDefault().Id);
            else
                ViewBag.Roles = new SelectList(db.Role, "Id", "DisplayName", "Seçiniz");

            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //
        // POST: /User/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(User user)
        {

            if (ModelState.IsValid)
            {
                int roleId = Convert.ToInt32(Request.Form["roleId"]);
                var role = db.Role.Where(r => r.Id == roleId).FirstOrDefault();
                var userIds = db.User.Include(c => c.Role).Where(r => r.Id == user.Id).FirstOrDefault();
                userIds.FullName = user.FullName;
                userIds.UserName = user.UserName;
                userIds.IsAdministrator = user.IsAdministrator;
                userIds.Status = user.Status;
                userIds.Email = user.Email;
                if (role != null)
                {
                    userIds.Role.Clear();
                    userIds.Role.Add(role);
                }
                db.Entry(userIds).State = System.Data.Entity.EntityState.Modified;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        //
        // GET: /User/Delete/5

        public ActionResult Delete(int id = 0)
        {
            User user = db.User.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //
        // POST: /User/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.User.Find(id);
            db.User.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}