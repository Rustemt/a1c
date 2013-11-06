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
    public class StatusController : Controller
    {
        private A1fxCrmEntities db = new A1fxCrmEntities();

        //
        // GET: /Status/

        public ActionResult Index()
        {
            return View(db.CustomerStatus.ToList());
        }

        //
        // GET: /Status/Details/5

        public ActionResult Details(int id = 0)
        {
            CustomerStatus customerstatus = db.CustomerStatus.Find(id);
            if (customerstatus == null)
            {
                return HttpNotFound();
            }
            return View(customerstatus);
        }

        //
        // GET: /Status/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Status/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CustomerStatus customerstatus)
        {
            if (ModelState.IsValid)
            {
                db.CustomerStatus.Add(customerstatus);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(customerstatus);
        }

        //
        // GET: /Status/Edit/5

        public ActionResult Edit(int id = 0)
        {
            CustomerStatus customerstatus = db.CustomerStatus.Find(id);
            if (customerstatus == null)
            {
                return HttpNotFound();
            }
            return View(customerstatus);
        }

        //
        // POST: /Status/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CustomerStatus customerstatus)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customerstatus).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(customerstatus);
        }

        //
        // GET: /Status/Delete/5

        public ActionResult Delete(int id = 0)
        {
            CustomerStatus customerstatus = db.CustomerStatus.Find(id);
            if (customerstatus == null)
            {
                return HttpNotFound();
            }
            return View(customerstatus);
        }

        //
        // POST: /Status/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CustomerStatus customerstatus = db.CustomerStatus.Find(id);
            db.CustomerStatus.Remove(customerstatus);
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