using System;

using Newtonsoft.Json;

using UDC.Common.Interfaces;

namespace UDC.SitefinityIntegrator.PostSyncTasks
{
    public class RunSearchIndexCfg : IPostSyncTaskCfg
    {
        public String IndexName { get; set; }

        public RunSearchIndexCfg()
        {
            this.IndexName = "";
        }

        public String SchemaAsJSON()
        {
            return JsonConvert.SerializeObject(new RunSearchIndexCfg());
        }
        public String ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}