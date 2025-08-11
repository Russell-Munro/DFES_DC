using System;
using System.Threading;
using System.Threading.Tasks;

namespace UDC.Common
{
    /// <summary>
    /// A helper class to run asynchronous code synchronously, avoiding deadlocks in ASP.NET applications.
    /// </summary>
    public static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory = new
            TaskFactory(CancellationToken.None,
                        TaskCreationOptions.None,
                        TaskContinuationOptions.None,
                        TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return _myTaskFactory
                .StartNew(func)
                .Unwrap() // Unwrap the inner Task<TResult>
                .GetAwaiter()
                .GetResult(); // This GetResult is now running on a ThreadPool thread
        }

        public static void RunSync(Func<Task> func)
        {
            _myTaskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }
    }
}