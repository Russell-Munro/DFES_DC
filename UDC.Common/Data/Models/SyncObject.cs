using System;
using System.Collections.Generic;

namespace UDC.Common.Data.Models
{
    public class SyncObject : AbstractModelBase
    {
        public String Id { get; set; }
        public String rootContainerId { get; set; }
        public String containerId { get; set; }
        public String Name { get; set; }
        public String Title { get; set; }
        public String FileName { get; set; }

        public Byte[] BinaryPayload { get; set; }
        public Int64 SizeBytes { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime LastUpdated { get; set; }

        public Dictionary<String, Object> Properties { get; set; }

        public SyncContainer Parent { get; set; }

        public SyncObject()
        {
            
        }
    }
}