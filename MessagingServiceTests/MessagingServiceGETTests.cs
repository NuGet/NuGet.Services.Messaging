using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using NuGet.Services.Messaging;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NuGet.Services.Messaging.Tests
{
    

    [TestClass]
    public class ReportAbuseReasonsTests
    {
        private static TestServer _server;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _server = TestServer.Create<Startup>();
        }

        [ClassCleanup]
        public static void ClassClean()
        {
            _server.Dispose();
        }

        [TestMethod]
        public async Task TestReportAbuseReasons_NuGet()
        {
            HttpResponseMessage response = await _server.HttpClient.GetAsync("/reasons/reportAbuse/NuGet");
            Stream responseStream = response.Content.ReadAsStreamAsync().Result;


            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json", response.Content.Headers.ContentType.ToString());

            StreamReader reader = new StreamReader(responseStream);
            String reasonsString = reader.ReadToEnd();
            JObject root = JObject.Parse(reasonsString);
            JArray reasons = root.Value<JArray>("reasons");

            // check reasons
            Assert.AreEqual(reasons[0], "The package owner is fraudulently claiming authorship");
            Assert.AreEqual(reasons[1], "The package violates a license I own");
            Assert.AreEqual(reasons[2], "The package contains malicious code");
            Assert.AreEqual(reasons[3], "The package has a bug/failed to install");
            Assert.AreEqual(reasons[4], "Other");

        }

        [TestMethod]
        public async Task TestReportAbuseReasons_PowerShellGallery()
        {
            HttpResponseMessage response = await _server.HttpClient.GetAsync("/reasons/reportAbuse/PowerShellGallery");
            Stream responseStream = response.Content.ReadAsStreamAsync().Result;


            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json", response.Content.Headers.ContentType.ToString());

            StreamReader reader = new StreamReader(responseStream);
            String reasonsString = reader.ReadToEnd();
            JObject root = JObject.Parse(reasonsString);
            JArray reasons = root.Value<JArray>("reasons");

            // check reasons
            Assert.AreEqual(reasons[0], "The module owner is fraudulently claiming authorship");
            Assert.AreEqual(reasons[1], "The module violates a license I own");
            Assert.AreEqual(reasons[2], "The module contains malicious code");
            Assert.AreEqual(reasons[3], "The module has a bug/failed to install");
            Assert.AreEqual(reasons[4], "Other");
        }

        [TestMethod]
        public async Task TestReportAbuseReasons_InvalidBrand()
        {
            HttpResponseMessage response = await _server.HttpClient.GetAsync("/reasons/reportAbuse/SomeBrand");
            Stream errorStream = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            StreamReader reader = new StreamReader(errorStream);
            String errorString = reader.ReadToEnd();

            Assert.AreEqual("NotFound", errorString);

        }

        [TestMethod]
        public async Task TestReportAbuseReasons_NoBrand()
        {
            HttpResponseMessage response = await _server.HttpClient.GetAsync("/reasons/reportAbuse");
            Stream errorStream = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            StreamReader reader = new StreamReader(errorStream);
            String errorString = reader.ReadToEnd();

            Assert.AreEqual("NotFound", errorString);
        }

    }


    [TestClass]
    public class ContactSupportReasonsTests
    {
        private static TestServer _server;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _server = TestServer.Create<Startup>();
        }

        [ClassCleanup]
        public static void ClassClean()
        {
            _server.Dispose();
        }
        
        [TestMethod]
        public async Task TestContactSupportReasons_NuGet()
        {
            HttpResponseMessage response = await _server.HttpClient.GetAsync("/reasons/contactSupport/NuGet");
            Stream responseStream = response.Content.ReadAsStreamAsync().Result;


            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json", response.Content.Headers.ContentType.ToString());

            StreamReader reader = new StreamReader(responseStream);
            String reasonsString = reader.ReadToEnd();
            JObject root = JObject.Parse(reasonsString);
            JArray reasons = root.Value<JArray>("reasons");

            // check reasons
            Assert.AreEqual(reasons[0], "The package contains private/confidential data");
            Assert.AreEqual(reasons[1], "The package was published as the wrong version");
            Assert.AreEqual(reasons[2], "The package was not intended to be published publically on this gallery");
            Assert.AreEqual(reasons[3], "The package contains malicious code");
            Assert.AreEqual(reasons[4], "Other");

        }

        [TestMethod]
        public async Task TestContactSupportReasons_PowerShellGallery()
        {
            HttpResponseMessage response = await _server.HttpClient.GetAsync("/reasons/contactSupport/PowerShellGallery");
            Stream responseStream = response.Content.ReadAsStreamAsync().Result;


            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json", response.Content.Headers.ContentType.ToString());

            StreamReader reader = new StreamReader(responseStream);
            String reasonsString = reader.ReadToEnd();
            JObject root = JObject.Parse(reasonsString);
            JArray reasons = root.Value<JArray>("reasons");

            // check reasons
            Assert.AreEqual(reasons[0], "The module contains private/confidential data");
            Assert.AreEqual(reasons[1], "The module was published as the wrong version");
            Assert.AreEqual(reasons[2], "The module was not intended to be published publically on this gallery");
            Assert.AreEqual(reasons[3], "The module contains malicious code");
            Assert.AreEqual(reasons[4], "Other");

        }

        [TestMethod]
        public async Task TestContactSupportReasons_InvalidBrand()
        {
            HttpResponseMessage response = await _server.HttpClient.GetAsync("/reasons/contactSupport/SomeBrand");
            Stream errorStream = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            StreamReader reader = new StreamReader(errorStream);
            String errorString = reader.ReadToEnd();

            Assert.AreEqual("NotFound", errorString);
        }

        [TestMethod]
        public async Task TestContactSupportReasons_NoBrand()
        {
            HttpResponseMessage response = await _server.HttpClient.GetAsync("/reasons/contactSupport");
            Stream errorStream = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            StreamReader reader = new StreamReader(errorStream);
            String errorString = reader.ReadToEnd();

            Assert.AreEqual("NotFound", errorString);
        }


    }



}
