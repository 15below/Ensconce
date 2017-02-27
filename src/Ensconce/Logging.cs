using System;

namespace Ensconce
{
    internal static class Logging
    {
        private static readonly DateTime Started = DateTime.Now;

        internal static void Log(string message, params object[] values)
        {
            if (Arguments.Quiet || Arguments.ReadFromStdIn) return;
            Console.Write("+{0:mm\\:ss\\.ff} - ", DateTime.Now - Started);
            Console.WriteLine(message, values);
        }

        internal static void LogError(string message, params object[] values)
        {
            if (Arguments.Quiet || Arguments.ReadFromStdIn) return;
            Console.Error.Write("+{0:mm\\:ss\\.ff} - ", DateTime.Now - Started);
            Console.Error.WriteLine(message, values);
        }
    }
}