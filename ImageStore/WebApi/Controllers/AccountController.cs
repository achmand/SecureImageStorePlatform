using System.Linq;
using System.Web.Http;
using Common;
using Logic.Domain;
using Logic.DomainObjects;
using WebApi.Attributes;

namespace WebApi.Controllers
{
    public sealed class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    [HmacAuth]
    public sealed class AccountController : ApiController
    {
        #region properties & variables 

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

        #endregion

        #region public methods

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

        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult Authenticate(LoginModel loginModel)
        {
            if (string.IsNullOrEmpty(loginModel.Username) || string.IsNullOrEmpty(loginModel.Password))
            {
                return BadRequest("Invalid inputs.");
            }

            var authenticate = UsersDomain.AuthenticateUser(loginModel.Username, loginModel.Password);
            if (authenticate.ProcessResult != ProcessResult.Success)
            {
                return BadRequest(authenticate.MessageResult);
            }

            var result =
                new
                {
                    message = authenticate.MessageResult,
                    username = authenticate.ObjectResult.Username,
                    role = authenticate.ObjectResult.RoleName
                };

            return Ok(result);
        }

        #endregion
    }
}
