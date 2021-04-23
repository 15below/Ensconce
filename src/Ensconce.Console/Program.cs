using Ensconce.Cli;
using System;
using System.Diagnostics;
using System.Linq;

namespace Ensconce.Console
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                CliProgram.MainLogic(args);
                return 0;
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine(ExceptionDetails.GetDetails("Something went wrong.", e));
                return -1;
            }
            finally
            {
                if (Debugger.IsAttached)
                {
                    System.Console.Error.WriteLine("Press Enter to quit...");
                    System.Console.ReadLine();
                }
            }
        }
    }
}
