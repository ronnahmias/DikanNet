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
                      "~/Scripts/MyJs/chosen.min.js",
                      "~/Scripts/MyJs/signature_pad.umd.js",
                      "~/Scripts/MyJs/signature_app.js",
                      "~/Scripts/MyJs/MyJS.js"));

            bundles.Add(new StyleBundle("~/Content/Css").Include(
                      "~/Content/Css/bootstrap.css",
                      "~/Content/Css/Chosen/chosen.min.css",
                      "~/Content/Css/BaseCSS.css"));
        }
    }
}
