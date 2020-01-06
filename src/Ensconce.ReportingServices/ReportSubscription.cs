using Ensconce.ReportingServices.SSRS2010;

namespace Ensconce.ReportingServices
{
    public class ReportSubscription
    {
        public bool Enabled;
        public string Name;
        public string Path;
        public ExtensionSettings ExtensionSettings;
        public string Description;
        public string EventType;
        public string ScheduleXml;
        public ParameterValue[] Parameters;
    }
}
