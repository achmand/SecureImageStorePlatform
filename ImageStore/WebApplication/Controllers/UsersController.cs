using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
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
            var result = await PopulateRoleList(registerUser);
            if (!result)
            {
                return RedirectToAction("Index", "Home");
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

        private async Task<bool> PopulateRoleList(RegisterUserModel registerUser)
        {
            var responseMsg = await HttpClient.GetAsync(ApiUri + "/Account/GetRoles");
            if (!responseMsg.IsSuccessStatusCode)
            {
                return false;
            }

            var responseData = responseMsg.Content.ReadAsStringAsync().Result;
            var roles = JsonConvert.DeserializeObject<List<RolesJson>>(responseData);
            if (roles == null)
            {
                return false;
            }

            var rolesResult = roles.Select(r => new SelectListItem { Value = r.RoleName, Text = r.RoleName });
            return true;
        }
    }
}