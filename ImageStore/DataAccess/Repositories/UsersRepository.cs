using System;
using System.Linq;
using Common;

namespace DataAccess.Repositories
{
    public sealed class UsersRepository : BaseRepository, IBasicRepository<User, string>
    {
        public string Add(User user)
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

        public string Update(User user)
        {
            StoreDbEntities.Users.Attach(user);

            var entry = StoreDbEntities.Entry(user);
            entry.Property(u => u.Actived).IsModified = true;
            entry.Property(u => u.LoginTries).IsModified = true;
            entry.Property(u => u.Locked).IsModified = true;

            SaveDatabase();

            var username = user.Username;
            return username;
        }

        public User GetByIdentifier(Func<User, bool> where)
        {
            return StoreDbEntities.Users.SingleOrDefault(where);
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
