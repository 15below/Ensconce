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
        private static readonly Regex HandleRegEx = new Regex(@"^(?<exe>.*?)\s*pid:\s*(?<pid>[0-9]+)\s*type:\s*(?<type>\w*)\s*?(?<hash>[A-Z0-9]+)\s*:\s*(?<path>.*)$");

        internal static void StopAndDeleteServicesInDirectory(string directory)
        {
            Logging.Log("Finding services to stop and delete in {0}", directory);
            var services = GetServicesInstalledInDirectory(directory).ToList();
            if (services.Any())
            {
                Logging.Log("{0} services found in {1}", services.Count, directory);
                foreach (var serviceDetails in services)
                {
                    var service = ServiceController.GetServices().First(svc => svc.DisplayName == serviceDetails.DisplayName);
                    if (service.Status != ServiceControllerStatus.Stopped)
                    {
                        Logging.Log("Stopping service {0}", serviceDetails.DisplayName);
                        service.Stop();
                    }

                    VerifyServiceStopped(serviceDetails.DisplayName, 30);

                    Logging.Log("Uninstalling service {0}", serviceDetails.DisplayName);
                    Process.Start("sc", $"delete \"{serviceDetails.ServiceName}\"")?.WaitForExit();

                    VerifyServiceUninstall(serviceDetails.DisplayName, 30);
                }
            }
            else
            {
                Logging.Log("No services found in {0}", directory);
            }
        }

        internal static void StopProcessesInDirectory(string directory)
        {
            Logging.Log("Stopping processes in directory: {0}", directory);
            var processes = GetProcessesRunningInDirectory(directory);

            if (processes.Any())
            {
                Logging.Log("{0} processes found in {1}", processes.Count, directory);

                foreach (var process in processes)
                {
                    try
                    {
                        Logging.Log("Attempting to kill {0}", process.Path);
                        process.Process.Kill();
                    }
                    catch (Win32Exception ex) when (ex.NativeErrorCode == 5) //Access Denied
                    {
                        // Process is already terminating.
                    }
                    catch (InvalidOperationException)
                    {
                        // Process has already terminated.
                    }

                    process.Process.WaitForExit(30000);
                }
            }
            else
            {
                Logging.Log("No processes found in {0}", directory);
            }
        }

        internal static void ReleaseHandlesInDirectory(string directory)
        {
            var handleExe = Path.Combine(Arguments.EnsconceDir, "Tools", "Handle", "handle.exe");

            Logging.Log("Finding handles in {0}", directory);
            var handles = RunHandleAndGetOutput(handleExe, directory).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(x => HandleRegEx.Match(x)).Where(x => x.Success).ToList();
            if (handles.Any())
            {
                Logging.Log("{0} handles found in {1}", handles.Count, directory);
                foreach (var handle in handles)
                {
                    try
                    {
                        Logging.Log("Attempting to release handle hash {0} on process {1} (PID {2})", handle.Groups["hash"].Value, handle.Groups["exe"].Value, handle.Groups["pid"].Value);
                        ReleaseHandle(handleExe, handle.Groups["hash"].Value, handle.Groups["pid"].Value);
                    }
                    catch (Exception)
                    {
                        Logging.LogWarn("Unable to stop handle hash {0} on process {1} (PID: {2})", handle.Groups["hash"].Value, handle.Groups["exe"].Value, handle.Groups["pid"].Value);
                    }
                }
            }
            else
            {
                Logging.Log("No handles found in {0}", directory);
            }
        }

        private static void VerifyServiceStopped(string serviceName, int maxAttempts)
        {
            var attempt = 0;
            while (attempt < maxAttempts)
            {
                if (ServiceController.GetServices().First(svc => svc.DisplayName == serviceName).Status == ServiceControllerStatus.Stopped)
                {
                    break;
                }

                Logging.Log("Still waiting for service {0} to stop after {1} seconds", serviceName, attempt);
                Thread.Sleep(1000);
                attempt++;
            }

            if (attempt >= maxAttempts)
            {
                Logging.LogError("Service {0} didn't stop in {1} seconds!", serviceName, maxAttempts);
                throw new Exception($"{serviceName} failed to stop in {maxAttempts} seconds");
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

        private static List<ServiceDetails> GetServicesInstalledInDirectory(string directory)
        {
            if (!directory.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                directory += Path.DirectorySeparatorChar;
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
                })
                .ToList();
        }

        private static List<ProcessDetails> GetProcessesRunningInDirectory(string directory)
        {
            if (!directory.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                directory += Path.DirectorySeparatorChar;
            }

            var wmiQueryString = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process";

            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            using (var results = searcher.Get())
            {
                var directoryFullName = new DirectoryInfo(directory).FullName;

                var query = (from p in Process.GetProcesses()
                             join mo in results.Cast<ManagementObject>()
                                 on p.Id equals (int)(uint)mo["ProcessId"]
                             select new ProcessDetails { Process = p, Path = (string)mo["ExecutablePath"] })
                             .Where(item => !string.IsNullOrEmpty(item.Path) && Path.GetFullPath(item.Path).Contains(directoryFullName))
                             .ToList();

                return query;
            }
        }

        private static string RunHandleAndGetOutput(string handleExe, string searchFolder)
        {
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

        private static void ReleaseHandle(string handleExe, string handleHash, string pid)
        {
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

        private class ProcessDetails
        {
            public Process Process { get; set; }
            public string Path { get; set; }
        }
    }
}
