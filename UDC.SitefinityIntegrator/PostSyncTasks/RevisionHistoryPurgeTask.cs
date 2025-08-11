using System;

using Newtonsoft.Json;

using UDC.Common;
using UDC.Common.Data.Models;
using UDC.Common.Data.Models.Configuration;
using UDC.Common.Interfaces;

using UDC.SitefinityIntegrator.Data;

namespace UDC.SitefinityIntegrator.PostSyncTasks
{
    public class RevisionHistoryPurgeTask : IPostSyncTask
    {
        public Guid PostSyncTaskID { get; set; }
        public String Name { get; set; }
        public IPostSyncTaskCfg Schema { get; set; }

        public PlatformCfg PlatformConfig { get; set; }
        public SyncContainer TargetContainer { get; set; }

        public RevisionHistoryPurgeTask()
        {
            this.PostSyncTaskID = new Guid("fd9d97a4-beb1-4593-a4a3-be5596edc98d");
            this.Name = "Document Revision History Purge";
            this.Schema = null;
        }

        public void SetConfig(PlatformCfg platformConfig, String cfg)
        {
            this.PlatformConfig = platformConfig;
        }
        public void RunTask()
        {
            if (this.TargetContainer != null && !String.IsNullOrEmpty(this.TargetContainer.Id))
            {
                PurgeRevisionHistories(this.TargetContainer.Id);
            }
            else
            {
                throw new Exception("No container was configured for this task. Check Post Sync Task Configuration.");
            }
        }

        public String GetTaskInstanceDescription()
        {
            return ((this.TargetContainer != null && String.IsNullOrEmpty(this.TargetContainer.Name)) ? this.TargetContainer.Name : "No container configured");
        }

        private Boolean PurgeRevisionHistories(String containerID)
        {
            Boolean retVal = false;
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            APIResponse objAPIResponse = null;
            Guid libGuid = GeneralHelpers.parseGUID(this.TargetContainer.Id);

            if(libGuid != Guid.Empty)
            {
                objAPIResponse = objPlatformIO.PurgeDocumentRevisionHistories(libGuid);
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
            }

            objAPIResponse = null;
            objPlatformIO = null;

            return retVal;
        }
    }
}