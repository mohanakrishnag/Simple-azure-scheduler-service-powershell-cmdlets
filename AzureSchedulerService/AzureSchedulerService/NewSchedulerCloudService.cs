using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.IO;

namespace AzureSchedulerService
{

    [Cmdlet(VerbsCommon.New, "SchedulerCloudService")]
    public class NewSchedulerCloudService : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string SubscriptionId;

        [Parameter(Position = 0, Mandatory = true)]
        public string CloudServiceId;

        [Parameter(Position = 3, Mandatory = true)]
        public string CertificateThumbprint;

        protected override void ProcessRecord()
        {
            // X.509 certificate variables.
            X509Store certStore = null;
            X509Certificate2Collection certCollection = null;
            X509Certificate2 certificate = null;

            // Request and response variables.
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;

            // Stream variables.
            Stream responseStream = null;
            StreamReader reader = null;

            // URI variable.
            Uri requestUri = null;

            // The thumbprint for the certificate. This certificate would have been
            // previously added as a management certificate within the Windows Azure management portal.
            string thumbPrint = CertificateThumbprint;

            // Open the certificate store for the current user.
            certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            certStore.Open(OpenFlags.ReadOnly);

            // Find the certificate with the specified thumbprint.
            certCollection = certStore.Certificates.Find(
                                 X509FindType.FindByThumbprint,
                                 thumbPrint,
                                 false);

            // Close the certificate store.
            certStore.Close();

            // Check to see if a matching certificate was found.
            if (0 == certCollection.Count)
            {
                throw new Exception("No certificate found containing thumbprint " + thumbPrint);
            }

            // A matching certificate was found.
            certificate = certCollection[0];


            // Create the request.
            requestUri = new Uri("https://management.core.windows.net/"
                                 + SubscriptionId
                                 + "/cloudServices/" + CloudServiceId);

            httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(requestUri);

            // Add the certificate to the request.
            httpWebRequest.ClientCertificates.Add(certificate);
            httpWebRequest.Method = "PUT";
            httpWebRequest.Headers.Add("x-ms-version", "2012-03-01");
            httpWebRequest.ContentType = "application/xml; charset=utf-8";
            //httpWebRequest.ContentLength = 239;


            //Service name is hardcoded for now..need to get from input
            string str = @"<CloudService xmlns:i='http://www.w3.org/2001/XMLSchema-instance' xmlns='http://schemas.microsoft.com/windowsazure'><Label>GmkServiceName</Label><Description>testing</Description><GeoRegion>uswest</GeoRegion></CloudService";
            byte[] bodyStart = System.Text.Encoding.UTF8.GetBytes(str.ToString());
            Stream dataStream = httpWebRequest.GetRequestStream();
            dataStream.Write(bodyStart, 0, str.ToString().Length);

            // Make the call using the web request.
            httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            // Parse the web response.
            responseStream = httpWebResponse.GetResponseStream();
            reader = new StreamReader(responseStream);

            // Close the resources no longer needed.
            httpWebResponse.Close();
            responseStream.Close();
            reader.Close();
        }
    }
}

