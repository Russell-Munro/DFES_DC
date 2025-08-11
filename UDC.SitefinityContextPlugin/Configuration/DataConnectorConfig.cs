using System;
using System.Configuration;

using Telerik.Sitefinity.Configuration;

namespace UDC.SitefinityContextPlugin.Configuration
{
    public class DataConnectorConfig : ConfigSection
    {
        [ConfigurationProperty("AdminUIUrl")]
        public SFConfigStringElement AdminUIUrl
        {
            get => (SFConfigStringElement)this["AdminUIUrl"];
            set => this["AdminUIUrl"] = value;
        }
        [ConfigurationProperty("SharedKey")]
        public SFConfigStringElement SharedKey
        {
            get => (SFConfigStringElement)this["SharedKey"];
            set => this["SharedKey"] = value;
        }
        [ConfigurationProperty("ConnectionStringKey")]
        public SFConfigStringElement ConnectionStringKey
        {
            get => (SFConfigStringElement)this["ConnectionStringKey"];
            set => this["ConnectionStringKey"] = value;
        }
    }
}