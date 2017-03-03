using BlockChainWebApplication.Core;
using BlockChainWebApplication.Core.Entities.Common;
using BlockChainWebApplication.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BlockChainWebApplication
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            AppManager.Prepare();
            Scheduler.Schedule();
        }

        protected void Session_Start(object sender, EventArgs e)
        {
          
        }
    }
}
