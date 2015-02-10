using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Services.Messaging;
using System.Net;
using Newtonsoft.Json.Linq;

namespace MessagingServiceTests
{
    

    [TestClass]
    public class ReportAbuseReasonsTests
    {
        
        [TestMethod]
        public void TestReportAbuseReasons_NuGet()
        {
            WebRequest request = (HttpWebRequest) WebRequest.Create("http://localhost:11717/reasons/reportAbuse/NuGet");
            request.Method = WebRequestMethods.Http.Get;
            WebResponse response = request.GetResponse();

            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(((HttpWebResponse)response).ContentType, "application/json");

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            String bodyContent = reader.ReadToEnd();
            JObject root = JObject.Parse(bodyContent);

            JArray reasons = root.Value<JArray>("reasons");

            Assert.AreEqual(reasons[0], "The package owner is fraudulently claiming authorship");
            Assert.AreEqual(reasons[1], "The package violates a license I own");
            Assert.AreEqual(reasons[2], "The package contains malicious code");
            Assert.AreEqual(reasons[3], "The package has a bug/failed to install");
            Assert.AreEqual(reasons[4], "Other");
            
        }

        [TestMethod]
        public void TestReportAbuseReasons_PowerShellGallery()
        {
            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/reasons/reportAbuse/PowerShellGallery");
            request.Method = WebRequestMethods.Http.Get;
            WebResponse response = request.GetResponse();

            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(((HttpWebResponse)response).ContentType, "application/json");

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            String bodyContent = reader.ReadToEnd();
            JObject root = JObject.Parse(bodyContent);

            JArray reasons = root.Value<JArray>("reasons");

            Assert.AreEqual(reasons[0], "The module owner is fraudulently claiming authorship");
            Assert.AreEqual(reasons[1], "The module violates a license I own");
            Assert.AreEqual(reasons[2], "The module contains malicious code");
            Assert.AreEqual(reasons[3], "The module has a bug/failed to install");
            Assert.AreEqual(reasons[4], "Other");
        }

        [TestMethod]
        public void TestReportAbuseReasons_InvalidBrand()
        {
            Startup s = new Startup();
            PrivateObject pr = new PrivateObject(s);

            //pr.Invoke()

            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/reasons/reportAbuse/SomeBrand");
            request.Method = WebRequestMethods.Http.Get;
            WebResponse response = request.GetResponse();

            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.NotFound);
        }

        [TestMethod]
        public void TestReportAbuseReasons_NoBrand()
        {
            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/reasons/reportAbuse");
            request.Method = WebRequestMethods.Http.Get;
            WebResponse response = request.GetResponse();

            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.NotFound);
        }

    }


    [TestClass]
    public class ContactSupportReasonsTests
    {
        
        [TestMethod]
        public void TestContactSupportReasons_NuGet()
        {
            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/reasons/contactSupport/NuGet");
            request.Method = WebRequestMethods.Http.Get;
            WebResponse response = request.GetResponse();

            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(((HttpWebResponse)response).ContentType, "application/json");

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            String bodyContent = reader.ReadToEnd();
            JObject root = JObject.Parse(bodyContent);

            JArray reasons = root.Value<JArray>("reasons");

            Assert.AreEqual(reasons[0], "The package contains private/confidential data");
            Assert.AreEqual(reasons[1], "The package was published as the wrong version");
            Assert.AreEqual(reasons[2], "The package was not intended to be published publically on this gallery");
            Assert.AreEqual(reasons[3], "The package contains malicious code");
            Assert.AreEqual(reasons[4], "Other");

        }

        [TestMethod]
        public void TestContactSupportReasons_PowerShellGallery()
        {
            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/reasons/contactSupport/PowerShellGallery");
            request.Method = WebRequestMethods.Http.Get;
            WebResponse response = request.GetResponse();

            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(((HttpWebResponse)response).ContentType, "application/json");

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            String bodyContent = reader.ReadToEnd();
            JObject root = JObject.Parse(bodyContent);

            JArray reasons = root.Value<JArray>("reasons");

            Assert.AreEqual(reasons[0], "The module contains private/confidential data");
            Assert.AreEqual(reasons[1], "The module was published as the wrong version");
            Assert.AreEqual(reasons[2], "The module was not intended to be published publically on this gallery");
            Assert.AreEqual(reasons[3], "The module contains malicious code");
            Assert.AreEqual(reasons[4], "Other");
        }

        [TestMethod]
        public void TestContactSupportReasons_InvalidBrand()
        {
            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/reasons/contactSupport/SomeBrand");
            request.Method = WebRequestMethods.Http.Get;
            WebResponse response = request.GetResponse();

            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.NotFound);
        }

        [TestMethod]
        public void TestContactSupportReasons_NoBrand()
        {
            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/reasons/contactSupport");
            request.Method = WebRequestMethods.Http.Get;
            WebResponse response = request.GetResponse();

            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.NotFound);
        }


    }



}
