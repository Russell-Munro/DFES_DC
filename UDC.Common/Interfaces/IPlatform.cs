using System;
using System.Collections.Generic;

namespace UDC.Common.Interfaces
{
    public interface IPlatform
    {
        Guid PlatformID { get; set; }
        String Name { get; set; }
    }
}