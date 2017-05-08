using System.Linq;
using System.Web.Http;
using Common;
using Logic.Domain;
using WebApi.Attributes;

namespace WebApi.Controllers
{
    [RoutePrefix("api/Account")]
    public sealed class AccountController : ApiController
    {
        private UsersDomain _usersDomain;

        public AccountController()
        {
        }

        public AccountController(UsersDomain usersDomain)
        {
            UsersDomain = usersDomain;
        }

        public UsersDomain UsersDomain
        {
            get
            {
                return _usersDomain ?? new UsersDomain();
            }
            private set
            {
                _usersDomain = value;
            }
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidationActionFilter]
        public IHttpActionResult Create(User user)
        {
            if (!ModelState.IsValid)
            {
                var error = new
                {
                    message = "The request is invalid.",
                    errors = ModelState.Values.SelectMany(e => e.Errors.Select(er => er.ErrorMessage))
                };

                return BadRequest(error.ToString());
            }

            /*
            var result = UsersDomain.Add(user);
            if (result.ProcessResult == ProcessResult.Failure)
            {
                return BadRequest(result.MessageResult);
            }*/

            return Ok();
        }

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
