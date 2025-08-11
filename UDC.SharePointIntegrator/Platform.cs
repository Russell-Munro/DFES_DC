using System;
using System.Collections.Generic;

using UDC.Common.Interfaces;

namespace UDC.SharePointIntegrator
{
    public class Platform : IPlatform
    {
        public Guid PlatformID { get; set; }
        public String Name { get; set; }
        
        public Platform()
        {
            this.PlatformID = new Guid("63cca006-289b-4fec-a7ac-a284f5fbbed6");
            this.Name = "SharePoint";
        }
    }
}