using System;
using System.Collections.Generic;
using System.Threading;

namespace Ensconce
{
    /// <summary>
    /// Retry logic applied from this Stack Overflow thread
    /// http://stackoverflow.com/questions/1563191/c-sharp-cleanest-way-to-write-retry-logic
    /// </summary>
    public static class Retry
    {
        public static void Do(Action action, TimeSpan retryInterval, int retryCount = 3)
        {
            Do<object>(() =>
            {
                action();
                return null;
            }, retryInterval, retryCount);
        }
        public static T Do<T>(Func<T> action, TimeSpan retryInterval, int retryCount = 3)
        {
            var exceptions = new List<Exception>();

            for (int retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    return action();
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine("Something went wrong, but we're going to try again. ;(");
                    Console.Out.WriteLine(ex.Message);
                    exceptions.Add(ex);
                    Thread.Sleep(retryInterval);
                }
            }

            throw new AggregateException(exceptions);
        }
    }
}
