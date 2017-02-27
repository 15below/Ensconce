using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Threading;

namespace Ensconce
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
            if (!directory.EndsWith(@"\")) directory = directory + @"\";

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
                        item.Process.Kill();
                        item.Process.WaitForExit();
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

                Logging.Log("Still waiting for service {0} to stop", serviceName);
                Thread.Sleep(1000);
                waitAttempt++;
            }

            if (waitAttempt >= maxWait)
            {
                Logging.LogError("Service {0} didn't stop in {1} seconds!", serviceName, maxWait);
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
            }
        }

        private static IEnumerable<ServiceDetails> GetServicesInstalledInDirectory(string directory)
        {
            if (!directory.EndsWith(@"\")) directory = directory + @"\";

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

        private class ServiceDetails
        {
            public string DisplayName { get; set; }
            public string ServiceName { get; set; }
            public string PathName { get; set; }
        }
    }
}