using System;
using System.Data.Entity.Validation;
using System.Linq;
using Common;

namespace DataAccess.Repositories
{
    public sealed class UsersRepository : BaseRepository, IBasicRepository<User, string>
    {
        public string Add(User user)
        {
            try
            {
                if (user == null)
                {
                    return null;
                }

                StoreDbEntities.Users.Add(user);
                SaveDatabase();
                var username = user.Username;
                return username;
            }

            catch (DbEntityValidationException e)
            {
                throw new Exception(e.Message);
            }
      
        }

        public bool Exists(Func<User, bool> where)
        {
            return StoreDbEntities.Users.Any(where);
        }

        public Role GetRole(Func<Role, bool> where)
        {
            return StoreDbEntities.Roles.SingleOrDefault(where);
        }

        public IQueryable<Role> GetRoles()
        {
            var roles = StoreDbEntities.Roles;
            return roles;
        }
    }
}
