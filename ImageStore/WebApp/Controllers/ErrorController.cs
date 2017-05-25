using System.Web.Mvc;

namespace WebApp.Controllers
{
    public sealed class ErrorController : Controller
    {
        #region actions

        public ActionResult Index()
        {
            return View("Error");
        }

        public ActionResult NotFound()
        {
            Response.StatusCode = 200;
            return View("NotFound");
        }

        public ActionResult InternalServer()
        {
            Response.StatusCode = 200;
            return View("InternalServer");
        }

        #endregion
    }
}