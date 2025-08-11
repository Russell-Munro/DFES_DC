using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace UDC.Common
{


    public class AppSettings
    {
        public static String GetValue(String key)
        {
            String retVal = "";
            String env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (string.IsNullOrWhiteSpace(env))
                env = "LOCAL";

            var pathToContentRoot = Directory.GetCurrentDirectory();

            //TODO: dont load  many times, cache this
            IConfigurationBuilder objCfgBuilder = new ConfigurationBuilder()
                .SetBasePath(pathToContentRoot)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
                
            IConfigurationRoot objCfg = objCfgBuilder.Build(); 

            retVal = objCfg.GetValue<String>(key);

            objCfg = null;
            objCfgBuilder = null;
            env = null;

            return retVal;
        }
    }
}