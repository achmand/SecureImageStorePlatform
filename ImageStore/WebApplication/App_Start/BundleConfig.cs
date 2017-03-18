using System.Web.Optimization;

namespace WebApplication
{
    public sealed class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/content/style/")
                .Include("~/Content/bootstrap.css")
                .Include("~/Content/Site.css"));

            BundleTable.EnableOptimizations = true;
        }
    }
}