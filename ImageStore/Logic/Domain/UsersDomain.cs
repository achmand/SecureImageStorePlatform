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

        public void Dispose()
        {
            UsersRepo.Dispose();
        }
    }
}
