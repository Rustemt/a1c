using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using A1fxCrm.Web.Models;
using PagedList;
using A1fxCrm.Web.Framework.Security.Attributes;
namespace Mvc4Bootstrap.Controllers
{
    //[Authorization(Group = "", Name = "Cust")]
    //[Authorization(Group = "", Name = "CustAdmin")]
    public class TicketsController : Controller
    {
        private A1fxCrmEntities db = new A1fxCrmEntities();

        [AllowAnonymous]
        public object PostSave(TicketModel loginModel)
        {
         
            CustomerTicket customerticket = new CustomerTicket
            {
                CustomerId = Convert.ToInt32(loginModel.id),
                Content = loginModel.content,
                CreatedDate = DateTime.Now,
                Title = "Title"
            };
            db.CustomerTicket.Add(customerticket);
            db.SaveChanges();
            //if (LoginManager.Login(loginModel.username, loginModel.password, out errorMessage))
            //{
            //    return new { Result = true, User = LoginManager.CurrentUser.Transform() };
            //}
            //else
            //{
            return customerticket;
            // }
        }
 
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

        public ActionResult TicketList( int page = 1)
        {
            int pageSize = 15;
            int pageNumber = page;
            var customerticket = db.CustomerTicket.Include(c => c.Customer).OrderByDescending(r => r.Id);
 
            var customerPaged = customerticket.ToPagedList(pageNumber, pageSize);

            foreach (var item in customerPaged)
            {
                var name = item.Customer.User.FirstName;
            }

            return View(customerPaged);
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
                customerticket.CreatedDate = DateTime.Now;
                db.CustomerTicket.Add(customerticket);
                db.SaveChanges();
                return RedirectToAction("Edits", "Customer", new { id = customerticket.CustomerId });
            }

            ViewBag.CustomerId = new SelectList(db.Customer, "Id", "FirstName", customerticket.CustomerId);
            return RedirectToAction("Edits", "Customer", new { id = customerticket.CustomerId });
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
                customerticket.CreatedDate = DateTime.Now;
                db.Entry(customerticket).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Edits", "Customer", new { id = customerticket.CustomerId });
            }
            ViewBag.CustomerId = new SelectList(db.Customer, "Id", "FirstName", customerticket.CustomerId);
            return RedirectToAction("Edits", "Customer", new { id = customerticket.CustomerId });
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