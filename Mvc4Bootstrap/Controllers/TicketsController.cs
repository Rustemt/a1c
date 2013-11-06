﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using A1fxCrm.Web.Models;
using PagedList;
namespace Mvc4Bootstrap.Controllers
{
    public class TicketsController : Controller
    {
        private A1fxCrmEntities db = new A1fxCrmEntities();

        //
        // GET: /Tickets/

        public ActionResult Index(int id)
        {

            var customerticket = db.CustomerTicket.Include(c => c.Customer).Where(r => r.CustomerId == id);

            if (customerticket.Count() < 1)
            {
                ViewBag.CustomerId = new SelectList(db.Customer.Where(r => r.Id == id), "Id", "FirstName", id);
                ViewBag.Customer = db.Customer.Where(r => r.Id == id).First();
                return View("CreateP");
            }
            
            return View(customerticket.ToList());
        }

        //
        // GET: /Tickets/Details/5

        public ActionResult Details(int id = 0)
        {
            CustomerTicket customerticket = db.CustomerTicket.Find(id);
            if (customerticket == null)
            {
                return HttpNotFound();
            }
            return View(customerticket);
        }

        //
        // GET: /Tickets/Create

        public ActionResult Create(int id)
        {
            ViewBag.CustomerId = new SelectList(db.Customer.Where(r => r.Id == id), "Id", "FirstName", id);
            ViewBag.Customer = db.Customer.Where(r => r.Id == id).First();
            return View();
        }

        //
        // POST: /Tickets/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CustomerTicket customerticket)
        {
            if (ModelState.IsValid)
            {
                db.CustomerTicket.Add(customerticket);
                db.SaveChanges();
                return RedirectToAction("Details", "Customer", new { id = customerticket.CustomerId });
            }

            ViewBag.CustomerId = new SelectList(db.Customer, "Id", "FirstName", customerticket.CustomerId);
            return RedirectToAction("Details", "Customer", new { id = customerticket.CustomerId });
        }

        //
        // GET: /Tickets/Edit/5

        public ActionResult Edit(int id = 0)
        {
            CustomerTicket customerticket = db.CustomerTicket.Find(id);
            if (customerticket == null)
            {
                return HttpNotFound();
            }
            ViewBag.CustomerId = new SelectList(db.Customer, "Id", "FirstName", customerticket.CustomerId);
            return View(customerticket);
        }

        //
        // POST: /Tickets/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CustomerTicket customerticket)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customerticket).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CustomerId = new SelectList(db.Customer, "Id", "FirstName", customerticket.CustomerId);
            return View(customerticket);
        }

        //
        // GET: /Tickets/Delete/5

        public ActionResult Delete(int id = 0)
        {
            CustomerTicket customerticket = db.CustomerTicket.Find(id);
            if (customerticket == null)
            {
                return HttpNotFound();
            }
            return View(customerticket);
        }

        //
        // POST: /Tickets/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CustomerTicket customerticket = db.CustomerTicket.Find(id);
            db.CustomerTicket.Remove(customerticket);
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