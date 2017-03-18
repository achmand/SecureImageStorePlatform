using System.Web.Http;
using Common;
using Logic.Domain;
using Logic.DomainObjects;
using WebApi.Attributes;

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

        [HttpGet]
        [HmacAuthentication]
        public IHttpActionResult GetRoles()
        {
            var roles = UsersDomain.GetRoles();
            return Ok(roles);
        }

        [HttpPost]
        public IHttpActionResult Create(User user)
        {
            var result = UsersDomain.Add(user);
            if (result.ProcessResult == ProcessResult.Failure)
            {
                return BadRequest(result.MessageResult);
            }

            return Ok(result);
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
