using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebApplication1.Models;

namespace WebApplication1
{
    public class MvcApplication : System.Web.HttpApplication
    {

        public static string[] CollectionPts = {
            "Stationery Store - Administration Building ( 9:30 AM )",
            "Management School ( 11:00 AM)",
            "Medical School ( 9:30 AM )",
            "Engineering School ( 11:00 AM )",
            "Science School ( 9:30 AM )",
            "University Hospital ( 11:00 AM )"
        };


        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
