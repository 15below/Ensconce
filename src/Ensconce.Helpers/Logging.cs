using System;

namespace Ensconce.Helpers
{
    public static class Logging
    {
        private static readonly DateTime Started = DateTime.Now;
        public static bool Quiet { get; set; }
        public static bool ReadFromStdIn { get; set; }

        public static void Log(string message, params object[] values)
        {
            if (Quiet || ReadFromStdIn)
            {
                return;
            }

            Console.Write("[INFO] +{0:mm\\:ss\\.fff} - ", DateTime.Now - Started);
            Console.WriteLine(message, values);
        }

        public static void LogWarn(string message, params object[] values)
        {
            if (Quiet || ReadFromStdIn)
            {
                return;
            }

            Console.Write("[WARN] +{0:mm\\:ss\\.fff} - ", DateTime.Now - Started);
            Console.WriteLine(message, values);
        }

        public static void LogError(string message, params object[] values)
        {
            if (Quiet || ReadFromStdIn)
            {
                return;
            }

            Console.Error.Write("[ERROR] +{0:mm\\:ss\\.fff} - ", DateTime.Now - Started);
            Console.Error.WriteLine(message, values);
        }
    }
}
