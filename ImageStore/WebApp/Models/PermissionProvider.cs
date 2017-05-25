using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Common;
using Logic.Security;

namespace WebApp.Models
{
    public sealed class PermissionsProvider
    {
        #region properties and variables 

        private Dictionary<string, string[]> PermissionDict { get; set; }
        private static Dictionary<string, Menu[]> MenuDict { get; set; }

        public bool Initialized => PermissionDict != null;

        #endregion

        #region ctors 

        public PermissionsProvider()
        {
        }

        public PermissionsProvider(Dictionary<string, string[]> permissionDictionary)
        {
            PermissionDict = permissionDictionary;
        }

        #endregion

        #region public methods 

        public void SetPermissionDict(Dictionary<string, string[]> permissionDictionary)
        {
            PermissionDict = permissionDictionary;
        }

        public void SetMenuDict(Dictionary<string, Menu[]> menuDictionary)
        {
            MenuDict = menuDictionary;
        }

        public static Menu[] GetMenusRole()
        {
            var identity = (ClaimsIdentity)Thread.CurrentPrincipal.Identity;
            var decRole = identity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Role);
            if (decRole == null)
            {
                return null;
            }
            
            var role = Encryption.DecClaim(decRole.Value);
            if (MenuDict == null)
            {
                return null;
            }

            return MenuDict.ContainsKey(role) ? MenuDict[role] : null;
        }

        public bool HasPermission(string role, string permission)
        {
            if (!Initialized)
            {
                return false;
            }

            if (!PermissionDict.ContainsKey(permission))
            {
                return false;
            }

            var permissionList = PermissionDict[permission];
            return Array.IndexOf(permissionList, role) >= 0;
        }

        #endregion
    }
}