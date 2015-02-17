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
        private const string TestJSONPath_InvalidBrand = "../../sampleJSON/ChangeEmailNoticeSamples/ChangeEmailNotice_InvalidBrand.json";
        
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

The email address associated to your NuGet account was recently changed from oldEmail@live.com to newEmail@live.com.

Thanks,
The NuGet Gallery Team", root_old["body"]["text"]);
            Assert.AreEqual(@"Hi there,

The email address associated to your NuGet account was recently changed from _oldEmail@live.com_ to _newEmail@live.com_.

Thanks,
The NuGet Gallery Team", root_old["body"]["html"]);




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

http://www.nuget.org/profile/email/verify

Thanks,
The NuGet Gallery Team", root_new["body"]["text"]);
            Assert.AreEqual(@"You recently changed your NuGet email address. 

To verify your new email address, please click the following link:

[Verify Email](http://www.nuget.org/profile/email/verify)

Thanks,
The NuGet Gallery Team", root_new["body"]["html"]);

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

The email address associated to your NuGet account was recently changed from oldEmail@live.com to newEmail@live.com.

Thanks,
The NuGet Gallery Team", root_old["body"]["text"]);
            Assert.AreEqual(@"Hi there,

The email address associated to your NuGet account was recently changed from _oldEmail@live.com_ to _newEmail@live.com_.

Thanks,
The NuGet Gallery Team", root_old["body"]["html"]);




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

http://www.nuget.org/profile/email/verify

Thanks,
The NuGet Gallery Team", root_new["body"]["text"]);
            Assert.AreEqual(@"You recently changed your NuGet email address. 

To verify your new email address, please click the following link:

[Verify Email](http://www.nuget.org/profile/email/verify)

Thanks,
The NuGet Gallery Team", root_new["body"]["html"]);

        }

        
        [TestMethod]
        public async Task TestChangeEmailNotice_InvalidBrand()
        {
            string fileContent = File.ReadAllText(TestJSONPath_InvalidBrand);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/changeEmailNotice", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, errorsJSON["error"]);
            Assert.AreEqual("ChangeEmailNotice FAIL: FakeBrand is not a valid brand.  Options:  NuGet", errorsJSON["description"]);

        }
        /*
        [TestMethod]
        public async Task TestChangeEmailNotice_InvalidEmail()
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



}
