using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            MvcHandler.DisableMvcResponseHeader = true;

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var application = sender as HttpApplication;
            application?.Context?.Response.Headers.Remove("Server");
            application?.Context?.Response.Headers.Remove("X-AspNet-Version");
            application?.Context?.Response.Headers.Remove("X-AspNetMvc-Version");
        }
    }
}
