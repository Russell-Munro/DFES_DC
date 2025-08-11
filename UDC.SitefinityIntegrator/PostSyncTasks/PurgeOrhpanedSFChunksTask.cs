using System;

using UDC.Common.Data.Models;
using UDC.Common.Interfaces;
using UDC.Common.Data.Models.Configuration;

using UDC.SitefinityIntegrator.Data;

namespace UDC.SitefinityIntegrator.PostSyncTasks
{
    public class PurgeOrhpanedSFChunksTask : IPostSyncTask
    {
        public Guid PostSyncTaskID { get; set; }
        public String Name { get; set; }
        public IPostSyncTaskCfg Schema { get; set; }
        
        public PlatformCfg PlatformConfig { get; set; }
        public SyncContainer TargetContainer { get; set; }

        public PurgeOrhpanedSFChunksTask()
        {
            this.PostSyncTaskID = new Guid("568a4fe3-40d8-48aa-92b8-ebba4ef8ba20");
            this.Name = "Purge Orphaned sf_chunks Records";
            this.Schema = null;
        }

        public void SetConfig(PlatformCfg platformConfig, String cfg)
        {
            this.PlatformConfig = platformConfig;
        }
        public void RunTask()
        {
            RunPurge();
        }

        public String GetTaskInstanceDescription()
        {
            return "Purges Orphaned Records from sf_chunks table";
        }

        private Boolean RunPurge()
        {
            Boolean retVal = false;
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            APIResponse objAPIResponse = objPlatformIO.PurgeOrhpanedSFChunks();

            if (objAPIResponse != null)
            {
                if (objAPIResponse.exitCode == 0)
                {
                    retVal = true;
                }
                else
                {
                    throw new Exception("The SitefinityContextPlugin returned an error while serving this request. The error was: " + objAPIResponse.message);
                }
            }

            objAPIResponse = null;
            objPlatformIO = null;

            return retVal;
        }
    }
}