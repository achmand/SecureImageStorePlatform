using System.Linq;
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
            var encodedResponse = Request.Form["g-Recaptcha-Response"];
            var isCaptchaValid = ReCaptcha.Validate(encodedResponse) == "True";

            if (!isCaptchaValid)
            {
                return Json("Captcha is missing. Please try again.", JsonRequestBehavior.AllowGet);
            }

            if (!ModelState.IsValid)
            {
                var errorList = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());

                return Json(errorList, JsonRequestBehavior.AllowGet);
            }


            var response = await HttpClient.PostAsJsonAsync(ApiUri + "/Account/Create", register);
            var message = response.Content.ReadAsAsync<object>();
            return Json(message, JsonRequestBehavior.AllowGet);
        }

        #region Helpers


        #endregion
    }
}