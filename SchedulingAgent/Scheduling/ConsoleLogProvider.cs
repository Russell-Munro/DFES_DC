using System;

using Quartz.Logging;

namespace SchedulingAgent.Scheduling
{
    public class ConsoleLogProvider : ILogProvider
    {
        public Logger GetLogger(String name)
        {
            return (level, func, exception, parameters) =>
            {
                if (level >= LogLevel.Info && func != null)
                {
                    Console.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] [" + level + "] " + func(), parameters);
                }
                return true;
            };
        }
        public IDisposable OpenNestedContext(String message)
        {
            throw new NotImplementedException();
        }
        public IDisposable OpenMappedContext(String key, String value)
        {
            throw new NotImplementedException();
        }

        public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
        {
            throw new NotImplementedException();
        }
    }
}