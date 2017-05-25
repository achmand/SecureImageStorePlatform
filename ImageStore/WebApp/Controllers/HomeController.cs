using System.Web.Mvc;

namespace WebApp.Controllers
{
    public sealed class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}