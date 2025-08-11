using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace UDC.Common
{
    public class AppSettings
    {
        private static readonly Lazy<IConfigurationRoot> _configuration = new Lazy<IConfigurationRoot>(() =>
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(env))
                env = "LOCAL";

            var pathToContentRoot = Directory.GetCurrentDirectory();

            return new ConfigurationBuilder()
                .SetBasePath(pathToContentRoot)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        });

        public static string GetValue(string key)
        {
            return _configuration.Value.GetValue<string>(key);
        }
    }
}

