using System;
using System.Web.Routing;

using Microsoft.Web.Infrastructure.DynamicModuleHelper;

using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Security.Model;

using UDC.SitefinityContextPlugin.Configuration;
using UDC.SitefinityContextPlugin.Data;
using UDC.SitefinityContextPlugin.Handlers;

namespace UDC.SitefinityContextPlugin
{
    public static class Startup
    {
        public static void PreApplicationStart()
        {
            //Sitefinity Bootstrapper_Initialized event, which is fired after initialization of the Sitefinity application
            Bootstrapper.Initialized += (new EventHandler<ExecutedEventArgs>(Startup.Bootstrapper_Initialized));
        }

        public static void Initialize()
        {
            RegisterConfigs();
            DefineRoles();
            RegisterHttpHandlers();
        }
        public static void Bootstrapper_Initialized(object sender, Telerik.Sitefinity.Data.ExecutedEventArgs e)
        {
            RegisterConfigs();
            DefineRoles();

            //if (e.CommandName == "RegisterRoutes")
            //{
            RegisterHttpHandlers();
            //}
        }

        private static void RegisterConfigs()
        {
            Telerik.Sitefinity.Configuration.Config.RegisterSection<DataConnectorConfig>();
        }
        private static void RegisterHttpHandlers()
        {
            //Register our HttpHandlers...

            ////////////////////
            //*** Need to figure this out... For now, just implemented via Web.Config Handler Mappings... Would be good to make dynamic / configless...
            ////////////////////
            
            //DynamicModuleUtility.RegisterModule
            //RouteTable.Routes.Add(new Route("Sitefinity/DataConnectorUI.axd", new DataConnectorUIHandler()));
        }
        private static void DefineRoles()
        {
            Role objR1 = CMSDataIO.CreateRole(Constants.RoleConnectionAdmin);
            Role objR2 = CMSDataIO.CreateRole(Constants.RoleSyncManager);

            objR1 = null;
            objR2 = null;
        }
    }
}