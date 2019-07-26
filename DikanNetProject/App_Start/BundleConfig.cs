using System.Web;
using System.Web.Optimization;

namespace DikanNetProject
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/MyJS.min.js"));

            bundles.Add(new StyleBundle("~/Content/Css").Include(
                      "~/Content/Css/bootstrap.css",
                      "~/Content/Css/BaseCSS.css"));
        }
    }
}
