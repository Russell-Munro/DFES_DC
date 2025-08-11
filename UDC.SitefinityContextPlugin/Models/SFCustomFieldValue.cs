using System;

namespace UDC.SitefinityContextPlugin.Models
{
    public class SFCustomFieldValue
    {
        public Object FieldValue { get; set; }
        public FieldTypes FieldType { get; set; }
        public Boolean IsMutuallyExclusive { get; set; }

        public enum FieldTypes
        {
            Default = 0,
            Taxonomy = 1,
        }

        public SFCustomFieldValue()
        {

        }
        public SFCustomFieldValue(Object fieldValue, FieldTypes fieldType)
        {
            this.FieldValue = fieldValue;
            this.FieldType = fieldType;
        }
        public SFCustomFieldValue(Object fieldValue, FieldTypes fieldType, Boolean isMutuallyExclusive)
        {
            this.FieldValue = fieldValue;
            this.FieldType = fieldType;
            this.IsMutuallyExclusive = isMutuallyExclusive;
        }
        public SFCustomFieldValue(Object fieldValue)
        {
            this.FieldValue = fieldValue;
            this.FieldType = FieldTypes.Default;
        }
    }
}