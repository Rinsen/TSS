﻿using System.Web;
using System.Web.Optimization;

namespace TietoCRM
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.UseCdn = true;
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js",
                      "~/Scripts/jquery-ui-1.11.4-min.js",
                      "~/Scripts/jquery-ui-timepicker-addon.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.min.css",
                      "~/Content/site.css",
                      "~/Content/sidebar.css",
                      "~/Content/bootstrap-select.min.css",
                      "~/Content/bootstrap-table.css",
                      "~/Content/jquery-ui-timepicker-addon.css"));
            // Less integration
            bundles.Add(new LessBundle("~/Content/less").Include(
                "~/Content/TSS-BaseColors.less",
                "~/Content/Site.less",
                "~/Content/Sidebar.less"));
            
        }
    }
}
