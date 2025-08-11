using System;
using System.Collections.Generic;

namespace UDC.Common.Data.Models
{
    public class SyncTag : AbstractModelBase
    {
        public String Id { get; set; }
        public String parentId { get; set; }
        public String Name { get; set; }

        public SyncTag Parent { get; set; }
        public List<SyncTag> SyncTags { get; set; }
    }
}