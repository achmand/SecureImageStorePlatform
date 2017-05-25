using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Utilities;
using DataAccess.Repositories;
using Logic.DomainObjects;
using Logic.Security;

namespace Logic.Domain
{
    public sealed class UsersDomain : IBasicDomain<string, User>, IDisposable
    {
        #region properties & variables 

        private const int LoginTries = 5;
        private const string DefaultMessage = "Incorrect login information.";
        private const string LockedMessage = "Account Locked. Please contact admin.";

        private UsersRepository UsersRepo { get; }

        #endregion

        #region ctors

        public UsersDomain()
        {
            UsersRepo = new UsersRepository();
        }

        #endregion

        #region users methods

        public User GetUser(string username)
        {
            var user = UsersRepo.GetByIdentifier(u => u.Username == username);
            return user;
        }

        public DomainResult<string> Add(User user)
        {
            var domainResult = new DomainResult<string>();
            if (string.IsNullOrEmpty(user?.Username) || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
            {
                domainResult.MessageResult = "User cannot be null.";
                domainResult.ProcessResult = ProcessResult.Failure;
                return domainResult;
            }

            var exist = UsersRepo.Exists(u => u.Username.ToLower() == user.ToString().ToLower() || u.Email.ToLower() == user.Email.ToLower());
            if (exist)
            {
                domainResult.MessageResult = "User already exists.";
                domainResult.ProcessResult = ProcessResult.Failure;
                return domainResult;
            }

            user.RoleName = "Default";
            var hashedPass = HashString(user.Password);
            user.Password = hashedPass;

            user.Version = 1;
            user.LoginTries = 0;
            user.Actived = true;
            user.IsExternal = false;
            user.Locked = false;
            user.DateCreated = HomelessMethods.GetCurrentTime();

            var encryption = new Encryption();
            var keys = encryption.GeneratePublicAndPrivateKey();
            user.PrivateKey = keys.PrivateKey;
            user.PublicKey = keys.PublicKey;

            var username = UsersRepo.Add(user);
            domainResult.MessageResult = "Success";
            domainResult.ProcessResult = ProcessResult.Success;
            domainResult.ObjectResult = username;

            return domainResult;
        }

        public DomainResult<User> AddExternalUser(User user)
        {
            var domainResult = new DomainResult<User>();

            user.Version = 1;
            user.LoginTries = 0;
            user.Actived = true;
            user.IsExternal = true;
            user.Locked = false;
            user.DateCreated = HomelessMethods.GetCurrentTime();
            user.Password = "EXTERNALLOGIN";
            var encryption = new Encryption();
            user.RoleName = "Default";
            var keys = encryption.GeneratePublicAndPrivateKey();
            user.PrivateKey = keys.PrivateKey;
            user.PublicKey = keys.PublicKey;

            var username = UsersRepo.Add(user);
            domainResult.ProcessResult = ProcessResult.Success;
            domainResult.MessageResult = username + " created";
            domainResult.ObjectResult = user;
            return domainResult;
        }

        public DomainResult<User> AuthenticateUser(string user, string password)
        {
            var domainResult = new DomainResult<User>();
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
            {
                domainResult.ProcessResult = ProcessResult.Failure;
                domainResult.MessageResult = "Fail";
                return domainResult;
            }

            user = user.ToLower();
            var checkUser = UsersRepo.GetByIdentifier(u => u.Username.ToLower() == user && u.IsExternal == false);
            if (checkUser == null)
            {
                domainResult.ProcessResult = ProcessResult.Failure;
                domainResult.MessageResult = DefaultMessage;
                return domainResult;
            }

            if (checkUser.Locked)
            {
                domainResult.ProcessResult = ProcessResult.Failure;
                domainResult.MessageResult = LockedMessage;
                return domainResult;
            }

            if (!checkUser.Actived)
            {
                domainResult.ProcessResult = ProcessResult.Failure;
                domainResult.MessageResult = "Account not activated.";
            }

            var hashedPass = checkUser.Password;
            var valid = BCrypt.Net.BCrypt.Verify(password, hashedPass);

            if (valid)
            {
                if (checkUser.LoginTries > 0)
                {
                    checkUser.LoginTries = 0;
                    UpdateLoginTry(checkUser);
                }

                domainResult.ProcessResult = ProcessResult.Success;
                domainResult.ObjectResult = checkUser;
                domainResult.MessageResult = "Success";
                return domainResult;
            }

            var loginTries = checkUser.LoginTries + 1;
            if (loginTries >= LoginTries)
            {
                checkUser.LoginTries = 0;
                checkUser.Locked = true;
                UpdateLoginTry(checkUser);
                domainResult.MessageResult = LockedMessage;
            }
            else
            {
                checkUser.LoginTries = loginTries;
                domainResult.MessageResult = DefaultMessage;
                UpdateLoginTry(checkUser);
            }

            domainResult.ProcessResult = ProcessResult.Failure;
            return domainResult;
        }

        public bool UsernameExists(string username)
        {
            var usernameExist = UsersRepo.Exists(u => u.Username == username);
            return usernameExist;
        }

        #endregion

        #region roles methods

        public IQueryable<Role> GetRoles()
        {
            var roles = UsersRepo.GetRoles();
            return roles;
        }

        public Dictionary<string, string[]> GetPermissionList()
        {
            var permissions = UsersRepo.GetPermissions();
            if (permissions == null)
            {
                return null;
            }

            var permDict = new Dictionary<string, string[]>();
            foreach (var permission in permissions)
            {
                var roleList = permission.Roles.Select(r => r.RoleName).ToArray();
                if (roleList.Length > 0)
                {
                    permDict.Add(permission.PermissionName, roleList);
                }
            }

            return permDict;
        }

        public Dictionary<string, Menu[]> GetMenuList()
        {
            var roles = UsersRepo.GetRoles();
            if (roles == null)
            {
                return null;
            }

            var permMenus = new Dictionary<string, Menu[]>();
            foreach (var role in roles)
            {
                var menuList = role.Menus.Select(m => m).ToArray();
                if (menuList.Length > 0)
                {
                    permMenus.Add(role.RoleName, menuList);
                }
            }

            return permMenus;
        }

        #endregion

        public void Dispose()
        {
            UsersRepo.Dispose();
        }

        #region private methods

        private static string HashString(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("");
            }

            var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
            var hash = BCrypt.Net.BCrypt.HashPassword(text, salt);
            return hash;
        }

        private void UpdateLoginTry(User user)
        {
            UsersRepo.Update(user);
        }

        #endregion
    }
}
