using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using UDC.Common.Data.Models.Configuration;

namespace UDC.Common.Database.Data.Models.Database
{
    [Table("equ_dc_Connection")]
    public class Connection
    {
        [Key]
        [Column("connectionID")]
        public Int64 Id { get; set; }
        public String Name { get; set; }

        public String SourcePlatformCfg { get; set; }
        public String DestinationPlatformCfg { get; set; }
        public String GlobalSrcDestObjSyncState { get; set; }

        public Boolean? Enabled { get; set; }

        public DateTime? DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }

        [ForeignKey("connectionID")]
        public List<ConnectionRule> ConnectionRules { get; set; }

        public object JsonSourcePlatformCfg => this.SourcePlatformCfg != null
            ? JsonConvert.DeserializeObject<PlatformCfg>(this.SourcePlatformCfg)
            : null;


        public object JsonDestinationPlatformCfg => this.DestinationPlatformCfg != null
            ? JsonConvert.DeserializeObject<PlatformCfg>(this.DestinationPlatformCfg)
            : null;

    }
}