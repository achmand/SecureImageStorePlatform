using System;
using System.Runtime.Caching;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Logic.Domain;
using WebApp.Models;

namespace WebApp
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            MvcHandler.DisableMvcResponseHeader = true;
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
            InitializePermissionProvider();

            /*
            var config = WebConfigurationManager.OpenWebConfiguration("~");
            var connSection = (ConnectionStringsSection)config.GetSection("connectionStrings");
            connSection.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
            config.Save();
            */
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var application = sender as HttpApplication;
            application?.Context?.Response.Headers.Remove("Server");
            application?.Context?.Response.Headers.Remove("X-AspNet-Version");
            application?.Context?.Response.Headers.Remove("X-AspNetMvc-Version");
        }

        private static void InitializePermissionProvider()
        {
            var permissionProvider = new PermissionsProvider();
            var usersDomain = new UsersDomain();

            var permissionList = usersDomain.GetPermissionList();
            if (permissionList != null)
            {
                permissionProvider.SetPermissionDict(permissionList);
            }

            var menuList = usersDomain.GetMenuList();
            if (menuList != null)
            {
                permissionProvider.SetMenuDict(menuList);
            }

            var cache = MemoryCache.Default;
            cache.AddOrGetExisting("PermissionManager", permissionProvider, new CacheItemPolicy());
        }
    }
}
