using System;
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
    public class CustomerController : Controller
    {
        private A1fxCrmEntities db = new A1fxCrmEntities();

        //
        // GET: /Customer/

        public ActionResult Index(string search, int page = 1, int uid = 0, int cid = 0)
        {
            int pageSize = 3;
            int pageNumber = page;
            var customer = db.Customer.Include(c => c.CustomerStatus).Include(c => c.User);
            if (uid > 0)
                customer = customer.Where(r => r.UserId == uid);

             if (cid > 0)
                customer = customer.Where(r => r.CustomerStatusId == cid);

            customer = customer.OrderByDescending(r => r.CreatedDate);
            var customerPaged = customer.ToPagedList(pageNumber, pageSize);


            return View(customerPaged);
        }

        //
        // GET: /Customer/Details/5

        public ActionResult Details(int id, int? page)
        {
            Customer customer = db.Customer.Find(id);


            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        //
        // GET: /Customer/Create

        public ActionResult Create()
        {
            ViewBag.CustomerStatusId = new SelectList(db.CustomerStatus, "Id", "Name");
            ViewBag.UserId = new SelectList(db.User, "Id", "ConfirmationToken");
            return View();
        }

        //
        // POST: /Customer/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                 var _user =db.User.Where(r=>r.UserName == HttpContext.User.Identity.Name).FirstOrDefault();
                 customer.UpdateUserId = _user.Id;
                 customer.UpdatedDate = DateTime.Now;
                 customer.CreateUserId = _user.Id;
                 customer.CreatedDate = DateTime.Now;
                 customer.UserId = _user.Id;
                db.Customer.Add(customer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CustomerStatusId = new SelectList(db.CustomerStatus, "Id", "Name", customer.CustomerStatusId);
            ViewBag.UserId = new SelectList(db.User, "Id", "ConfirmationToken", customer.UserId);
            return View(customer);
        }

        //
        // GET: /Customer/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Customer customer = db.Customer.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            ViewBag.CustomerStatusId = new SelectList(db.CustomerStatus, "Id", "Name", customer.CustomerStatusId);
            ViewBag.UserId = new SelectList(db.User, "Id", "Email", customer.UserId);
            return View(customer);
        }

        //
        // POST: /Customer/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                var _customer = db.Customer.Where(r => r.Id == customer.Id).FirstOrDefault();
                
                var _user =db.User.Where(r=>r.UserName == HttpContext.User.Identity.Name).FirstOrDefault();
                _customer.FirstName = customer.FirstName;
                _customer.LastName = customer.LastName;
                _customer.Email = customer.Email;
                _customer.Mobile = customer.Mobile;
                _customer.Phone = customer.Phone;
                _customer.Referance = customer.Referance;
                _customer.Source = customer.Source;
                _customer.UserId = customer.UserId;
  
                _customer.UpdatedDate = DateTime.Now;
                _customer.UpdateUserId = _user.Id;
                db.Entry(_customer).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CustomerStatusId = new SelectList(db.CustomerStatus, "Id", "Name", customer.CustomerStatusId);
            ViewBag.UserId = new SelectList(db.User, "Id", "ConfirmationToken", customer.UserId);
            return View(customer);
        }

        //
        // GET: /Customer/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Customer customer = db.Customer.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        //
        // POST: /Customer/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Customer customer = db.Customer.Find(id);
            db.Customer.Remove(customer);
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