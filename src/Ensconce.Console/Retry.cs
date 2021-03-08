using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Ensconce.Console
{
    /// <summary>
    /// Retry logic applied from this Stack Overflow thread
    /// http://stackoverflow.com/questions/1563191/c-sharp-cleanest-way-to-write-retry-logic
    /// </summary>
    public static class Retry
    {
        private static readonly int retryCount = 5;

        public static void Do(Action action, TimeSpan retryInterval, Type[] doNotRetryTheseExceptions = null)
        {
            Do<object>(() =>
                {
                    action();
                    return null;
                },
                retryInterval,
                doNotRetryTheseExceptions);
        }

        public static T Do<T>(Func<T> action, TimeSpan retryInterval, Type[] doNotRetryTheseExceptions = null)
        {
            var exceptions = new List<Exception>();

            for (var retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    return action();
                }
                catch (Exception ex) when (doNotRetryTheseExceptions?.Any(t => ex.GetType() == t) == true)
                {
                    throw;
                }
                catch (Exception ex) when (retry + 1 < retryCount)
                {
                    System.Console.Out.WriteLine($"Something went wrong on attempt {retry + 1} of {retryCount}, but we're going to try again in {retryInterval.TotalMilliseconds}ms...");
                    exceptions.Add(ex);
                    Thread.Sleep(retryInterval);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    System.Console.Out.WriteLine($"Something went wrong on attempt {retry + 1} of {retryCount}, throwing all {exceptions.Count} exceptions...");
                }
            }

            throw new AggregateException(exceptions);
        }
    }
}
