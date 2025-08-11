using System;

namespace UDC.Common.Data.Models.Configuration
{
    public class PlatformCfg
    {
        public String PlatformID { get; set; }
        public String IntegratorID { get; set; }
        public String EndPointURL { get; set; }
        public String ServiceUsername { get; set; }
        public String ServicePassword { get; set; }
        public String ServiceDomain { get; set; }

        public String PlatformId { get; set; }
    }
}