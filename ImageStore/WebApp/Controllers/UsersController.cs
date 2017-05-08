using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    public sealed class UsersController : BaseController
    {
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel register)
        {
            var responseMessage = await HttpClient.PostAsJsonAsync(ApiUri + "/Account/Create", register);
            return RedirectToAction("Index", "Home");
        }

        #region Helpers


        #endregion
    }
}