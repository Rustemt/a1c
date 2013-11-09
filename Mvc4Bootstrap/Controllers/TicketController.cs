using A1fxCrm.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace A1fxCrm.Web.Controllers
{
    public class TicketController : ApiController
    {
        private A1fxCrmEntities db = new A1fxCrmEntities();

        [AllowAnonymous]
        public object PostTicket(TicketModel loginModel)
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
            return new { model = customerticket };
            // }
        }
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
