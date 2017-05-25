using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Logic.Domain;
using Logic.Security;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Newtonsoft.Json.Linq;
using WebApp.Models;

namespace WebApp.Controllers
{
    public sealed class UsersController : BaseController
    {
        #region properties & variables 

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        #endregion

        #region actions

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
            try
            {
                var encodedResponse = Request.Form["g-Recaptcha-Response"];
                var result = ReCaptcha.Validate(encodedResponse);
                var isCaptchaValid = result.ToLower() == "true";
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
                var msg = message.Result.ToString();
                if (msg == "Success")
                {
                    return RedirectToAction("Index", "Home");
                }

                dynamic json = JObject.Parse(msg);
                var outerMessage = json.Message.ToString();
                return Json(outerMessage, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                LogsDomain.LogLevel("Users Domain", e.Message);
                return RedirectToAction("InternalServer", "Error");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LogIn(LoginViewModel loginViewModel)
        {
            try
            {
                var response = await HttpClient.PostAsJsonAsync(ApiUri + "/Account/Authenticate", loginViewModel);
                var message = response.Content.ReadAsAsync<object>();
                var jsonResult = message.Result.ToString();
                dynamic result = JObject.Parse(jsonResult);

                if (result.message != "Success")
                {
                    TempData["Message"] = result.Message.ToString();
                    return RedirectToAction("LogIn", "Account");
                }

                var username = Encryption.EncClaim(result.username.ToString());
                var role = Encryption.EncClaim(result.role.ToString());
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, username),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role)
                }, DefaultAuthenticationTypes.ApplicationCookie);

                AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);
                return RedirectToAction("Index", "Home");
            }

            catch (Exception e)
            {
                LogsDomain.LogLevel("Users Domain", e.Message);
                return RedirectToAction("InternalServer", "Error");
            }
        }

        #endregion
    }
}