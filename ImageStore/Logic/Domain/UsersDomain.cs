using System;
using System.Linq;
using Common;
using Common.Utilities;
using DataAccess.Repositories;
using Logic.DomainObjects;

namespace Logic.Domain
{
    public sealed class UsersDomain : IBasicDomain<string, User>, IDisposable
    {
        private const int LoginTries = 5;
        private const string DefaultMessage = "Incorrect login information.";
        private const string LockedMessage = "Account Locked. Please contact admin.";

        private UsersRepository UsersRepo { get; }

        public UsersDomain()
        {
            UsersRepo = new UsersRepository();
        }

        public DomainResult<string> Add(User user)
        {
            var domainResult = new DomainResult<string>();
            if (user == null)
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

            var defaultRole = UsersRepo.GetRole(r => r.RoleName == "Default");
            user.Role = defaultRole;

            var hashedPass = HashString(user.Password);
            user.Password = hashedPass;
            user.Version = 1;
            user.LoginTries = 0;
            user.Actived = true;
            user.Locked = false;
            user.DateCreated = HomeLessMethods.GetCurrentTime();

            var username = UsersRepo.Add(user);

            domainResult.MessageResult = "Registration completed.";
            domainResult.ProcessResult = ProcessResult.Success;
            domainResult.ObjectResult = username;

            return domainResult;
        }

        public IQueryable<Role> GetRoles()
        {
            var roles = UsersRepo.GetRoles();
            return roles;
        }

        public DomainResult<string> AuthenticateUser(string user, string password)
        {
            var domainResult = new DomainResult<string>();
            user = user.ToLower();

            var checkUser = UsersRepo.GetByIdentifier(u => u.Username.ToLower() == user);
            if (checkUser == null)
            {
                domainResult.ProcessResult = ProcessResult.Failure;
                domainResult.MessageResult = DefaultMessage;
                return domainResult;
            }

            if (checkUser.Locked)
            {
                domainResult.ProcessResult = ProcessResult.Failure;
                domainResult.MessageResult = LockedMessage; // TODO -> Send link to unlock 
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
            }

            domainResult.ProcessResult = ProcessResult.Failure;
            return domainResult;
        }

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
