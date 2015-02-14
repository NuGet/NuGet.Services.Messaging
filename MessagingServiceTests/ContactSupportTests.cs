﻿using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using NuGet.Services.Messaging;
using NuGet.Services.Metadata.Catalog.Persistence;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MessagingServiceTests
{

    [TestClass]
    public class ContactSupportTests
    {
        private static TestServer _server;
        private static TestServer _server_noStorage;
        // private static TestServer _server_noAAD;

        private static StorageManager _storageManager;
        private static StorageManager _storageManagerFake;

        private const string TestJSONPath = "../../sampleJSON/ContactSupportSamples/ContactSupport.json";
        private const string TestJSONPath_InsufficientParameters = "../../sampleJSON/ContactSupportSamples/ContactSupport_MissingPackageId.json";
        private const string TestJSONPath_ExtraParameters = "../../sampleJSON/ContactSupportSamples/ContactSupport_ExtraParameter.json";
        //private const string TestJSONPath_InvalidParameters = "ContactSupport_InvalidParams.json";
        private const string fileStorageLocation = "../../Messages";

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            // ensure uses file storage
            _storageManager = new StorageManager(new FileStorage("http://localhost:8000/messages", fileStorageLocation));
            _server = TestServer.Create(app =>
            {
                var startup = new Startup();
                startup.ConfigureStorageManager(_storageManager);
                startup.Configuration(app);
            });

            // Inject connection failure to storage:  use fake storage.  Allows creation, but fails on save.
            _storageManagerFake = new StorageManager(new FakeFileStorage("http://localhost:8000/messages", fileStorageLocation));
            _server_noStorage = TestServer.Create(app =>
            {
                var startup = new Startup();
                startup.ConfigureStorageManager(_storageManagerFake);
                startup.Configuration(app);
            });

            // Inject connection failureto AAD:  use fake AAD connection.  Allows creation, but fails on access
            /*AADManager aadManager = new AADManager(FakeAADConnection(...));
            _server_noAAD = TestServer.Create(app =>
            {
                var startup = new Startup();
                startup.ConfigureAADManager(aadManager);
                startup.Configuration(app);
            });
            */

        }

        [ClassCleanup]
        public static void ClassClean()
        {
            _server.Dispose();
            _server_noStorage.Dispose();
        }


        [TestCleanup]
        public async void Cleanup()
        {
            // delete all contents of fileStorageLocation
            bool result = await _storageManager.Delete("email1");
        }



        [TestMethod]
        public async Task TestContactSupport()
        {
            string fileContent = File.ReadAllText(TestJSONPath);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/contactSupport", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                StreamReader errorsReader = new StreamReader(errors);
                string errorsString = errorsReader.ReadToEnd();
                JObject errorsJSON = JObject.Parse(errorsString);
                // print errors
            }

            // says service unavailable, but has the response?
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);


            // Check message
            StreamStorageContent messageJSON = (StreamStorageContent)await _storageManager.Load("email1");
            StreamReader reader = new StreamReader(messageJSON.GetContentStream());
            string bodyContent = reader.ReadToEnd();
            JObject root = JObject.Parse(bodyContent);

            Assert.AreEqual("support@powershellgallery.com", root["to"]);
            Assert.AreEqual("someuser@live.com", root["from"]);
            Assert.AreEqual("someuser@live.com", root["cc"]);
            Assert.AreEqual("[PowerShellGallery] Support Request for 'SomeTestPackage' version 1.0.0 (Reason: This package contains sensitive data.)", root["subject"]);
            Assert.AreEqual(@"**Email:** rebro-1 (someuser@live.com)

**Module:** SomeTestPackage
http://www.powershellgallery.com/packages/SomeTestPackage

**Version:** 1.0.0
http://www.powershellgallery.com/packages/SomeTestPackage/1.0.0

**Reason:**
This package contains sensitive data.

**Message:**
Please remove this package right away, it has all my secrets in it!", root["body"]["text"]);
            Assert.AreEqual(@"**Email:** rebro-1 (someuser@live.com)

**Module:** SomeTestPackage
http://www.powershellgallery.com/packages/SomeTestPackage

**Version:** 1.0.0
http://www.powershellgallery.com/packages/SomeTestPackage/1.0.0

**Reason:**
This package contains sensitive data.

**Message:**
Please remove this package right away, it has all my secrets in it!", root["body"]["html"]);

        }

        [TestMethod]
        public async Task TestContactSupport_InsufficientParameters()
        {
            string fileContent = File.ReadAllText(TestJSONPath_InsufficientParameters);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/contactSupport", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, errorsJSON["error"]);
            Assert.AreEqual("ContactSupport FAIL: Insufficient parameters.", errorsJSON["description"]);
            string expectedMissingParams = (new JArray(new List<string>() { "packageId" })).ToString();
            string actualMissingParams = ((JArray)errorsJSON["missingParameters"]).ToString();
            Assert.AreEqual(expectedMissingParams, actualMissingParams);

        }

        [TestMethod]
        public async Task TestContactSupport_ExtraParameters()
        {
            string fileContent = File.ReadAllText(TestJSONPath_ExtraParameters);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/contactSupport", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                StreamReader errorsReader = new StreamReader(errors);
                string errorsString = errorsReader.ReadToEnd();
                JObject errorsJSON = JObject.Parse(errorsString);
                // print errors
            }

            // says service unavailable, but has the response?
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);


            // Check message
            StreamStorageContent messageJSON = (StreamStorageContent)await _storageManager.Load("email1");
            StreamReader reader = new StreamReader(messageJSON.GetContentStream());
            string bodyContent = reader.ReadToEnd();
            JObject root = JObject.Parse(bodyContent);

            Assert.AreEqual("support@powershellgallery.com", root["to"]);
            Assert.AreEqual("someuser@live.com", root["from"]);
            Assert.AreEqual("someuser@live.com", root["cc"]);
            Assert.AreEqual("[PowerShellGallery] Support Request for 'SomeTestPackage' version 1.0.0 (Reason: This package contains sensitive data.)", root["subject"]);
            Assert.AreEqual(@"**Email:** rebro-1 (someuser@live.com)

**Module:** SomeTestPackage
http://www.powershellgallery.com/packages/SomeTestPackage

**Version:** 1.0.0
http://www.powershellgallery.com/packages/SomeTestPackage/1.0.0

**Reason:**
This package contains sensitive data.

**Message:**
Please remove this package right away, it has all my secrets in it!", root["body"]["text"]);
            Assert.AreEqual(@"**Email:** rebro-1 (someuser@live.com)

**Module:** SomeTestPackage
http://www.powershellgallery.com/packages/SomeTestPackage

**Version:** 1.0.0
http://www.powershellgallery.com/packages/SomeTestPackage/1.0.0

**Reason:**
This package contains sensitive data.

**Message:**
Please remove this package right away, it has all my secrets in it!", root["body"]["html"]);

        }

        /*
        [TestMethod]
        public void TestContactSupport_InvalidParameters()
        {
            
        }
        */
        /*
        [TestMethod]
        public void TestContactSupport_AADConnectionFailed()
        {

        }
         */

        [TestMethod]
        public async Task TestContactSupport_StorageConnectionFailed()
        {
            string fileContent = File.ReadAllText(TestJSONPath);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server_noStorage.HttpClient.PostAsync("/contactSupport", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.ServiceUnavailable, errorsJSON["error"]);
            Assert.AreEqual("ContactSupport FAIL: Storage unavailable.  Email not sent.", errorsJSON["description"]);

        }
    }


    
}