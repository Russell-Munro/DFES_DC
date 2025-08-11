using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UDC.Common.Data.Models;
using UDC.Common.Data.Models.Configuration;

namespace UDC.Common.Database.Data.Models.Database
{
  

    [Table("equ_dc_ConnectionRule")]
    public class ConnectionRule
    {
        [Key]
        [Column("connectionRuleID")]
        public long Id { get; set; }

        public long connectionID { get; set; }
        public string Name { get; set; }
        public string SyncIntervalCron { get; set; }
        public DateTime? LastExecuted { get; set; }
        public string LastExecutedStatus { get; set; }
        public string SourceContainerCfg { get; set; }
        public string DestinationContainerCfg { get; set; }
        public string FieldMappings { get; set; }
        public string SrcDestObjSyncState { get; set; }
        public string SourcePostSyncTasks { get; set; }
        public string DestinationPostSyncTasks { get; set; }
        public bool? Enabled { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }

        // No [ForeignKey] attribute needed on navigation property
        public List<DataConnectorLog> DataConnectorLogs { get; set; }

        public Connection Connection { get; set; }

        public ConnectionRule()
        {
            DataConnectorLogs = new List<DataConnectorLog>();
        }

        // --- ADD THIS SECTION ---

        /// <summary>
        /// Deserializes the FieldMappings JSON string into a strongly-typed list.
        /// The [NotMapped] attribute tells Entity Framework to ignore this property.
        /// </summary>
        [NotMapped]
        public List<SyncFieldMapping> JsonFieldMappings =>
            JsonConvert.DeserializeObject<List<SyncFieldMapping>>(FieldMappings ?? "[]");


        /// <summary>
        /// Deserializes the SourceContainerCfg JSON string.
        /// </summary>
        [NotMapped]
        public IntegratorCfg JsonSourceContainerCfg =>
            !string.IsNullOrEmpty(SourceContainerCfg)
                ? JsonConvert.DeserializeObject<IntegratorCfg>(SourceContainerCfg)
                : null;

 
    /// <summary>
    /// Deserializes the DestinationContainerCfg JSON string.
    /// </summary>
    [NotMapped]
        public IntegratorCfg JsonDestinationContainerCfg =>
        !string.IsNullOrEmpty(DestinationContainerCfg)
            ? JsonConvert.DeserializeObject<IntegratorCfg>(DestinationContainerCfg)
            : null;
    }

    public class FieldConfig
    {
        public string Id { get; set; }
        public string Key { get; set; }
    }
}
