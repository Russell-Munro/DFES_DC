using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using UDC.Common.Database.Data; // Change to your actual DbContext namespace
using UDC.SchedulingAgent;      // Where your BackgroundWorker lives

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        // Optionally configure extra configuration sources here.
    })
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        // Register your EF Core context (edit for your DB type and connection string name)
        services.AddDbContext<DatabaseContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register Quartz.NET with DI
        services.AddQuartz();
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        // Register your WebSocketServer as singleton if required:
        // services.AddSingleton<WebSocketServer>();

        // Register your background service
       services.AddHostedService<BackgroundWorker>();
    });

// Enable Windows Service support if running as a service
builder.UseWindowsService();

var host = builder.Build();
await host.RunAsync();
