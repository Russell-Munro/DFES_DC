using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Newtonsoft.Json;

using UDC.Common;
using UDC.Common.Interfaces;
using UDC.Common.Data.Models.Configuration;
using UDC.Common.Data;
using UDC.Common.Database.Data.Models.Database;
using UDC.Common.Database.Data;

namespace UDC.DataConnectorCore
{
    public class ProviderHelpers
    {
        public enum Targets
        {
            Source = 1,
            Destination = 2
        }

        public static List<IIntegrator> GetSrcIntegrators(IPlatform target)
        {
            List<IIntegrator> arrRetVal = GetIntegratorInstances(target);
            
            if(arrRetVal != null && arrRetVal.Count > 0)
            {
                arrRetVal = arrRetVal.Where(obj => obj.SupportsRead).ToList();
            }

            return arrRetVal;
        }
        public static List<IIntegrator> GetDestIntegrators(IPlatform target)
        {
            List<IIntegrator> arrRetVal = GetIntegratorInstances(target);

            if (arrRetVal != null && arrRetVal.Count > 0)
            {
                arrRetVal = arrRetVal.Where(obj => obj.SupportsWrite).ToList();
            }

            return arrRetVal;
        }

        public static IPlatform GetPlatform(Guid platformID)
        {
            IPlatform retVal = null;
            List<IPlatform> arrRetVal = GetPlatformInstances();

            if (arrRetVal != null && arrRetVal.Count > 0)
            {
                retVal = arrRetVal.Where(obj => obj.PlatformID == platformID).FirstOrDefault();
            }

            arrRetVal = null;

            return retVal;
        }
        public static IIntegrator GetIntegrator(Guid integratorID)
        {
            IIntegrator retVal = null;
            List<IIntegrator> arrRetVal = GetIntegratorInstances();

            if (arrRetVal != null && arrRetVal.Count > 0)
            {
                retVal = arrRetVal.Where(obj => obj.IntegratorID == integratorID).FirstOrDefault();
            }

            arrRetVal = null;

            return retVal;
        }
        public static IIntegrator GetIntegrator(Guid integratorID, PlatformCfg cfg)
        {
            IIntegrator retVal = GetIntegrator(integratorID);
            
            if(retVal != null)
            {
                retVal.PlatformConfig = cfg;
            }

            return retVal;
        }
        public static IIntegrator GetIntegrator(Int64 connectionID, Targets target, DatabaseContext databaseContext)
        {
            IIntegrator objIntegrator = null;

            Connection objEntity = databaseContext.Connections.Where(obj => obj.Id == connectionID).FirstOrDefault();
                if (objEntity != null)
                {
                    PlatformCfg objPlatformCfg = null;

                    if (target == Targets.Source)
                    {
                        objPlatformCfg = JsonConvert.DeserializeObject<PlatformCfg>(objEntity.SourcePlatformCfg);
                    }
                    else
                    {
                        objPlatformCfg = JsonConvert.DeserializeObject<PlatformCfg>(objEntity.DestinationPlatformCfg);
                    }
                    if (objPlatformCfg != null)
                    {
                        Guid integratorGuid = GeneralHelpers.parseGUID(objPlatformCfg.IntegratorID);
                        if (integratorGuid != Guid.Empty)
                        {
                            objIntegrator = ProviderHelpers.GetIntegrator(integratorGuid, objPlatformCfg);
                        }
                    }
                    objPlatformCfg = null;
                }
                objEntity = null;

            return objIntegrator;
        }
        public static IPostSyncTask GetPostSyncTask(IPlatform target, Guid postSyncTaskID)
        {
            IPostSyncTask retVal = null;
            
            if(target != null)
            {
                List<IPostSyncTask> arrTasks = GetSupportedPostSyncTasks(target);
                retVal = arrTasks.Where(obj => obj.PostSyncTaskID == postSyncTaskID).FirstOrDefault();
                arrTasks = null;
            }

            return retVal;
        }

        public static List<IPlatform> GetPlatformInstances()
        {
            List<Type> arrAllProviderTypes = GetPlatformTypes();
            List<IPlatform> arrRetVal = null;

            if (arrAllProviderTypes != null && arrAllProviderTypes.Count > 0)
            {
                arrRetVal = new List<IPlatform>();
                foreach (Type objType in arrAllProviderTypes)
                {
                    arrRetVal.Add(Activator.CreateInstance(objType) as IPlatform);
                }
            }

            arrAllProviderTypes = null;

            return arrRetVal;
        }
        public static List<IIntegrator> GetIntegratorInstances()
        {
            List<Type> arrAllProviderTypes = GetIntegratorTypes();
            List<IIntegrator> arrRetVal = null;

            if (arrAllProviderTypes != null && arrAllProviderTypes.Count > 0)
            {
                arrRetVal = new List<IIntegrator>();
                foreach (Type objType in arrAllProviderTypes)
                {
                    arrRetVal.Add(Activator.CreateInstance(objType) as IIntegrator);
                }
            }

            arrAllProviderTypes = null;

            return arrRetVal;
        }
        public static List<IIntegrator> GetIntegratorInstances(IPlatform target)
        {
            List<Type> arrAllProviderTypes = GetIntegratorTypes(target.GetType().Assembly);
            List<IIntegrator> arrRetVal = null;

            if (arrAllProviderTypes != null && arrAllProviderTypes.Count > 0)
            {
                arrRetVal = new List<IIntegrator>();
                foreach (Type objType in arrAllProviderTypes)
                {
                    arrRetVal.Add(Activator.CreateInstance(objType) as IIntegrator);
                }
            }

            arrAllProviderTypes = null;

            return arrRetVal;
        }
        public static List<IPostSyncTask> GetSupportedPostSyncTasks(IPlatform target)
        {
            List<Type> arrAllProviderTypes = GetPlatformPostSyncTasks(target.GetType().Assembly);
            List<IPostSyncTask> arrRetVal = null;

            if (arrAllProviderTypes != null && arrAllProviderTypes.Count > 0)
            {
                arrRetVal = new List<IPostSyncTask>();
                foreach (Type objType in arrAllProviderTypes)
                {
                    arrRetVal.Add(Activator.CreateInstance(objType) as IPostSyncTask);
                }
            }

            arrAllProviderTypes = null;

            return arrRetVal;
        }

        public static List<Type> GetPlatformTypes()
        {
            List<Assembly> arrAssemblies = GetPluginAssemblies();
            List<Type> arrResolvedTypes = null;

            if (arrAssemblies != null && arrAssemblies.Count > 0)
            {
                arrResolvedTypes = arrAssemblies.SelectMany(x => x.GetTypes()).Where(mytype => typeof(IPlatform).IsAssignableFrom(mytype) && mytype.GetInterfaces().Contains(typeof(IPlatform))).ToList();
            }

            arrAssemblies = null;

            return arrResolvedTypes;
        }
        public static List<Type> GetPlatformPostSyncTasks(Assembly target)
        {
            List<Type> arrResolvedTypes = null;

            if (target != null)
            {
                arrResolvedTypes = target.GetTypes().Where(mytype => typeof(IPostSyncTask).IsAssignableFrom(mytype) && mytype.GetInterfaces().Contains(typeof(IPostSyncTask))).ToList();
            }

            return arrResolvedTypes;
        }
        public static List<Type> GetIntegratorTypes(Assembly target)
        {
            List<Type> arrResolvedTypes = null;

            if (target != null)
            {
                arrResolvedTypes = target.GetTypes().Where(mytype => typeof(IIntegrator).IsAssignableFrom(mytype) && mytype.GetInterfaces().Contains(typeof(IIntegrator))).ToList();
            }

            return arrResolvedTypes;
        }
        public static List<Type> GetIntegratorTypes()
        {
            List<Assembly> arrAssemblies = GetPluginAssemblies();
            List<Type> arrResolvedTypes = null;

            if (arrAssemblies != null && arrAssemblies.Count > 0)
            {
                arrResolvedTypes = arrAssemblies.SelectMany(x => x.GetTypes()).Where(mytype => typeof(IIntegrator).IsAssignableFrom(mytype) && mytype.GetInterfaces().Contains(typeof(IIntegrator))).ToList();
            }

            arrAssemblies = null;

            return arrResolvedTypes;
        }
        public static List<Assembly> GetPluginAssemblies()
        {
            List<Assembly> retVal = new List<Assembly>();
            String currPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            List<String> arrFiles = Directory.GetFiles(currPath, "UDC.*Integrator.dll").ToList();

            if(arrFiles != null && arrFiles.Count > 0)
            {
                foreach (String file in arrFiles)
                {
                    Assembly objAssembly = Assembly.LoadFile(file);
                    if (objAssembly != null)
                    {
                        retVal.Add(objAssembly);
                    }
                }
            }

            arrFiles = null;

            return retVal;
        }
    }
}