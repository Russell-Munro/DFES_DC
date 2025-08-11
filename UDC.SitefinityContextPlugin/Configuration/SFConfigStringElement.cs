using System;
using System.Configuration;

using Telerik.Sitefinity.Configuration;

namespace UDC.SitefinityContextPlugin.Configuration
{
    public class SFConfigStringElement : ConfigElement
    {
        public SFConfigStringElement(ConfigElement parent)
            : base(parent)
        {
        }

        [ConfigurationProperty("Value", IsRequired = true)]
        public String Value
        {
            get
            {
                return (String)this["Value"];
            }
            set
            {
                this["Value"] = value;
            }
        }
    }
}