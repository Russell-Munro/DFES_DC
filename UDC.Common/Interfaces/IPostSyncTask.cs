using System;

using UDC.Common.Data.Models;
using UDC.Common.Data.Models.Configuration;

namespace UDC.Common.Interfaces
{
    public interface IPostSyncTask
    {
        Guid PostSyncTaskID { get; set; }
        String Name { get; set; }
        IPostSyncTaskCfg Schema { get; set; }

        PlatformCfg PlatformConfig { get; set; }
        SyncContainer TargetContainer { get; set; }

        void SetConfig(PlatformCfg platformConfig, String cfg);
        void RunTask();

        String GetTaskInstanceDescription();
    }
}