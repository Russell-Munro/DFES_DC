using System;

using static UDC.Common.Constants;

namespace UDC.Common.Data.Models.Configuration
{
    public class SyncOptions
    {
        public NullActions NullAction { get; set; }
        public Boolean MutuallyExclusive { get; set; }
        public Boolean AlwaysUpdateFromSrc { get; set; }
    }
}