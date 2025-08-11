using System;

namespace UDC.Common.Interfaces
{
    public interface IPostSyncTaskCfg
    {
        String SchemaAsJSON();
        String ToJSON();
    }
}