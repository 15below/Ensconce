using Ensconce.ReportingServices.SSRS2010;
using System;

namespace Ensconce.ReportingServices
{
    public class ReportingServicesCaller
    {
        private readonly string networkDomain;
        private readonly string networkLogin;
        private readonly string networkPassword;
        private readonly ReportingService2010Soap rs;

        public ReportingServicesCaller(string reportingServicesUrl, string networkDomain, string networkLogin, string networkPassword)
        {
            if (!Uri.TryCreate(reportingServicesUrl, UriKind.RelativeOrAbsolute, out Uri reportingServicesUri))
            {
                throw new UriFormatException(string.Format("reporting services uri of '{0}' is invalid!", reportingServicesUri));
            }

            if (string.IsNullOrWhiteSpace(networkPassword))
            {
                throw new NullReferenceException("networkPassword is null or empty!");
            }

            if (string.IsNullOrWhiteSpace(networkDomain))
            {
                throw new NullReferenceException("networkDomain is null or empty!");
            }

            if (string.IsNullOrWhiteSpace(networkLogin))
            {
                throw new NullReferenceException("networkLogin is null or empty!");
            }

            this.networkDomain = networkDomain;
            this.networkLogin = networkLogin;
            this.networkPassword = networkPassword;

            rs = new ReportingService2010SoapClient(ReportingService2010SoapClient.EndpointConfiguration.ReportingService2010Soap, reportingServicesUri.AbsoluteUri);
        }

        public TRet CallReport<TRet>(Func<ReportingService2010Soap, TRet> run)
        {
            rs.LogonUser(new LogonUserRequest { userName = networkLogin, password = networkPassword, authority = networkDomain });

            try
            {
                return run(rs);
            }
            finally
            {
                rs.Logoff(new LogoffRequest());
            }
        }

        public void CallReport(Action<ReportingService2010Soap> run)
        {
            rs.LogonUser(new LogonUserRequest { userName = networkLogin, password = networkPassword, authority = networkDomain });

            try
            {
                run(rs);
            }
            finally
            {
                rs.Logoff(new LogoffRequest());
            }
        }
    }
}
