using System.Web;
using System.Web.Optimization;

namespace TimeSheetMvc4WebApplication
{
    public class BundleConfig
    {
        // Дополнительные сведения о Bundling см. по адресу http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.UseCdn = true;

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*",
                        "~/Scripts/bootstrap.js"));

            // Используйте версию Modernizr для разработчиков, чтобы учиться работать. Когда вы будете готовы перейти к работе,
            // используйте средство построения на сайте http://modernizr.com, чтобы выбрать только нужные тесты.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css"
                        ));

            BundleTable.Bundles.Add(new ScriptBundle("~/Content/datejs").Include("~/Scripts/datejs/date.js", "~/Scripts/datejs/ru-RU.js"));
            BundleTable.Bundles.Add(new ScriptBundle("~/Content/angularjs").Include("~/Scripts/angular.js"));
            BundleTable.Bundles.Add(new StyleBundle("~/Content/timeSheet").Include("~/Content/themes/TimeSheetCSS.css", "~/Content/themes/TimeSheetApprovalCSS.css"));

            BundleTable.Bundles.Add(new ScriptBundle("~/Scripts/bootstrap-datepicker").Include("~/Scripts/bootstrap-datepicker.js",
                                                                                                   "~/Scripts/bootstrap-datepicker.ru.js"));
            BundleTable.Bundles.Add(new StyleBundle("~/Content/bootstrap-datepicker").Include("~/Content/bootstrap-datepicker.css"));
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap.js"));
            BundleTable.Bundles.Add(new StyleBundle("~/Content/bootstrap").Include("~/Content/bootstrap.css"));
            BundleTable.Bundles.Add(new StyleBundle("~/Content/bootstrap-theme").Include("~/Content/bootstrap-theme.css"));


            BundleTable.EnableOptimizations = true;
        }
    }
}