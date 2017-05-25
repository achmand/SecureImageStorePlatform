using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Common;
using Logic.Domain;
using Logic.Security;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace WebApp.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        #region properties & variables 

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        private UsersDomain _usersDomain;

        public UsersDomain UsersDomain
        {
            get { return _usersDomain ?? new UsersDomain(); }
            private set { _usersDomain = value; }
        }

        #endregion

        #region actions 

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ChallengeResult(provider,
                Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            try
            {
                var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (loginInfo == null)
                {
                    return View("Login");
                }

                var username = loginInfo.DefaultUserName;
                var user = UsersDomain.GetUser(username);
                if (user == null)
                {
                    var newUser = new User
                    {
                        Username = username,
                        Email = loginInfo.Email
                    };

                    var result = UsersDomain.AddExternalUser(newUser);
                    var userCreated = result.ObjectResult;

                    var usernameClaim = Encryption.EncClaim(userCreated.Username);
                    var roleClaim = Encryption.EncClaim(userCreated.RoleName);
                    var identity = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, usernameClaim),
                        new Claim(ClaimTypes.Name, usernameClaim),
                        new Claim(ClaimTypes.Role, roleClaim)
                    }, DefaultAuthenticationTypes.ApplicationCookie);

                    AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    var usernameClaim = Encryption.EncClaim(user.Username);
                    var roleClaim = Encryption.EncClaim(user.RoleName);

                    var identity = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, usernameClaim),
                        new Claim(ClaimTypes.Name, usernameClaim),
                        new Claim(ClaimTypes.Role, roleClaim)
                    }, DefaultAuthenticationTypes.ApplicationCookie);

                    AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);
                    return RedirectToAction("Index", "Home");
                }

            }
            catch (Exception e)
            {
                LogsDomain.LogLevel("Users Domain", e.Message);
                return RedirectToAction("InternalServer", "Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region helpers 

        private const string XsrfKey = "XsrfId";

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_usersDomain != null)
                {
                    _usersDomain.Dispose();
                    _usersDomain = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}