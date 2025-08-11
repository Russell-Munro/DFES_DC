using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Quartz;
using Newtonsoft.Json;

using UDC.Common;
using UDC.Common.Database.Logging;
using UDC.DataConnectorCore;
using UDC.DataConnectorCore.Models;

using static UDC.Common.Constants;

using SchedulingAgent.Models;
using SchedulingAgent.WebSocketService;

namespace SchedulingAgent.Scheduling
{
    [DisallowConcurrentExecution]
    public class ExecuteSyncJob : IJob
    {
        private Int64 RuleId = 0;

        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap objJobDataMap = context.JobDetail.JobDataMap;

            RuleId = objJobDataMap.GetLong("ruleId");
            await Task.Run(() => ExecuteSync(RuleId));

            objJobDataMap = null;
        }

        private void ExecuteSync(Int64 ruleId)
        {
            SyncService objSyncSvc = new SyncService(LogVerbosityLevels.All);
            SyncStatus objSyncStatus = null;

            Console.WriteLine("Executing Sync for Rule " + ruleId);

            objSyncSvc.SyncStateUpdated += ObjSyncSvc_SyncStateUpdated;
            objSyncStatus = objSyncSvc.ExecuteRuleSync(ruleId);

            SyncCompleted(objSyncStatus);
        }
        private void SyncCompleted(SyncStatus syncStatus)
        {
            SocketResponse objResponse = new SocketResponse(SocketFrameType.SyncStats, 0, "OK", null);
            Dictionary<String, Object> objStatus = null;

            Console.WriteLine("Completed Sync for Rule " + RuleId);

            try
            {
                Logger.Write(RuleId, "ExecuteSyncJob.ExecuteSync()", "SyncCompleted", JsonConvert.SerializeObject(syncStatus));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing to Database " + ex.Message);
            }

            syncStatus.SyncLog = null;
            objStatus = GeneralHelpers.objectToDictionary(syncStatus);
            objStatus.Add("connectionRuleID", RuleId);

            objResponse.data = objStatus;
            WebSocketServer.Broadcast(JsonConvert.SerializeObject(objResponse));

            objStatus = null;
            objResponse = null;
        }

        private void ObjSyncSvc_SyncStateUpdated(object sender, SyncService.SyncStateUpdatedEventArgs e)
        {
            SocketResponse objResponse = new SocketResponse(SocketFrameType.SyncStateUpdate, 0, "OK", null);
            Dictionary<String, Object> objState = GeneralHelpers.objectToDictionary(e.SyncState);

            objState.Add("connectionRuleID", RuleId);
            objResponse.data = objState;
            
            WebSocketServer.Broadcast(JsonConvert.SerializeObject(objResponse));

            objState = null;
            objResponse = null;
        }
    }
}