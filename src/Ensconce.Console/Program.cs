using System;
using System.Threading;

namespace Ensconce.Console
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                MainLogic(args);
                return 0;
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine("Something went wrong. :(");
                System.Console.Error.WriteLine(e.Message);
                System.Console.Error.WriteLine(e.StackTrace);
                return -1;
            }
        }

        private static void MainLogic(string[] args)
        {
            Logging.Log("-------------------------------------------");
            Logging.Log("    15below Ensconce Deployment Helper     ");
            Logging.Log("-------------------------------------------");

            Arguments.SetUpAndParseOptions(args);

            Logging.Log("Arguments parsed");

            if (!string.IsNullOrWhiteSpace(Arguments.DictionarySavePath) || !string.IsNullOrWhiteSpace(Arguments.DictionaryPostUrl))
            {
                DictionaryExport.ExportTagDictionary();
                return;
            }

            if (Arguments.ReadFromStdIn)
            {
                using (var input = System.Console.In)
                {
                    System.Console.Out.Write(input.ReadToEnd().Render());
                }
                // No other operations can be performed when reading from stdin
                return;
            }

            if (Arguments.DeployReports || Arguments.DeployReportingRole)
            {
                Reporting.RunReportingServices();
                // No other operations can be performed when deploying reports
                return;
            }

            if (Arguments.UpdateConfig)
            {
                TagSubstitution.DefaultUpdate();
            }

            if (!string.IsNullOrEmpty(Arguments.TemplateFilters))
            {
                TagSubstitution.UpdateFiles();
            }

            if (!string.IsNullOrEmpty(Arguments.ConnectionString) || !string.IsNullOrEmpty(Arguments.DatabaseName))
            {
                DatabaseInteraction.DoDeployment();
            }

            if (Arguments.Replace)
            {
                Arguments.DeployTo.ForEach(FileInteraction.DeleteDirectory);
                Thread.Sleep(500); // Allow for the delete to complete
            }

            if (Arguments.CopyTo || Arguments.Replace)
            {
                Arguments.DeployTo.ForEach(dt => FileInteraction.CopyDirectory(Arguments.DeployFrom, dt));
            }

            Logging.Log("Ensconce operation complete");
        }
    }
}