using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UDC.Common.Database.Data.Models.Database;

namespace DataConnectorUI.Models
{
    public class ConnectionModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string SourcePlatformCfg { get; set; }
        public string DestinationPlatformCfg { get; set; }

        public Boolean Enabled { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime LastUpdated { get; set; }

        // public List<ConnectionRule> ConnectionRules { get; set; }

    }
}
