using System.Linq;
using Common;

namespace DataAccess.Repositories
{
    public sealed class UsersRepository : BaseRepository , IBasicRepository<User, string>
    {
        public string Add(User user)
        {
            if (user == null)
            {
                return null;
            }

            StoreDbEntities.Users.Add(user);
            var username = user.Username;
            return username;
        }

        public IQueryable<Role> GetRoles()
        {
            var roles = StoreDbEntities.Roles;
            return roles;
        }

    }
}
