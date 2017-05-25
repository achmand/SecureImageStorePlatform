using System.Linq;
using System.Runtime.Caching;
using System.Security.Claims;
using System.Threading;
using System.Web.Mvc;
using Logic.Security;
using WebApp.Models;

namespace WebApp.Attributes
{
    public class ClaimsAuthorizeAttribute : AuthorizeAttribute
    {
        #region properties and variables

        private readonly string[] _claimValues;

        #endregion

        #region public methods

        public ClaimsAuthorizeAttribute(params string[] values)
        {
            _claimValues = values;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var cache = MemoryCache.Default;
            var permissionManager = (PermissionsProvider) cache.AddOrGetExisting("PermissionManager", new PermissionsProvider(), new CacheItemPolicy());
            var identity = (ClaimsIdentity)Thread.CurrentPrincipal.Identity;
            var decRole = identity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Role);

            if (decRole != null)
            {
                var permission = _claimValues[1];
                var role = Encryption.DecClaim(decRole.Value);
                var isValid = permissionManager.HasPermission(role, permission);

                if (isValid)
                {
                    base.OnAuthorization(filterContext);
                }
                else
                {
                    HandleUnauthorizedRequest(filterContext);
                }
            }
            else
            {
                HandleUnauthorizedRequest(filterContext);
            }
        }

        #endregion
    }
}