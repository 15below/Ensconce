using Cake.Core;
using Cake.Core.Diagnostics;

namespace FifteenBelow.Octopus.Cake
{
    internal static class LogExtensions
    {
        internal static void LogError(this ICakeContext context, string message)
        {
            context.Log.Write(Verbosity.Quiet, LogLevel.Error, message);
        }

        internal static void LogWarning(this ICakeContext context, string message)
        {
            context.Log.Write(Verbosity.Minimal, LogLevel.Warning, message);
        }

        internal static void LogInfo(this ICakeContext context, string message)
        {
            context.Log.Write(Verbosity.Normal, LogLevel.Information, message);
        }

        internal static void LogDebug(this ICakeContext context, string message)
        {
            context.Log.Write(Verbosity.Verbose, LogLevel.Verbose, message);
        }

        internal static void LogTrace(this ICakeContext context, string message)
        {
            context.Log.Write(Verbosity.Diagnostic, LogLevel.Debug, message);
        }
    }
}
