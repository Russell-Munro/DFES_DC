using System;
using System.Collections.Generic;

namespace UDC.Common.Data.Models
{
    public class SyncContainer : AbstractModelBase
    {
        public String Id { get; set; }
        public String parentId { get; set; }
        public String Name { get; set; }

        public SyncContainer Parent { get; set; }
        public List<SyncContainer> SyncContainers { get; set; }

        public List<SyncObject> SyncObjects { get; set; }
    }
}