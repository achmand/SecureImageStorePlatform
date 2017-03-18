using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public sealed class UsersController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult> Register()
        {
            var registerUser = new RegisterUserModel();
            var responseMsg = await HttpClient.GetAsync(ApiUri + "/Account/GetRoles");
            if (!responseMsg.IsSuccessStatusCode)
            {
                RedirectToAction("Index", "Home"); // redirect to something went wrong pag
            }

            var responseData = responseMsg.Content.ReadAsStringAsync().Result;
            var roles = JsonConvert.DeserializeObject<List<RolesJson>>(responseData);
            if (roles != null)
            {
                registerUser.RoleList = roles.Select(r => new SelectListItem { Value = r.RoleName, Text = r.RoleName });
            }

            return View(registerUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterUserModel registerUserModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registerUserModel);
            }

            var newUser = new
            {
                registerUserModel.Username,
                registerUserModel.Email,
                registerUserModel.Password,
                registerUserModel.RoleName
            };

            var responseMsg = await HttpClient.PostAsJsonAsync(ApiUri + "/Account/Create", newUser);
            return View();
        }
    }
}