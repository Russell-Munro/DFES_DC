using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Quartz;
using SchedulingAgent.Scheduling;
using SchedulingAgent.WebSocketService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UDC.Common.Database.Data;
using UDC.Common.Database.Data.Models.Database;
using UDC.DataConnectorCore;
using static Azure.Core.HttpHeader;

namespace UDC.SchedulingAgent
{
    public class BackgroundWorker : IHostedService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
        private IScheduler _scheduler;
        private Task _backgroundTask;
        private CancellationTokenSource _cts;

        public BackgroundWorker(
            ISchedulerFactory schedulerFactory,
            IDbContextFactory<DatabaseContext> dbContextFactory
        )
        {
            _schedulerFactory = schedulerFactory;
            _dbContextFactory = dbContextFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Starting...");
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _backgroundTask = BackgroundTask(_cts.Token);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Service is stopping.");
            _cts.Cancel();
            if (_backgroundTask != null)
                await _backgroundTask;

            if (_scheduler != null)
                await _scheduler.Shutdown();
        }

        private async Task BackgroundTask(CancellationToken cancellationToken)
        {
            try
            {
                await StartServices(cancellationToken);

                // Service loop: if in interactive mode, offer CLI, else delay loop
                bool isInteractive = Environment.UserInteractive;
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        if (isInteractive)
                            await ExecCLI(cancellationToken);
                        else
                            await Task.Delay(2000, cancellationToken);
                    }
                    catch (TaskCanceledException) { }
                }
            }
            catch (Exception ex)
            {
                LogConsoleError(ex);
            }
            Console.WriteLine("Background task stopping.");
        }

        private async Task StartServices(CancellationToken cancellationToken)
        {
            await StartScheduler(cancellationToken);
            await LoadScheduledJobs(cancellationToken);
            await StartSocketServer();
        }

        private async Task StartScheduler(CancellationToken cancellationToken)
        {
            _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            await _scheduler.Start();
        }

        private async Task LoadScheduledJobs(CancellationToken cancellationToken)
        {
            try
            {
                await _scheduler.Clear();

                // Always add Cleanup Job
                IJobDetail cleanupJob = JobBuilder.Create<ExecuteCleanupJob>()
                    .WithIdentity("CleanupDB", "Maintenance")
                    .Build();

                ITrigger cleanupTrigger = TriggerBuilder.Create()
                    .WithIdentity("CleanupDB", "Maintenance")
                    .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever())
                    .StartAt(DateTime.UtcNow)
                    .Build();

                await _scheduler.ScheduleJob(cleanupJob, cleanupTrigger);

                // Load jobs from DB
                using var db = _dbContextFactory.CreateDbContext();
                List<ConnectionRule> rules = await db.ConnectionRules
                    .Include(r => r.Connection)
                    .Where(r => r.Enabled == true)
                    .ToListAsync(cancellationToken);

                foreach (var rule in rules)
                {
                    if (!string.IsNullOrEmpty(rule.SyncIntervalCron))
                    {
                        try
                        {
                            IJobDetail syncJob = JobBuilder.Create<ExecuteSyncJob>()
                                .WithIdentity($"RuleSync_{rule.Id}", "RuleSyncs")
                                .UsingJobData("ruleId", rule.Id)
                                .Build();

                            ITrigger syncTrigger = TriggerBuilder.Create()
                                .WithIdentity($"RuleSync_{rule.Id}", "RuleSyncs")
                                .WithCronSchedule(rule.SyncIntervalCron, x => x.WithMisfireHandlingInstructionDoNothing())
                                .StartAt(DateTime.UtcNow)
                                .Build();

                            await _scheduler.ScheduleJob(syncJob, syncTrigger);
                        }
                        catch (Exception ex2)
                        {
                            LogConsole($"Error creating scheduled job for rule {rule.Id}. Possibly invalid CRON string.", ConsoleColor.Red);
                            LogConsoleError(ex2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogConsole("Error loading scheduled jobs", ConsoleColor.Red);
                LogConsoleError(ex);
            }
        }

        private Task StartSocketServer()
        {
            try
            {
                // TODO: Migrate WebSocketServer to DI if possible, or initialize here
                string listenUrl = UDC.Common.AppSettings.GetValue("WebSocketHostUrl");
                string listenPort = UDC.Common.AppSettings.GetValue("WebSocketPort");

                WebSocketServer.SocketStateChanged += WebSocketServer_SocketStateChanged;
                WebSocketServer.CLIEvent += WebSocketServer_CLIEvent;
                WebSocketServer.Start($"{listenUrl}:{listenPort}/");
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                LogConsole("Error starting web socket server", ConsoleColor.Red);
                LogConsoleError(ex);
            }
            return Task.CompletedTask;
        }

        private async Task ExecCLI(CancellationToken cancellationToken)
        {
            string strCmd = "";
            bool runLoop = true;
            string intro = "**************************************************\n" +
                           "Universal Data Connector Command Line Interface\n" +
                           "**************************************************\n" +
                           "\nType 'help' for CLI reference...\n";
            LogConsole(intro, ConsoleColor.White);
            Console.WriteLine();
            Console.WriteLine();

            while (runLoop)
            {
                Console.Write("UDC:\\");
                strCmd = Console.ReadLine();
                switch (strCmd)
                {
                    case "help":
                    case "h":
                        string help = "quit(q) -     Terminate the Universal Data Connector.\n" +
                                      "help(h) -     This very prompt.\n" +
                                      "jobs(j) -     List Job Status.\n" +
                                      "reload(r) -   Reload Jobs from Database.\n" +
                                      "clients(c) -  Show Connected Clients.\n" +
                                      "environment(e) -  Show current environment.\n" +
                                      "testmode(t) -  Show if the connector is running in test mode.\n" +
                                      "testsystems(ts) -  Test connectivity to systems given a supplied rule.\n" +
                                      "runsync(rs) -  Run an actual sync given a supplied rule.\n";
                        LogConsole(help, ConsoleColor.Green);
                        break;
                    case "jobs":
                    case "j":
                        LogConsole(await GetRunningJobs(), ConsoleColor.Green);
                        break;
                    case "clients":
                    case "c":
                        LogConsole(WebSocketServer.GetConnectedClientList(), ConsoleColor.Green);
                        break;
                    case "reload":
                    case "r":
                        await LoadScheduledJobs(cancellationToken);
                        LogConsole("Done!", ConsoleColor.Green);
                        break;
                    case "quit":
                    case "q":
                        LogConsole("Bye Now!", ConsoleColor.White);
                        runLoop = false;
                        break;
                    case "environment":
                    case "e":
                        LogConsole("Environment: " + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), ConsoleColor.Green);
                        break;
                    case "testmode":
                    case "t":
                        LogConsole("TestMode: " + UDC.Common.AppSettings.GetValue("TestMode"), ConsoleColor.Green);
                        LogConsole("TestSyncObjectLimit: " + UDC.Common.AppSettings.GetValue("TestSyncObjectLimit"), ConsoleColor.Green);
                        break;
                    case "testsystems":
                    case "ts":
                        LogConsole("Enter RuleID: ", ConsoleColor.Green);
                        if (long.TryParse(Console.ReadLine(), out long intRuleID) && intRuleID > 0)
                        {
                            LogConsole("Rodger Rodger! Please wait...", ConsoleColor.Green);

                            SyncService objSyncService = new SyncService();
                            string result = objSyncService.TestSystemConnectivity(intRuleID);

                            LogConsole("TestResults: \n\n" + result, ConsoleColor.Green);
                        }
                        else
                        {
                            LogConsole("Invalid Rule!", ConsoleColor.Red);
                        }
                        break;
                    case "runsync":
                    case "rs":
                        LogConsole("Enter RuleID: ", ConsoleColor.Green);
                        if (long.TryParse(Console.ReadLine(), out long intRuleID2) && intRuleID2 > 0)
                        {
                            LogConsole("Rodger Rodger! Please wait...", ConsoleColor.Green);

                            SyncService objSyncService = new SyncService();

                            objSyncService.ExecuteRuleSync(intRuleID2);
                            LogConsole("Sync Complete...\n\n", ConsoleColor.Green);
                        }
                        else
                        {
                            LogConsole("Invalid Rule!", ConsoleColor.Red);
                        }
                        break;
                    case "":
                        break;
                    default:
                        LogConsole("Invalid Command!", ConsoleColor.Red);
                        break;
                }
            }

            await Task.FromCanceled(cancellationToken);
        }

        // Helper for displaying running jobs
        private async Task<string> GetRunningJobs()
        {
            string retVal = "Running Jobs:\n";
            try
            {
                var runningJobs = await _scheduler.GetCurrentlyExecutingJobs();
                foreach (var job in runningJobs)
                {
                    retVal += job.JobDetail.Key + "\n";
                }
            }
            catch (Exception ex)
            {
                LogConsole("Error running scheduled job", ConsoleColor.Red);
                LogConsoleError(ex);
            }
            return retVal;
        }

        // WebSocketServer events (adapt if needed)
        private static void WebSocketServer_SocketStateChanged(object sender, WebSocketServer.SocketStateChangedEventArgs e)
        {
            switch (e.LogType)
            {
                case UDC.Common.Constants.LogTypes.Trace:
                    LogConsole(e.Msg, ConsoleColor.DarkGray);
                    break;
                case UDC.Common.Constants.LogTypes.Warning:
                    LogConsole(e.Msg, ConsoleColor.DarkYellow);
                    break;
                case UDC.Common.Constants.LogTypes.Error:
                    LogConsole(e.Msg, ConsoleColor.DarkRed);
                    break;
            }
        }
        private static void WebSocketServer_CLIEvent(object sender, WebSocketServer.CLIEventArgs e)
        {
            // You may need to pass _scheduler into this logic if you want to trigger jobs
        }

        public static void LogConsole(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
        public static void LogConsoleError(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nException {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"  Inner Exception {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            Console.ResetColor();
        }
    }
}
