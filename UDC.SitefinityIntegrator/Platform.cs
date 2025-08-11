using System;
using System.Collections.Generic;

using UDC.Common.Interfaces;

namespace UDC.SitefinityIntegrator
{
    public class Platform : IPlatform
    {
        public Guid PlatformID { get; set; }
        public String Name { get; set; }
        
        public Platform()
        {
            this.PlatformID = new Guid("f4e5f407-9d8f-4711-a13b-d55b02c3b81f");
            this.Name = "Sitefinity 14.4";
        }
    }
}