using System;
using static UDC.Common.Constants;

namespace UDC.Common.Data.Models
{
    public class SyncField
    {
        public String Id { get; set; }
        public String Key { get; set; }
        public String Title { get; set; }
        public String NativeType { get; set; }
        public FieldDataTypes FieldDataType { get; set; }
        public String LinkedLookupId { get; set; }
        public Boolean Writable { get; set; }

        public SyncField()
        {
            
        }
        public SyncField(String id, String key, String title, String nativeType, FieldDataTypes fieldDataType, String linkedLookupId, Boolean writable)
        {
            this.Id = id;
            this.Key = key;
            this.Title = title;
            this.NativeType = nativeType;
            this.FieldDataType = fieldDataType;
            this.LinkedLookupId = linkedLookupId;
            this.Writable = writable;
        }
    }
}