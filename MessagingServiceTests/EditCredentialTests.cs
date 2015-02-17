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
        private const string TestJSONPath_InvalidBrand = "../../sampleJSON/EditCredentialSamples/EditCredential_InvalidBrand.json";
        private const string TestJSONPath_InvalidAction = "../../sampleJSON/EditCredentialSamples/EditCredential_InvalidAction.json";
        private const string TestJSONPath_InvalidType = "../../sampleJSON/EditCredentialSamples/EditCredential_InvalidType.json";

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
            Assert.AreEqual("[NuGet Gallery] MSAccount added to your account", root["subject"]);
            Assert.AreEqual("A MSAccount was added to your account and can now be used to log in. If you did not request this change, please reply to this email to contact support.", root["body"]["text"]);
            Assert.AreEqual("A MSAccount was added to your account and can now be used to log in. If you did not request this change, please reply to this email to contact support.", root["body"]["html"]);

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
            Assert.AreEqual("[NuGet Gallery] MSAccount added to your account", root["subject"]);
            Assert.AreEqual("A MSAccount was added to your account and can now be used to log in. If you did not request this change, please reply to this email to contact support.", root["body"]["text"]);
            Assert.AreEqual("A MSAccount was added to your account and can now be used to log in. If you did not request this change, please reply to this email to contact support.", root["body"]["html"]);

        }

        
        [TestMethod]
        public async Task TestEditCredential_InvalidBrand()
        {
            string fileContent = File.ReadAllText(TestJSONPath_InvalidBrand);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/editCredential", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, errorsJSON["error"]);
            Assert.AreEqual("EditCredential FAIL: FakeBrand is not a valid brand.  Options:  NuGet", errorsJSON["description"]);
            
        }

        [TestMethod]
        public async Task TestEditCredential_InvalidAction()
        {
            string fileContent = File.ReadAllText(TestJSONPath_InvalidAction);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/editCredential", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, errorsJSON["error"]);
            Assert.AreEqual("EditCredential FAIL: get is not a valid action.  Options:  add, remove", errorsJSON["description"]);
            
            
        }

        [TestMethod]
        public async Task TestEditCredential_InvalidType()
        {
            string fileContent = File.ReadAllText(TestJSONPath_InvalidType);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/editCredential", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, errorsJSON["error"]);
            Assert.AreEqual("EditCredential FAIL: account is not a valid type.  Options:  APIKey, password, MSAccount", errorsJSON["description"]);
            
            
        }


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
