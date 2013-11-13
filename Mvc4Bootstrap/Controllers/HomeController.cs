using A1fxCrm.Web.Framework.Security.Attributes;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace Mvc4Bootstrap.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        [Authorization(Group = "", Name = "User")]
        public ActionResult Index()
        {
          
            return View();
        }

        public ActionResult FeaturedExample()
        {
            return View();
        }

        public ActionResult DialogExample()
        {
            return View();
        }
    }
}
