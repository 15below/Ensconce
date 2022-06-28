using Ensconce.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using System.Threading;

namespace Ensconce.Cli
{
    internal static class ApplicationInteraction
    {
        internal static void StopAndDeleteServicesInDirectory(string directory)
        {
            foreach (var serviceDetails in GetServicesInstalledInDirectory(directory))
            {
                var service = ServiceController.GetServices().First(svc => svc.DisplayName == serviceDetails.DisplayName);
                if (service.Status != ServiceControllerStatus.Stopped)
                {
                    Logging.Log("Stopping service {0}", serviceDetails.DisplayName);
                    service.Stop();
                }

                VerifyServiceStopped(serviceDetails.DisplayName, 30);

                Logging.Log("Uninstalling service {0}", serviceDetails.DisplayName);
                Process.Start("sc", string.Format("delete \"{0}\"", serviceDetails.ServiceName)).WaitForExit();

                VerifyServiceUninstall(serviceDetails.DisplayName, 30);
            }
        }

        internal static void StopProcessesInDirectory(string directory)
        {
            if (!directory.EndsWith(@"\"))
            {
                directory = directory + @"\";
            }

            Logging.Log("Stopping processes in directory: {0}", directory);
            var wmiQueryString = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process";

            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            using (var results = searcher.Get())
            {
                var query = from p in Process.GetProcesses()
                            join mo in results.Cast<ManagementObject>()
                            on p.Id equals (int)(uint)mo["ProcessId"]
                            select new
                            {
                                Process = p,
                                Path = (string)mo["ExecutablePath"]
                            };

                foreach (var item in query)
                {
                    if (!string.IsNullOrEmpty(item.Path) && Path.GetFullPath(item.Path).Contains(new DirectoryInfo(directory).FullName))
                    {
                        const int Win32ErrorCodeAccessDenied = 5;
                        try
                        {
                            Logging.Log("Attempting to kill {0}", item.Path);
                            item.Process.Kill();
                        }
                        catch (Win32Exception ex) when (ex.NativeErrorCode == Win32ErrorCodeAccessDenied)
                        {
                            // Process is already terminating.
                        }
                        catch (InvalidOperationException)
                        {
                            // Process has already terminated.
                        }
                        item.Process.WaitForExit();
                    }
                }
            }
        }

        internal static void KillHandlesInDirectory(string directory)
        {
            var handleExe = Path.Combine(Arguments.DeployToolsDir, "Tools", "Handle", "handle.exe");

            var handleRegEx = new Regex(@"^(?<exe>.*?)\s*pid:\s*(?<pid>[0-9]+)\s*type:\s*(?<type>\w*)\s*?(?<hash>[A-Z0-9]+)\s*:\s*(?<path>.*)$", RegexOptions.Compiled);
            var handles = RunHandleAndGetOutput(handleExe, directory);
            foreach (var handle in handles.Split('\n'))
            {
                var matches = handleRegEx.Match(handle);
                if (matches.Success)
                {
                    try
                    {
                        KillHandle(handleExe, matches.Groups["hash"].Value, matches.Groups["exe"].Value, matches.Groups["pid"].Value);
                    }
                    catch (Exception)
                    {
                        Logging.LogWarn("Unable to stop hash {0} on process {1} (PID: {2})", matches.Groups["hash"].Value, matches.Groups["exe"].Value, matches.Groups["pid"].Value);
                    }
                }
            }
        }

        private static void VerifyServiceStopped(string serviceName, int maxWait)
        {
            var waitAttempt = 0;
            while (waitAttempt < maxWait)
            {
                if (ServiceController.GetServices().First(svc => svc.DisplayName == serviceName).Status == ServiceControllerStatus.Stopped)
                {
                    break;
                }

                Logging.Log("Still waiting for service {0} to stop after {1} seconds", serviceName, waitAttempt);
                Thread.Sleep(1000);
                waitAttempt++;
            }

            if (waitAttempt >= maxWait)
            {
                Logging.LogError("Service {0} didn't stop in {1} seconds!", serviceName, maxWait);
                throw new Exception($"{serviceName} failed to stop in {maxWait} seconds");
            }
        }

        private static void VerifyServiceUninstall(string serviceName, int maxWait)
        {
            var waitAttempt = 0;
            while (waitAttempt < maxWait)
            {
                if (!ServiceController.GetServices().Any(svc => svc.DisplayName == serviceName))
                {
                    break;
                }

                Logging.Log("Still waiting for service {0} to be removed", serviceName);
                Thread.Sleep(1000);
                waitAttempt++;
            }

            if (waitAttempt >= maxWait)
            {
                Logging.LogError("Service {0} still installed after {1} seconds!", serviceName, maxWait);
                throw new Exception($"{serviceName} failed to uninstall in {maxWait} seconds");
            }
        }

        private static IEnumerable<ServiceDetails> GetServicesInstalledInDirectory(string directory)
        {
            if (!directory.EndsWith(@"\"))
            {
                directory = directory + @"\";
            }

            var wqlObjectQuery = new WqlObjectQuery("SELECT * FROM Win32_Service");
            var managementObjectSearcher = new ManagementObjectSearcher(wqlObjectQuery);
            var managementObjectCollection = managementObjectSearcher.Get();

            return managementObjectCollection
                .OfType<ManagementObject>()
                .Where(svc => svc.GetPropertyValue("PathName") != null)
                .Where(svc => svc.GetPropertyValue("PathName").ToString().Contains(directory))
                .Select(svc => new ServiceDetails
                {
                    PathName = svc.GetPropertyValue("PathName").ToString(),
                    DisplayName = svc.GetPropertyValue("DisplayName").ToString(),
                    ServiceName = svc.GetPropertyValue("Name").ToString()
                });
        }

        private static string RunHandleAndGetOutput(string handleExe, string searchFolder)
        {
            Logging.Log("Finding handles in {0}", searchFolder);
            var p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = handleExe;
            p.StartInfo.Arguments = $"{searchFolder} /nobanner";
            p.StartInfo.Verb = "runas";
            p.Start();
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return output;
        }

        private static void KillHandle(string handleExe, string handleHash, string process, string pid)
        {
            Logging.Log("Attempting to kill hash {0} on process {1} (PID {2})", handleHash, process, pid);
            var p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = handleExe;
            p.StartInfo.Arguments = $"-c {handleHash} -y -p {pid} /nobanner";
            p.StartInfo.Verb = "runas";
            p.Start();
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                throw new Exception("non-zero exit code from Handle");
            }
        }

        private class ServiceDetails
        {
            public string DisplayName { get; set; }
            public string ServiceName { get; set; }
            public string PathName { get; set; }
        }
    }
}
