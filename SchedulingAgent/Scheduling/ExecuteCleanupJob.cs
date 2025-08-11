using System;
using System.Threading.Tasks;

using Quartz;

using UDC.Common.Data;
using UDC.Common.Database.Data;

namespace SchedulingAgent.Scheduling
{
    [DisallowConcurrentExecution]
    public class ExecuteCleanupJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await Task.Run(() => ExecuteCleanup());
        }

        private void ExecuteCleanup()
        {
            try
            {
                //Clean Up our Log Table so it doesn't bloat up the DB over time with very verbose logging...
                Console.WriteLine("Executing Log Cleanup...");
                using (DatabaseContext objDB = new DatabaseContext())
                {
                    objDB.CleanupLogs();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing to Database " + ex.Message);
            }
        }
    }
}