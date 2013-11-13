using System.Web.Optimization;

namespace A1fxCrm.Web
{
    public static class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            RegisterStyleBundles(bundles);
            RegisterJavascriptBundles(bundles);
        }

        private static void RegisterStyleBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/css")
                            .Include("~/Content/bootstrap.css")
                            .Include("~/Content/dataTables.bootstrap.css")
                        
                            .Include("~/Content/flaty.css")
                            .Include("~/Content/flaty-responsive.css"));
                            
        }

        private static void RegisterJavascriptBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/js")
                            .Include("~/Scripts/jquery-{version}.js")
                            .Include("~/Scripts/jquery-ui-{version}.js")
                            .Include("~/Scripts/dataTables.bootstrap.js")
                            .Include("~/Scripts/jquery.dataTables.js")
                             
                            .Include("~/Scripts/falty.js")
                            .Include("~/Scripts/bootstrap.js"));
            
        }
    }
}