﻿using System.Web;
using System.Web.Optimization;

namespace DikanNetProject
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js", "~/Scripts/jquery.unobtrusive-ajax.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/javascript").Include(
                      "~/Scripts/MyJs/chosen.min.js",
                      "~/Scripts/MyJs/MyJS.js"));

            bundles.Add(new ScriptBundle("~/bundles/formSlider").Include(
                  "~/Scripts/MyJs/jquery-ui.min.js",
                  "~/Scripts/MyJs/formSlider.js",
                  "~/Scripts/MyJs/NewSocio.js"));

            bundles.Add(new ScriptBundle("~/bundles/signature_pad").Include(
                      "~/Scripts/MyJs/signature_pad.umd.js",
                      "~/Scripts/MyJs/signature_app.js"));

            bundles.Add(new ScriptBundle("~/bundles/datepicker").Include(
                      "~/Scripts/MyJs/datepicker.js",
                      "~/Scripts/MyJs/datepicker.he-HE.js"));

            bundles.Add(new StyleBundle("~/Content/Css").Include(
                      "~/Content/Css/bootstrap.css",
                      "~/Content/Css/Chosen/chosen.min.css",
                      "~/Content/Css/BaseCSS.css"));

            bundles.Add(new StyleBundle("~/Content/formSlider").Include(
                      "~/Content/Css/formSlider.css"));

            bundles.Add(new StyleBundle("~/Content/datepicker").Include(
                      "~/Content/Css/datepicker.css"));
        }
    }
}
