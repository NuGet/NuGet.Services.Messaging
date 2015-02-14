using Microsoft.Owin.Testing;
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
    /// <summary>
    /// ContactOwners tests
    /// </summary>
    [TestClass]
    public class ContactOwnersTests
    {
        private static TestServer _server;
        private static TestServer _server_noStorage;
        // private static TestServer _server_noAAD;

        private static StorageManager _storageManager;
        private static StorageManager _storageManagerFake;

        private const string TestJSONPath = "../../sampleJSON/ContactOwnersSamples/ContactOwners.json";
        private const string TestJSONPath_InsufficientParameters = "../../sampleJSON/ContactOwnersSamples/ContactOwners_MissingPackageId.json";
        private const string TestJSONPath_ExtraParameters = "../../sampleJSON/ContactOwnersSamples/ContactOwners_ExtraParameter.json";
        //private const string TestJSONPath_InvalidParameters = "ContactOwners_InvalidParams.json";
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


        /// <summary>
        /// Tests happy path.
        /// </summary>
        [TestMethod]
        public async Task TestContactOwners()
        {
            string fileContent = File.ReadAllText(TestJSONPath);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/contactOwners", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                StreamReader errorsReader = new StreamReader(errors);
                string errorsString = errorsReader.ReadToEnd();
                JObject errorsJSON = JObject.Parse(errorsString);
            }

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);


            // Check message
            StreamStorageContent messageJSON = (StreamStorageContent)await _storageManager.Load("email1");
            StreamReader reader = new StreamReader(messageJSON.GetContentStream());
            string bodyContent = reader.ReadToEnd();
            JObject root = JObject.Parse(bodyContent);

            Assert.AreEqual("user1@gmail.com,user2@gmail.com,user3@gmail.com", root["to"]);
            Assert.AreEqual("someuser@live.com", root["from"]);
            Assert.AreEqual("someuser@live.com", root["cc"]);
            Assert.AreEqual("[PowerShellGallery] Message for owners of the module 'SomeTestPackage'", root["subject"]);
            Assert.AreEqual(@"User rebro-1 &lt;someuser@live.com&gt; sends the following message to the owners of module 'SomeTestPackage':
            
            Hello owners, I would like to be an owner too.  Please add me!

    -----------------------------------------------
    To stop receiving contact emails as an owner of this module, sign in to the PowerShell Gallery and 
    [change your email notification settings](/profile/edit).", root["body"]["text"]);
            Assert.AreEqual(@"User rebro-1 &lt;someuser@live.com&gt; sends the following message to the owners of module 'SomeTestPackage':
            
            Hello owners, I would like to be an owner too.  Please add me!

    -----------------------------------------------
    <em>
    To stop receiving contact emails as an owner of this module, sign in to the PowerShell Gallery and 
    [change your email notification settings](/profile/edit).
    </em>", root["body"]["html"]);

        }
    
        
        /// <summary>
        /// Sends a JSON file that is missing the "packageId" parameter.
        /// Expected result:  status code is BadRequest, and returns in error stream a JSON message containing missing parameters.
        /// </summary>
        [TestMethod]
        public async Task TestContactOwners_InsufficientParameters()
        {
            string fileContent = File.ReadAllText(TestJSONPath_InsufficientParameters);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/contactOwners", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, errorsJSON["error"]);
            Assert.AreEqual("ContactOwners FAIL: Insufficient parameters.", errorsJSON["description"]);
            string expectedMissingParams = (new JArray(new List<string>() { "packageId" })).ToString();
            string actualMissingParams = ((JArray)errorsJSON["missingParameters"]).ToString();
            Assert.AreEqual(expectedMissingParams, actualMissingParams);
        }


        /// <summary>
        /// Sends a JSON file containing an extra parameter.  
        /// Expected Result:  Do nothing, simply ignore extra parameters.
        /// </summary>
        [TestMethod]
        public async Task TestContactOwners_ExtraParameters()
        {
            string fileContent = File.ReadAllText(TestJSONPath_ExtraParameters);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/contactOwners", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                StreamReader errorsReader = new StreamReader(errors);
                string errorsString = errorsReader.ReadToEnd();
                JObject errorsJSON = JObject.Parse(errorsString);
                // print errors?
            }

            // says service unavailable, but has the response?
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);


            // Check message
            StreamStorageContent messageJSON = (StreamStorageContent)await _storageManager.Load("email1");
            StreamReader reader = new StreamReader(messageJSON.GetContentStream());
            string bodyContent = reader.ReadToEnd();
            JObject root = JObject.Parse(bodyContent);

            Assert.AreEqual("user1@gmail.com,user2@gmail.com,user3@gmail.com", root["to"]);
            Assert.AreEqual("someuser@live.com", root["from"]);
            Assert.AreEqual("someuser@live.com", root["cc"]);
            Assert.AreEqual("[PowerShellGallery] Message for owners of the module 'SomeTestPackage'", root["subject"]);
            Assert.AreEqual(@"User rebro-1 &lt;someuser@live.com&gt; sends the following message to the owners of module 'SomeTestPackage':
            
            Hello owners, I would like to be an owner too.  Please add me!

    -----------------------------------------------
    To stop receiving contact emails as an owner of this module, sign in to the PowerShell Gallery and 
    [change your email notification settings](/profile/edit).", root["body"]["text"]);
            Assert.AreEqual(@"User rebro-1 &lt;someuser@live.com&gt; sends the following message to the owners of module 'SomeTestPackage':
            
            Hello owners, I would like to be an owner too.  Please add me!

    -----------------------------------------------
    <em>
    To stop receiving contact emails as an owner of this module, sign in to the PowerShell Gallery and 
    [change your email notification settings](/profile/edit).
    </em>", root["body"]["html"]);

        }


        /*
        [TestMethod]
        public void TestContactOwners_InvalidParameters()
        {
            
        }
        */
        /*
        [TestMethod]
        public void TestContactOwners_AADConnectionFailed()
        {
            // Generate AAD connection failure (change connection strings so connection cannot be made?)

            // Check status is ServiceUnavailable

            //HttpStatusCode.ServiceUnavailable
        }
        */

        /// <summary>
        /// Simulate storage connection issue by providing an invalid
        /// </summary>
        [TestMethod]
        public async Task TestContactOwners_StorageConnectionFailed()
        {
            string fileContent = File.ReadAllText(TestJSONPath);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server_noStorage.HttpClient.PostAsync("/contactOwners", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.ServiceUnavailable, errorsJSON["error"]);
            Assert.AreEqual("ContactOwners FAIL: Storage unavailable.  Email not sent.", errorsJSON["description"]);
            
        }
    
    }


 

    [TestClass]
    public class ReportAbuseTests
    {
        private static TestServer _server;
        private static TestServer _server_noStorage;
        // private static TestServer _server_noAAD;

        private static StorageManager _storageManager;
        private static StorageManager _storageManagerFake;

        private const string TestJSONPath = "../../sampleJSON/ReportAbuseSamples/ReportAbuse.json";
        private const string TestJSONPath_InsufficientParameters = "../../sampleJSON/ReportAbuseSamples/ReportAbuse_MissingPackageId.json";
        private const string TestJSONPath_ExtraParameters = "../../sampleJSON/ReportAbuseSamples/ReportAbuse_ExtraParameter.json";
        //private const string TestJSONPath_InvalidParameters = "ReportAbuse_InvalidParams.json";
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
        public async Task TestReportAbuse()
        {
            string fileContent = File.ReadAllText(TestJSONPath);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/reportAbuse", postContent);
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
            Assert.AreEqual("rebro-somewhere@live.com", root["from"]);
            Assert.AreEqual("rebro-somewhere@live.com", root["cc"]);
            Assert.AreEqual("[PowerShellGallery] Support Request for 'SomeTestPackage' version 1.0.0 (Reason: This package has a bug in it.)", root["subject"]);
            Assert.AreEqual(@"**Email:** rebro-1 (rebro-somewhere@live.com)

**Module:** SomeTestPackage
http://www.powershellgallery.com/packages/SomeTestPackage

**Version:** 1.0.0
http://www.powershellgallery.com/packages/SomeTestPackage/1.0.0

**Reason:**
This package has a bug in it.

**Has the package owner been contacted?:**
Yes

**Message:**
Please remove this package right away, it is terrible!", root["body"]["text"]);
            Assert.AreEqual(@"**Email:** rebro-1 (rebro-somewhere@live.com)

**Module:** SomeTestPackage
http://www.powershellgallery.com/packages/SomeTestPackage

**Version:** 1.0.0
http://www.powershellgallery.com/packages/SomeTestPackage/1.0.0

**Reason:**
This package has a bug in it.

**Has the package owner been contacted?:**
Yes

**Message:**
Please remove this package right away, it is terrible!", root["body"]["html"]);

        }
    
       
        [TestMethod]
        public async Task TestReportAbuse_InsufficientParameters()
        {
            string fileContent = File.ReadAllText(TestJSONPath_InsufficientParameters);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/reportAbuse", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, errorsJSON["error"]);
            Assert.AreEqual("ReportAbuse FAIL: Insufficient parameters.", errorsJSON["description"]);
            string expectedMissingParams = (new JArray(new List<string>() { "packageId" })).ToString();
            string actualMissingParams = ((JArray)errorsJSON["missingParameters"]).ToString();
            Assert.AreEqual(expectedMissingParams, actualMissingParams);

        }

        [TestMethod]
        public async Task TestReportAbuse_ExtraParameters()
        {
            string fileContent = File.ReadAllText(TestJSONPath_ExtraParameters);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/reportAbuse", postContent);
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
            Assert.AreEqual("rebro-somewhere@live.com", root["from"]);
            Assert.AreEqual("rebro-somewhere@live.com", root["cc"]);
            Assert.AreEqual("[PowerShellGallery] Support Request for 'SomeTestPackage' version 1.0.0 (Reason: This package has a bug in it.)", root["subject"]);
            Assert.AreEqual(@"**Email:** rebro-1 (rebro-somewhere@live.com)

**Module:** SomeTestPackage
http://www.powershellgallery.com/packages/SomeTestPackage

**Version:** 1.0.0
http://www.powershellgallery.com/packages/SomeTestPackage/1.0.0

**Reason:**
This package has a bug in it.

**Has the package owner been contacted?:**
Yes

**Message:**
Please remove this package right away, it is terrible!", root["body"]["text"]);
            Assert.AreEqual(@"**Email:** rebro-1 (rebro-somewhere@live.com)

**Module:** SomeTestPackage
http://www.powershellgallery.com/packages/SomeTestPackage

**Version:** 1.0.0
http://www.powershellgallery.com/packages/SomeTestPackage/1.0.0

**Reason:**
This package has a bug in it.

**Has the package owner been contacted?:**
Yes

**Message:**
Please remove this package right away, it is terrible!", root["body"]["html"]);

        }

        /*
        [TestMethod]
        public void TestReportAbuse_InvalidParameters()
        {
            
        }
        */
        /*
        [TestMethod]
        public void TestReportAbuse_AADConnectionFailed()
        {

        }
        */

        [TestMethod]
        public async Task TestReportAbuse_StorageConnectionFailed()
        {
            string fileContent = File.ReadAllText(TestJSONPath);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server_noStorage.HttpClient.PostAsync("/reportAbuse", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.ServiceUnavailable, errorsJSON["error"]);
            Assert.AreEqual("ReportAbuse FAIL: Storage unavailable.  Email not sent.", errorsJSON["description"]);

        }

    }
    


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


    [TestClass]
    public class InvitePackageOwnerTests
    {
        private static TestServer _server;
        private static TestServer _server_noStorage;
        // private static TestServer _server_noAAD;

        private static StorageManager _storageManager;
        private static StorageManager _storageManagerFake;

        private const string TestJSONPath = "../../sampleJSON/InvitePackageOwner/InvitePackageOwner.json";
        private const string TestJSONPath_InsufficientParameters = "../../sampleJSON/InvitePackageOwner/InvitePackageOwner_MissingPackageId.json";
        private const string TestJSONPath_ExtraParameters = "../../sampleJSON/InvitePackageOwner/InvitePackageOwner_ExtraParameter.json";
        //private const string TestJSONPath_InvalidParameters = "InvitePackageOwner_InvalidParams.json";
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
        public async Task TestInvitePackageOwner()
        {
            string fileContent = File.ReadAllText(TestJSONPath);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/invitePackageOwner", postContent);
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

            Assert.AreEqual("someuser@live.com", root["to"]);
            Assert.AreEqual("someuser@live.com", root["from"]);
            Assert.AreEqual("[PowerShellGallery] The user 'rebro-1' wants to add you as an owner of the module 'SomeTestPackage'.", root["subject"]);
            Assert.AreEqual(@"The user 'rebro-1' wants to add you as an owner of the module 'SomeTestPackage'. 
If you do not want to be listed as an owner of this module, simply delete this email.

To accept this request and become a listed owner of the module, click the following URL:

[PowerShell Gallery](/owners/confirm)

Thanks,
The PowerShell Gallery Team", root["body"]["text"]);
            Assert.AreEqual(@"The user 'rebro-1' wants to add you as an owner of the module 'SomeTestPackage'. 
If you do not want to be listed as an owner of this module, simply delete this email.

To accept this request and become a listed owner of the module, click the following URL:

[PowerShell Gallery](/owners/confirm)

Thanks,
The PowerShell Gallery Team", root["body"]["html"]);

        }

        [TestMethod]
        public async Task TestInvitePackageOwner_InsufficientParameters()
        {
            string fileContent = File.ReadAllText(TestJSONPath_InsufficientParameters);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/invitePackageOwner", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, errorsJSON["error"]);
            Assert.AreEqual("InvitePackageOwner FAIL: Insufficient parameters.", errorsJSON["description"]);
            string expectedMissingParams = (new JArray(new List<string>() { "packageId" })).ToString();
            string actualMissingParams = ((JArray)errorsJSON["missingParameters"]).ToString();
            Assert.AreEqual(expectedMissingParams, actualMissingParams);

        }

        [TestMethod]
        public async Task TestInvitePackageOwner_ExtraParameters()
        {
            string fileContent = File.ReadAllText(TestJSONPath_ExtraParameters);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/invitePackageOwner", postContent);
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

            Assert.AreEqual("someuser@live.com", root["to"]);
            Assert.AreEqual("someuser@live.com", root["from"]);
            Assert.AreEqual("[PowerShellGallery] The user 'rebro-1' wants to add you as an owner of the module 'SomeTestPackage'.", root["subject"]);
            Assert.AreEqual(@"The user 'rebro-1' wants to add you as an owner of the module 'SomeTestPackage'. 
If you do not want to be listed as an owner of this module, simply delete this email.

To accept this request and become a listed owner of the module, click the following URL:

[PowerShell Gallery](/owners/confirm)

Thanks,
The PowerShell Gallery Team", root["body"]["text"]);
            Assert.AreEqual(@"The user 'rebro-1' wants to add you as an owner of the module 'SomeTestPackage'. 
If you do not want to be listed as an owner of this module, simply delete this email.

To accept this request and become a listed owner of the module, click the following URL:

[PowerShell Gallery](/owners/confirm)

Thanks,
The PowerShell Gallery Team", root["body"]["html"]);

        }

        /*
        [TestMethod]
        public void TestInvitePackageOwner_InvalidParameters()
        {
             
        }
        */
        /*
        [TestMethod]
        public void TestInvitePackageOwner_AADConnectionFailed()
        {

        }
        */

        [TestMethod]
        public async Task TestInvitePackageOwner_StorageConnectionFailed()
        {
            string fileContent = File.ReadAllText(TestJSONPath);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server_noStorage.HttpClient.PostAsync("/invitePackageOwner", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.ServiceUnavailable, errorsJSON["error"]);
            Assert.AreEqual("InvitePackageOwner FAIL: Storage unavailable.  Email not sent.", errorsJSON["description"]);

        }
    }





    [TestClass]
    public class NewAccountWelcomeTests
    {
        private static TestServer _server;
        private static TestServer _server_noStorage;
        // private static TestServer _server_noAAD;

        private static StorageManager _storageManager;
        private static StorageManager _storageManagerFake;

        private const string TestJSONPath = "../../sampleJSON/NewAccountWelcomeSamples/NewAccountWelcome.json";
        private const string TestJSONPath_InsufficientParameters = "../../sampleJSON/NewAccountWelcomeSamples/NewAccountWelcome_MissingEmail.json";
        private const string TestJSONPath_ExtraParameters = "../../sampleJSON/NewAccountWelcomeSamples/NewAccountWelcome_ExtraParameter.json";
        //private const string TestJSONPath_InvalidParameters = "NewAccountWelcome_InvalidParams.json";
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
        public async Task TestNewAccountWelcome()
        {
            string fileContent = File.ReadAllText(TestJSONPath);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/newAccountWelcome", postContent);
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

            Assert.AreEqual("Some_Email@live.com", root["to"]);
            Assert.AreEqual("support@nuget.org", root["from"]);
            Assert.AreEqual("[NuGet Gallery] Please verify your account.", root["subject"]);
            Assert.AreEqual(@"Thank you for registering with the NuGet Gallery. 
We can't wait to see what packages you'll upload.

So we can be sure to contact you, please verify your email address and click the following link:

/profile/email/verify

Thanks,
The NuGet Team", root["body"]["text"]);
            Assert.AreEqual(@"Thank you for registering with the NuGet Gallery. 
We can't wait to see what packages you'll upload.

So we can be sure to contact you, please verify your email address and click the following link:

/profile/email/verify

Thanks,
The NuGet Team", root["body"]["html"]);

        }

        [TestMethod]
        public async Task TestNewAccountWelcome_InsufficientParameters()
        {
            string fileContent = File.ReadAllText(TestJSONPath_InsufficientParameters);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/newAccountWelcome", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, errorsJSON["error"]);
            Assert.AreEqual("NewAccountWelcome FAIL: Insufficient parameters.", errorsJSON["description"]);
            string expectedMissingParams = (new JArray(new List<string>() { "email" })).ToString();
            string actualMissingParams = ((JArray)errorsJSON["missingParameters"]).ToString();
            Assert.AreEqual(expectedMissingParams, actualMissingParams);

        }

        [TestMethod]
        public async Task TestNewAccountWelcome_ExtraParameters()
        {
            string fileContent = File.ReadAllText(TestJSONPath_ExtraParameters);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/newAccountWelcome", postContent);
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

            Assert.AreEqual("Some_Email@live.com", root["to"]);
            Assert.AreEqual("support@nuget.org", root["from"]);
            Assert.AreEqual("[NuGet Gallery] Please verify your account.", root["subject"]);
            Assert.AreEqual(@"Thank you for registering with the NuGet Gallery. 
We can't wait to see what packages you'll upload.

So we can be sure to contact you, please verify your email address and click the following link:

/profile/email/verify

Thanks,
The NuGet Team", root["body"]["text"]);
            Assert.AreEqual(@"Thank you for registering with the NuGet Gallery. 
We can't wait to see what packages you'll upload.

So we can be sure to contact you, please verify your email address and click the following link:

/profile/email/verify

Thanks,
The NuGet Team", root["body"]["html"]);

        }

        /*
        [TestMethod]
        public void TestNewAccountWelcome_InvalidParameters()
        {
             
        }
        */
        /*
        [TestMethod]
        public void TestNewAccountWelcome_AADConnectionFailed()
        {

        }
        */

        [TestMethod]
        public async Task TestNewAccountWelcome_StorageConnectionFailed()
        {
            string fileContent = File.ReadAllText(TestJSONPath);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server_noStorage.HttpClient.PostAsync("/newAccountWelcome", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.ServiceUnavailable, errorsJSON["error"]);
            Assert.AreEqual("NewAccountWelcome FAIL: Storage unavailable.  Email not sent.", errorsJSON["description"]);

        }

    }


    [TestClass]
    public class ChangeEmailNoticeTests
    {
        private static TestServer _server;
        private static TestServer _server_noStorage;
        // private static TestServer _server_noAAD;

        private static StorageManager _storageManager;
        private static StorageManager _storageManagerFake;

        private const string TestJSONPath = "../../sampleJSON/ChangeEmailNoticeSamples/ChangeEmailNotice.json";
        private const string TestJSONPath_InsufficientParameters = "../../sampleJSON/ChangeEmailNoticeSamples/ChangeEmailNotice_MissingNewEmail.json";
        private const string TestJSONPath_ExtraParameters = "../../sampleJSON/ChangeEmailNoticeSamples/ChangeEmailNotice_ExtraParameter.json";
        //private const string TestJSONPath_InvalidParameters = "ChangeEmailNotice_InvalidParams.json";
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
        public async Task TestChangeEmailNotice()
        {
            string fileContent = File.ReadAllText(TestJSONPath);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/changeEmailNotice", postContent);
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



            // Check first message (old)
            StreamStorageContent oldMessageJSON = (StreamStorageContent)await _storageManager.Load("email1");
            StreamReader reader_old = new StreamReader(oldMessageJSON.GetContentStream());
            string oldBodyContent = reader_old.ReadToEnd();
            JObject root_old = JObject.Parse(oldBodyContent);

            Assert.AreEqual("oldEmail@live.com", root_old["to"]);
            Assert.AreEqual("support@nuget.org", root_old["from"]);
            Assert.AreEqual("[NuGet Gallery] Recent changes to your account.", root_old["subject"]);
            Assert.AreEqual(@"Hi there,

The email address associated to your NuGet account was recently 
changed from _oldEmail@live.com_ to _newEmail@live.com_.

Thanks,
The NuGet Team", root_old["body"]["text"]);
            Assert.AreEqual(@"Hi there,

The email address associated to your NuGet account was recently 
changed from _oldEmail@live.com_ to _newEmail@live.com_.

Thanks,
The NuGet Team", root_old["body"]["html"]);




            // check second message (new)
            StreamStorageContent newMessageJSON = (StreamStorageContent)await _storageManager.Load("email2");
            StreamReader reader_new = new StreamReader(newMessageJSON.GetContentStream());
            string newBodyContent = reader_new.ReadToEnd();
            JObject root_new = JObject.Parse(newBodyContent);

            Assert.AreEqual("newEmail@live.com", root_new["to"]);
            Assert.AreEqual("support@nuget.org", root_new["from"]);
            Assert.AreEqual("[NuGet Gallery] Please verify your new email address.", root_new["subject"]);
            Assert.AreEqual(@"You recently changed your NuGet email address. 

To verify your new email address, please click the following link:

/profile/email/verify

Thanks,
The NuGet Team", root_new["body"]["text"]);
            Assert.AreEqual(@"You recently changed your NuGet email address. 

To verify your new email address, please click the following link:

[Verify Email](/profile/email/verify)

Thanks,
The NuGet Team", root_new["body"]["html"]);

        }

        [TestMethod]
        public async Task TestChangeEmailNotice_InsufficientParameters()
        {
            string fileContent = File.ReadAllText(TestJSONPath_InsufficientParameters);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/changeEmailNotice", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, errorsJSON["error"]);
            Assert.AreEqual("ChangeEmailNotice FAIL: Insufficient parameters.", errorsJSON["description"]);
            string expectedMissingParams = (new JArray(new List<string>() { "newEmail" })).ToString();
            string actualMissingParams = ((JArray)errorsJSON["missingParameters"]).ToString();
            Assert.AreEqual(expectedMissingParams, actualMissingParams);

        }

        [TestMethod]
        public async Task TestChangeEmailNotice_ExtraParameters()
        {
            string fileContent = File.ReadAllText(TestJSONPath_ExtraParameters);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/changeEmailNotice", postContent);
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


            // Check first message (old)
            StreamStorageContent oldMessageJSON = (StreamStorageContent)await _storageManager.Load("email1");
            StreamReader reader_old = new StreamReader(oldMessageJSON.GetContentStream());
            string oldBodyContent = reader_old.ReadToEnd();
            JObject root_old = JObject.Parse(oldBodyContent);

            Assert.AreEqual("oldEmail@live.com", root_old["to"]);
            Assert.AreEqual("support@nuget.org", root_old["from"]);
            Assert.AreEqual("[NuGet Gallery] Recent changes to your account.", root_old["subject"]);
            Assert.AreEqual(@"Hi there,

The email address associated to your NuGet account was recently 
changed from _oldEmail@live.com_ to _newEmail@live.com_.

Thanks,
The NuGet Team", root_old["body"]["text"]);
            Assert.AreEqual(@"Hi there,

The email address associated to your NuGet account was recently 
changed from _oldEmail@live.com_ to _newEmail@live.com_.

Thanks,
The NuGet Team", root_old["body"]["html"]);




            // check second message (new)
            StreamStorageContent newMessageJSON = (StreamStorageContent)await _storageManager.Load("email2");
            StreamReader reader_new = new StreamReader(newMessageJSON.GetContentStream());
            string newBodyContent = reader_new.ReadToEnd();
            JObject root_new = JObject.Parse(newBodyContent);

            Assert.AreEqual("newEmail@live.com", root_new["to"]);
            Assert.AreEqual("support@nuget.org", root_new["from"]);
            Assert.AreEqual("[NuGet Gallery] Please verify your new email address.", root_new["subject"]);
            Assert.AreEqual(@"You recently changed your NuGet email address. 

To verify your new email address, please click the following link:

/profile/email/verify

Thanks,
The NuGet Team", root_new["body"]["text"]);
            Assert.AreEqual(@"You recently changed your NuGet email address. 

To verify your new email address, please click the following link:

[Verify Email](/profile/email/verify)

Thanks,
The NuGet Team", root_new["body"]["html"]);

        }

        /*
        [TestMethod]
        public void TestChangeEmailNotice_InvalidParameters()
        {
             
        }
        */
        /*
        [TestMethod]
        public void TestChangeEmailNotice_AADConnectionFailed()
        {

        }
        */

        [TestMethod]
        public async Task TestChangeEmailNotice_StorageConnectionFailed()
        {
            string fileContent = File.ReadAllText(TestJSONPath);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server_noStorage.HttpClient.PostAsync("/changeEmailNotice", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.ServiceUnavailable, errorsJSON["error"]);
            Assert.AreEqual("ChangeEmailNotice FAIL: Storage unavailable.  Emails not sent.", errorsJSON["description"]);

        }

    }


    [TestClass]
    public class ResetPasswordInstructionsTests
    {
        private static TestServer _server;
        private static TestServer _server_noStorage;
        // private static TestServer _server_noAAD;

        private static StorageManager _storageManager;
        private static StorageManager _storageManagerFake;

        private const string TestJSONPath = "../../sampleJSON/ResetPasswordInstructionsSamples/ResetPasswordInstructions.json";
        private const string TestJSONPath_InsufficientParameters = "../../sampleJSON/ResetPasswordInstructionsSamples/ResetPasswordInstructions_MissingAction.json";
        private const string TestJSONPath_ExtraParameters = "../../sampleJSON/ResetPasswordInstructionsSamples/ResetPasswordInstructions_ExtraParameter.json";
        //private const string TestJSONPath_InvalidParameters = "ResetPasswordInstructions_InvalidParams.json";
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
        public async Task TestResetPasswordInstructions()
        {
            string fileContent = File.ReadAllText(TestJSONPath);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/resetPasswordInstructions", postContent);
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

            Assert.AreEqual("someuser@live.com", root["to"]);
            Assert.AreEqual("support@nuget.org", root["from"]);
            Assert.AreEqual("[NuGet Gallery] Please reset your password.", root["subject"]);
            Assert.AreEqual(@"The word on the street is you lost your password. Sorry to hear it!
If you haven't forgotten your password you can safely ignore this email. Your password has not been changed.

Click the following link within the next 5 hours to reset your password:

profile/password/reset

Thanks,
The NuGet Team", root["body"]["text"]);
            Assert.AreEqual(@"The word on the street is you lost your password. Sorry to hear it!
If you haven't forgotten your password you can safely ignore this email. Your password has not been changed.

Click the following link within the next 5 hours to reset your password:

[Reset Password](profile/password/reset)

Thanks,
The NuGet Team", root["body"]["html"]);

        }

        [TestMethod]
        public async Task TestResetPasswordInstructions_InsufficientParameters()
        {
            string fileContent = File.ReadAllText(TestJSONPath_InsufficientParameters);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/resetPasswordInstructions", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, errorsJSON["error"]);
            Assert.AreEqual("ResetPasswordInstructions FAIL: Insufficient parameters.", errorsJSON["description"]);
            string expectedMissingParams = (new JArray(new List<string>() { "action" })).ToString();
            string actualMissingParams = ((JArray)errorsJSON["missingParameters"]).ToString();
            Assert.AreEqual(expectedMissingParams, actualMissingParams);

        }

        [TestMethod]
        public async Task TestResetPasswordInstructions_ExtraParameters()
        {
            string fileContent = File.ReadAllText(TestJSONPath_ExtraParameters);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/resetPasswordInstructions", postContent);
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

            Assert.AreEqual("someuser@live.com", root["to"]);
            Assert.AreEqual("support@nuget.org", root["from"]);
            Assert.AreEqual("[NuGet Gallery] Please reset your password.", root["subject"]);
            Assert.AreEqual(@"The word on the street is you lost your password. Sorry to hear it!
If you haven't forgotten your password you can safely ignore this email. Your password has not been changed.

Click the following link within the next 5 hours to reset your password:

profile/password/reset

Thanks,
The NuGet Team", root["body"]["text"]);
            Assert.AreEqual(@"The word on the street is you lost your password. Sorry to hear it!
If you haven't forgotten your password you can safely ignore this email. Your password has not been changed.

Click the following link within the next 5 hours to reset your password:

[Reset Password](profile/password/reset)

Thanks,
The NuGet Team", root["body"]["html"]);

        }

        /*
        [TestMethod]
        public void TestResetPasswordInstructions_InvalidParameters()
        {
             
        }
        */
        /*
        [TestMethod]
        public void TestResetPasswordInstructions_AADConnectionFailed()
        {

        }
        */

        [TestMethod]
        public async Task TestResetPasswordInstructions_StorageConnectionFailed()
        {
            string fileContent = File.ReadAllText(TestJSONPath);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server_noStorage.HttpClient.PostAsync("/resetPasswordInstructions", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.ServiceUnavailable, errorsJSON["error"]);
            Assert.AreEqual("ResetPasswordInstructions FAIL: Storage unavailable.  Email not sent.", errorsJSON["description"]);

        }

    }


    [TestClass]
    public class EditCredentialTests
    {
        private static TestServer _server;
        private static TestServer _server_noStorage;
        // private static TestServer _server_noAAD;

        private static StorageManager _storageManager;
        private static StorageManager _storageManagerFake;

        private const string TestJSONPath = "../../sampleJSON/EditCredentialSamples/EditCredential.json";
        private const string TestJSONPath_InsufficientParameters = "../../sampleJSON/EditCredentialSamples/EditCredential_MissingUsername.json";
        private const string TestJSONPath_ExtraParameters = "../../sampleJSON/EditCredentialSamples/EditCredential_ExtraParameter.json";
        //private const string TestJSONPath_InvalidParameters = "EditCredential_InvalidParams.json";
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
        public async Task TestEditCredential()
        {
            string fileContent = File.ReadAllText(TestJSONPath);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/editCredential", postContent);
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

            Assert.AreEqual("someuser@live.com", root["to"]);
            Assert.AreEqual("support@nuget.org", root["from"]);
            Assert.AreEqual("[NuGet Gallery] Some credential? added to your account", root["subject"]);
            Assert.AreEqual("A Some credential? was added to your account and can now be used to log in. If you did not request this change, please reply to this email to contact support.", root["body"]["text"]);
            Assert.AreEqual("A Some credential? was added to your account and can now be used to log in. If you did not request this change, please reply to this email to contact support.", root["body"]["html"]);

        }

        [TestMethod]
        public async Task TestEditCredential_InsufficientParameters()
        {
            string fileContent = File.ReadAllText(TestJSONPath_InsufficientParameters);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/editCredential", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, errorsJSON["error"]);
            Assert.AreEqual("EditCredential FAIL: Insufficient parameters.", errorsJSON["description"]);
            string expectedMissingParams = (new JArray(new List<string>() { "username" })).ToString();
            string actualMissingParams = ((JArray)errorsJSON["missingParameters"]).ToString();
            Assert.AreEqual(expectedMissingParams, actualMissingParams);

        }

        [TestMethod]
        public async Task TestEditCredential_ExtraParameters()
        {
            string fileContent = File.ReadAllText(TestJSONPath_ExtraParameters);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/editCredential", postContent);
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

            Assert.AreEqual("someuser@live.com", root["to"]);
            Assert.AreEqual("support@nuget.org", root["from"]);
            Assert.AreEqual("[NuGet Gallery] Some credential? added to your account", root["subject"]);
            Assert.AreEqual("A Some credential? was added to your account and can now be used to log in. If you did not request this change, please reply to this email to contact support.", root["body"]["text"]);
            Assert.AreEqual("A Some credential? was added to your account and can now be used to log in. If you did not request this change, please reply to this email to contact support.", root["body"]["html"]);

        }

        /*
        [TestMethod]
        public void TestEditCredential_InvalidParameters()
        {
             
        }
        */
        /*
        [TestMethod]
        public void TestEditCredential_AADConnectionFailed()
        {

        }
        */

        [TestMethod]
        public async Task TestEditCredential_StorageConnectionFailed()
        {
            string fileContent = File.ReadAllText(TestJSONPath);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server_noStorage.HttpClient.PostAsync("/editCredential", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.ServiceUnavailable, errorsJSON["error"]);
            Assert.AreEqual("EditCredential FAIL: Storage unavailable.  Email not sent.", errorsJSON["description"]);

        }

    }

}
