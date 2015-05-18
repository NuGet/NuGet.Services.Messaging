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
using System.Configuration;

namespace NuGet.Services.Messaging.Tests
{

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
        private const string TestJSONPath_InvalidBrand = "../../sampleJSON/ReportAbuseSamples/ReportAbuse_InvalidBrand.json";
        private const string TestJSONPath_InvalidOwnersContacted = "../../sampleJSON/ReportAbuseSamples/ReportAbuse_InvalidOwnersContacted.json";
        private const string TestJSONPath_InvalidFromAddress = "../../sampleJSON/ReportAbuseSamples/ReportAbuse_InvalidEmail.json";

        private static string fileStorage_BaseAddress = ConfigurationManager.AppSettings["Storage.Secondary.BaseAddress"];
        private static string fileStorage_Path = "../../Messages/ReportAbuseTests";
        private static string fileStorage_Queue = "../../Messages/ReportAbuseTests/queue.txt";


        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            // ensure uses file storage
            _storageManager = new StorageManager(new FileStorage(fileStorage_BaseAddress, fileStorage_Path), "file", fileStorage_Queue);
            _server = TestServer.Create(app =>
            {
                var startup = new Startup();
                startup.ConfigureStorageManager(_storageManager);
                startup.Configuration(app);
            });

            // Inject connection failure to storage:  use fake storage.  Allows creation, but fails on save.
            _storageManagerFake = new StorageManager(new FakeFileStorage(fileStorage_BaseAddress, fileStorage_Path), "fake");
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
        public static async void ClassClean()
        {
            _server.Dispose();
            _server_noStorage.Dispose();

            await _storageManager.DeleteAll();
        }

        [TestCleanup]
        public void Cleanup()
        {
            //await _storageManager.DeleteAll();
        }



        [TestMethod]
        public async Task TestReportAbuse()
        {
            string fileContent = File.ReadAllText(TestJSONPath);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/reportAbuse", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;


            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);


            // Check message
            string guid = _storageManager.GetLastContentName();
            StorageContent messageContent = await _storageManager.Load(guid);
            using (StreamReader reader = new StreamReader(messageContent.GetContentStream()))
            {
                string content = await reader.ReadToEndAsync();
                JObject root = JObject.Parse(content);

                Assert.AreEqual("support@powershellgallery.com", root["to"]);
                Assert.AreEqual("someuser@live.com", root["from"]);
                Assert.AreEqual("someuser@live.com", root["cc"]);
                Assert.AreEqual("PowerShell Gallery: Support Request for 'SomeTestPackage' version 1.0.0 (Reason: This package has a bug in it.)", root["subject"]);
                Assert.AreEqual(@"
Email: 
rebro-1 <someuser@live.com>

Module: 
SomeTestPackage:  http://www.powershellgallery.com/modules/SomeTestPackage

Version: 
1.0.0:  http://www.powershellgallery.com/modules/SomeTestPackage/1.0.0

Reason:
This package has a bug in it.

Has the package owner been contacted?:
Yes

Message:
Please remove this package right away, it is terrible!", root["body"]["text"]);
                Assert.AreEqual(@"
<html>
<body>
    <div>
        <h3>Email:</h3>
        <p>
            rebro-1 &lt;someuser@live.com&gt;
        </p>
    </div>
    <div>
        <h3>Module:</h3>
        <p>
            <a href='http://www.powershellgallery.com/modules/SomeTestPackage'>SomeTestPackage</a>
        </p>
    </div>
    <div>
        <h3>Version:</h3>
        <a href='http://www.powershellgallery.com/modules/SomeTestPackage/1.0.0'>1.0.0</a>
    </div>
    <div>
        <h3>Reason:</h3>
        <p>This package has a bug in it.</p>
    </div>
    <div>
        <h3>Has the module owner been contacted?:</h3>
        <p>Yes</p>
    </div>
    <div>
        <h3>Message:</h3>
        <p>Please remove this package right away, it is terrible!</p>
    </div>
</body>
</html>", root["body"]["html"]);
            }
            
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


            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);


            // Check message
            string guid = _storageManager.GetLastContentName();
            StorageContent messageContent = await _storageManager.Load(guid);
            using (StreamReader reader = new StreamReader(messageContent.GetContentStream()))
            {
                string content = await reader.ReadToEndAsync();
                JObject root = JObject.Parse(content);

                Assert.AreEqual("support@powershellgallery.com", root["to"]);
                Assert.AreEqual("someuser@live.com", root["from"]);
                Assert.AreEqual("someuser@live.com", root["cc"]);
                Assert.AreEqual("PowerShell Gallery: Support Request for 'SomeTestPackage' version 1.0.0 (Reason: This package has a bug in it.)", root["subject"]);
                Assert.AreEqual(@"
Email: 
rebro-1 <someuser@live.com>

Module: 
SomeTestPackage:  http://www.powershellgallery.com/modules/SomeTestPackage

Version: 
1.0.0:  http://www.powershellgallery.com/modules/SomeTestPackage/1.0.0

Reason:
This package has a bug in it.

Has the package owner been contacted?:
Yes

Message:
Please remove this package right away, it is terrible!", root["body"]["text"]);
                Assert.AreEqual(@"
<html>
<body>
    <div>
        <h3>Email:</h3>
        <p>
            rebro-1 &lt;someuser@live.com&gt;
        </p>
    </div>
    <div>
        <h3>Module:</h3>
        <p>
            <a href='http://www.powershellgallery.com/modules/SomeTestPackage'>SomeTestPackage</a>
        </p>
    </div>
    <div>
        <h3>Version:</h3>
        <a href='http://www.powershellgallery.com/modules/SomeTestPackage/1.0.0'>1.0.0</a>
    </div>
    <div>
        <h3>Reason:</h3>
        <p>This package has a bug in it.</p>
    </div>
    <div>
        <h3>Has the module owner been contacted?:</h3>
        <p>Yes</p>
    </div>
    <div>
        <h3>Message:</h3>
        <p>Please remove this package right away, it is terrible!</p>
    </div>
</body>
</html>", root["body"]["html"]);
            }
            
        }

        
        [TestMethod]
        public async Task TestReportAbuse_InvalidBrand()
        {
            string fileContent = File.ReadAllText(TestJSONPath_InvalidBrand);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/reportAbuse", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, errorsJSON["error"]);
            Assert.AreEqual("ReportAbuse FAIL: FakeBrand is not a valid brand.  Options:  NuGet, PowerShellGallery", errorsJSON["description"]);
            
        }

        [TestMethod]
        public async Task TestReportAbuse_InvalidOwnersContacted()
        {
            string fileContent = File.ReadAllText(TestJSONPath_InvalidOwnersContacted);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/reportAbuse", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, errorsJSON["error"]);
            Assert.AreEqual("ReportAbuse FAIL: Try ContactOwners first.", errorsJSON["description"]);
            
        }

        
        [TestMethod]
        public async Task TestReportAbuse_InvalidFromAddress()
        {
            string fileContent = File.ReadAllText(TestJSONPath_InvalidFromAddress);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/reportAbuse", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, errorsJSON["error"]);
            Assert.AreEqual("ReportAbuse FAIL: not_an_email_address is not a valid email address.", errorsJSON["description"]);
            
        }
        
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
    



}
