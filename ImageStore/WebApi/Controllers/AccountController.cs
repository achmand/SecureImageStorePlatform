using System.Linq;
using System.Web.Http;
using Common;
using Logic.Domain;
using Logic.DomainObjects;

namespace WebApi.Controllers
{
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
        public IHttpActionResult Create(User user)
        {
            ModelState.Clear();
            Validate(user);

            if (!ModelState.IsValid)
            {
                var error = new
                {
                    message = "The request is invalid.",
                    errors = ModelState.Values.SelectMany(e => e.Errors.Select(er => er.ErrorMessage))
                };

                return BadRequest(error.ToString());
            }

            var response = UsersDomain.Add(user);
            if (response.ProcessResult != ProcessResult.Failure) return Ok(response.MessageResult);
            {
                var error = new
                {
                    message = "The request is invalid.",
                    errors = response.MessageResult
                };

                return BadRequest(error.ToString());
            }
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
