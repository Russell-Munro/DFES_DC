using System;

using UDC.Common.Data.Models.Configuration;

namespace UDC.Common.Data.Models
{
    public class SyncFieldMapping : AbstractModelBase
    {
        public SyncField SrcField { get; set; }
        public SyncField DestField { get; set; }
        public SyncOptions Options { get; set; }
        
        public SyncFieldMapping()
        {
            this.Options = new SyncOptions();
        }
        public SyncFieldMapping(SyncField srcField, SyncField destField)
        {
            this.SrcField = srcField;
            this.DestField = destField;
            this.Options = new SyncOptions();
        }
        public SyncFieldMapping(SyncField srcField, SyncField destField, SyncOptions options)
        {
            this.SrcField = srcField;
            this.DestField = destField;
            this.Options = options;
        }
    }
}