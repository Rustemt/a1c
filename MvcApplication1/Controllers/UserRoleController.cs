using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplication1.Models;

namespace MvcApplication1.Controllers
{
    public class UserRoleController : Controller
    {
        private A1fxCrmEntities db = new A1fxCrmEntities();

        //
        // GET: /UserRole/

        public ActionResult Index()
        {
            return View(db.PermissionItem.ToList());
        }

        //
        // GET: /UserRole/Details/5

        public ActionResult Details(int id = 0)
        {
            PermissionItem permissionitem = db.PermissionItem.Find(id);
            if (permissionitem == null)
            {
                return HttpNotFound();
            }
            return View(permissionitem);
        }

        //
        // GET: /UserRole/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /UserRole/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PermissionItem permissionitem)
        {
            if (ModelState.IsValid)
            {
                db.PermissionItem.Add(permissionitem);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(permissionitem);
        }

        //
        // GET: /UserRole/Edit/5

        public ActionResult Edit(int id = 0)
        {
            PermissionItem permissionitem = db.PermissionItem.Find(id);
            if (permissionitem == null)
            {
                return HttpNotFound();
            }
            return View(permissionitem);
        }

        //
        // POST: /UserRole/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PermissionItem permissionitem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(permissionitem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(permissionitem);
        }

        //
        // GET: /UserRole/Delete/5

        public ActionResult Delete(int id = 0)
        {
            PermissionItem permissionitem = db.PermissionItem.Find(id);
            if (permissionitem == null)
            {
                return HttpNotFound();
            }
            return View(permissionitem);
        }

        //
        // POST: /UserRole/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PermissionItem permissionitem = db.PermissionItem.Find(id);
            db.PermissionItem.Remove(permissionitem);
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