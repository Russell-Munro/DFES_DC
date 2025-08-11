using AutoMapper;
using DataConnectorUI.Controllers.Filters;
using DataConnectorUI.GraphQL.Mutations;
using DataConnectorUI.GraphQL.Queries;
using DataConnectorUI.GraphQL.Schemas;
using DataConnectorUI.GraphQL.Types;
using DataConnectorUI.Models;
using DataConnectorUI.Repositories;
using DataConnectorUI.Services;
using GraphQL;
using GraphQL.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using UDC.Common;
using UDC.Common.Database.Data;
using UDC.Common.Database.Data.Models.Database;

// --- 1. Use WebApplication.CreateBuilder for a minimal hosting setup ---
// This replaces the old Program.cs and Startup.cs structure.
var builder = WebApplication.CreateBuilder(args);

// --- 2. Configure Hosting: Windows Service and Kestrel ---

builder.Logging.ClearProviders();
builder.Logging.AddConsole(); // Console sink is default in dev
builder.Logging.AddDebug();

// This single line correctly configures the app to run as a Windows Service.
// It replaces all the old manual "isService" checks from your Program.cs.
builder.Host.UseWindowsService();
// Configure the Kestrel web server (logic from your old Program.cs).
builder.WebHost.UseKestrel(options =>
{
    var httpPort = 80;
    var httpsPort = 443;

#if DEBUG
    httpPort = 63232;
    httpsPort = 63231;
#endif

    options.Listen(IPAddress.Any, httpPort);
    options.Listen(IPAddress.Any, httpsPort, listenOptions =>
    {
        // Use the standard builder.Configuration to get app settings.
        var sslCertSubject = builder.Configuration["SSLCertSubject"]
            ?? throw new InvalidOperationException("SSLCertSubject is not configured in appsettings.json.");
        listenOptions.UseHttps(GetHttpsCertificateFromStore(sslCertSubject));
    });
});

// --- 3. Configure Services (from Startup.ConfigureServices) ---

var services = builder.Services;


// Standard way to make HttpContext available in services.
services.AddHttpContextAccessor();

services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

// Add MVC services with optional Newtonsoft.Json support.
// The old `EnableEndpointRouting = false` is removed as it's obsolete.
services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
    });
services.AddRazorPages();

// Register your custom application services.
services.AddScoped<AdminAuthFilter>();
services.AddScoped<ConnectionQuery>();
//services.AddScoped<ConnectionMutation>();
services.AddScoped<AuthSessionService>();
services.AddScoped<ConnectionRepository>();
services.AddScoped<FieldMappingType>();
services.AddScoped<IntegratorCfgType>();

// Register Entity Framework Core DbContext.
services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(DatabaseContext.GetConnectionString()));

// Add and configure GraphQL using the modern builder pattern.
services.AddGraphQL(options => options
.AddSchema<ConnectionSchema>(GraphQL.DI.ServiceLifetime.Scoped)
.AddSystemTextJson()
.AddGraphTypes(typeof(Program).Assembly)

);

// Use the modern AddAutoMapper extension method.
// This requires the `AutoMapper.Extensions.Microsoft.DependencyInjection` package (or AutoMapper v13+).
services.AddAutoMapper(cfg =>
{
    cfg.CreateMap<Connection, ConnectionModel>();
    cfg.CreateMap<ConnectionRule, ConnectionRuleModel>();
});

// --- 4. Build the Application ---
var app = builder.Build();

// --- 5. Configure the HTTP Request Pipeline (from Startup.Configure) ---

//app.MapGet("/", (ILogger<Program> logger) =>
//{
//    logger.LogInformation("This is an info log.");
//    logger.LogWarning("This is a warning.");
//    return "Hello from .NET 8 logging!";
//});

// Map the GraphQL endpoint.
app.UseGraphQL("/graphql");

// Use developer-friendly error pages and GraphiQL in the Development environment.
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/graphql"))
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            Console.WriteLine("GraphQL Request Body:");
            Console.WriteLine(body);
            context.Request.Body.Position = 0;
        }
        await next();
    });

    app.UseDeveloperExceptionPage();
    // The GraphiQL endpoint is enabled by default with .UseGraphQL() in dev.
    // It will be available at /ui/graphiql
    app.UseGraphQLGraphiQL();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();

// UseRouting must come before endpoint mapping.
app.UseRouting();

// Authentication/Authorization middleware (if you use it).
app.UseAuthentication();
app.UseAuthorization();



// Map controller routes. This replaces the old `app.UseMvc(...)`.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// --- 6. Run Database Initialization ---
// This pattern is fine for development/startup tasks.
// For production, consider using EF Core Migrations applied at deployment time.
using (var scope = app.Services.CreateScope())
{
    var servicesProvider = scope.ServiceProvider;
    try
    {
        var dbContext = servicesProvider.GetRequiredService<DatabaseContext>();
        // Your custom method to create the schema. For migrations, use dbContext.Database.Migrate();
        dbContext.CreateDatabaseSchema();
    }
    catch (Exception ex)
    {
        var logger = servicesProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database schema." + ex.Message);
        // You might want to prevent the app from starting if the DB is essential.
        return; // Exit if DB connection fails.
    }
}

// --- 7. Run the Application ---
// A single app.Run() handles both console and Windows Service execution.
app.Run();

// --- Helper Method (from your old Program.cs) ---
static X509Certificate2 GetHttpsCertificateFromStore(string sslCertSubject)
{
    using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
    store.Open(OpenFlags.ReadOnly);
    var certCollection = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, $"CN={sslCertSubject}", false);

    if (certCollection.Count == 0)
    {
        throw new Exception($"HTTPS certificate with subject 'CN={sslCertSubject}' not found in the LocalMachine/My store.");
    }

    return certCollection[0];
}
