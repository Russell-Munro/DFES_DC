using System;

using Newtonsoft.Json;

using UDC.Common.Data.Models;
using UDC.Common.Interfaces;
using UDC.Common.Data.Models.Configuration;

using UDC.SitefinityIntegrator.Data;

namespace UDC.SitefinityIntegrator.PostSyncTasks
{
    public class RunSearchIndexTask : IPostSyncTask
    {
        public Guid PostSyncTaskID { get; set; }
        public String Name { get; set; }
        public IPostSyncTaskCfg Schema { get; set; }
        public RunSearchIndexCfg Cfg { get; set; }

        public PlatformCfg PlatformConfig { get; set; }
        public SyncContainer TargetContainer { get; set; }

        public RunSearchIndexTask()
        {
            this.PostSyncTaskID = new Guid("60d1e1fb-cc33-4624-af81-12bec8547e0c");
            this.Name = "Run Search Index";
            this.Schema = new RunSearchIndexCfg();
        }
        
        public void SetConfig(PlatformCfg platformConfig, String cfg)
        {
            this.PlatformConfig = platformConfig;
            this.Cfg = JsonConvert.DeserializeObject<RunSearchIndexCfg>(cfg);
        }
        public void RunTask()
        {
            if(this.Cfg != null && !String.IsNullOrEmpty(this.Cfg.IndexName))
            {
                RunSearchIndex(this.Cfg.IndexName);
            }
            else
            {
                throw new Exception("No index name was configured for this task. Check Post Sync Task Configuration.");
            }
        }

        public String GetTaskInstanceDescription()
        {
            return ((this.Cfg != null && !String.IsNullOrEmpty(this.Cfg.IndexName)) ? this.Cfg.IndexName : "No index configured");
        }

        private Boolean RunSearchIndex(String indexName)
        {
            Boolean retVal = false;
            PlatformIO objPlatformIO = new PlatformIO(this.PlatformConfig);
            APIResponse objAPIResponse = objPlatformIO.RunSearchIndex(indexName);

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