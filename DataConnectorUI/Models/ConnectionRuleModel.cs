using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataConnectorUI.Models
{
    public class ConnectionRuleModel
    {
        public Int64 connectionID { get; set; }
        public String Name { get; set; }

        public String SyncIntervalCron { get; set; }
        public DateTime? LastExecuted { get; set; }
        public String LastExecutedStatus { get; set; }
        public String SourceContainerCfg { get; set; }
        public String DestinationContainerCfg { get; set; }
        public String FieldMappings { get; set; }

/*
        public Object JsonFieldMappings { get; set; }
*/
        public Object JsonDestinationContainerCfg { get; set; }


        public String SrcDestObjSyncState { get; set; }

        public Boolean? Enabled { get; set; }

        public DateTime? DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }

        //public object JsonFieldMappings {get;set;}


    }
}
