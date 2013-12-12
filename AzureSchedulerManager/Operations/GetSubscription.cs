using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Operations
{
    public static class GetSubscription
    {
        public static CertificateCloudCredentials GetCredentials(string path, string subscriptionName)
        {
            var profile = XDocument.Load(path);
            var subscriptionId = profile.Descendants("Subscription")
                .First(element => element.Attribute("Name").Value == subscriptionName)
                .Attribute("Id").Value;
            var certificate = new X509Certificate2(
                Convert.FromBase64String(profile.Descendants("PublishProfile").Single().Attribute("ManagementCertificate").Value));
            return new CertificateCloudCredentials(subscriptionId, certificate);
        }
    }
}
