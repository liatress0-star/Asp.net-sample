using System;
using System.Web;
using System.Web.Routing;

namespace WebApp
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Session_Start(object sender, EventArgs e) { }
        protected void Application_BeginRequest(object sender, EventArgs e) { }
        protected void Application_AuthenticateRequest(object sender, EventArgs e) { }
        protected void Application_Error(object sender, EventArgs e) { }
        protected void Session_End(object sender, EventArgs e) { }
        protected void Application_End(object sender, EventArgs e) { }
    }

    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapPageRoute("Default", "", "~/GridSample.aspx");
        }
    }
}
