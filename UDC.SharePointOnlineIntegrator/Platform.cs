using System;
using UDC.Common.Interfaces;

namespace UDC.SharePointOnlineIntegrator
{
    public class Platform : IPlatform
    {
        public Guid PlatformID { get; set; }
        public string Name { get; set; }

        public Platform()
        {
            PlatformID = Guid.Parse("a34e24b2-0e39-4f38-9d8a-9a5b9aef1c9d");
            Name = "SharePoint Online";
        }
    }
}
