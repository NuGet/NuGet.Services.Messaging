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
        private const string TestJSONPath_InvalidBrand = "../../sampleJSON/ResetPasswordInstructionsSamples/ResetPasswordInstructions_InvalidBrand.json";
        private const string TestJSONPath_InvalidAction = "../../sampleJSON/ResetPasswordInstructionsSamples/ResetPasswordInstructions_InvalidAction.json";

        private static string fileStorage_BaseAddress = ConfigurationManager.AppSettings["Storage.Secondary.BaseAddress"];
        private static string fileStorage_Path = "../../Messages/ResetPasswordInstructionsTests";
        private static string fileStorage_Queue = "../../Messages/ResetPasswordInstructionsTests/queue.txt";


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
        public async Task TestResetPasswordInstructions()
        {
            string fileContent = File.ReadAllText(TestJSONPath);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/resetPasswordInstructions", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;


            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);


            // Check message
            string guid = _storageManager.GetLastContentName();
            StorageContent messageContent = await _storageManager.Load(guid);
            using (StreamReader reader = new StreamReader(messageContent.GetContentStream()))
            {
                string content = await reader.ReadToEndAsync();
                JObject root = JObject.Parse(content);

                Assert.AreEqual("someuser@live.com", root["to"]);
                Assert.AreEqual("support@nuget.org", root["from"]);
                Assert.AreEqual("NuGet Gallery: Please reset your password.", root["subject"]);
                Assert.AreEqual(@"The word on the street is you lost your password. Sorry to hear it!
If you haven't forgotten your password you can safely ignore this email. Your password has not been changed.

Click the following link within the next 12 hours to reset your password:

http://www.nuget.org/profile/password/reset

Thanks,
The NuGet Gallery Team", root["body"]["text"]);
                Assert.AreEqual(@"
<html>
<body>
    <p>
        The word on the street is you lost your password. Sorry to hear it!
    </p>
    <p>
        If you haven't forgotten your password you can safely ignore this email. Your password has not been changed.
    </p>
    <p>
        Click the following link within the next 12 hours to reset your password:
    </p>
    <a href='http://www.nuget.org/profile/password/reset'>Reset Password</a>
    <p>
        Thanks,<br>
        The NuGet Gallery Team
    </p>
</body>
</html>", root["body"]["html"]);
            }
            
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


            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);


            // Check message
            string guid = _storageManager.GetLastContentName();
            StorageContent messageContent = await _storageManager.Load(guid);
            using (StreamReader reader = new StreamReader(messageContent.GetContentStream()))
            {
                string content = await reader.ReadToEndAsync();
                JObject root = JObject.Parse(content);

                Assert.AreEqual("someuser@live.com", root["to"]);
                Assert.AreEqual("support@nuget.org", root["from"]);
                Assert.AreEqual("NuGet Gallery: Please reset your password.", root["subject"]);
                Assert.AreEqual(@"The word on the street is you lost your password. Sorry to hear it!
If you haven't forgotten your password you can safely ignore this email. Your password has not been changed.

Click the following link within the next 12 hours to reset your password:

http://www.nuget.org/profile/password/reset

Thanks,
The NuGet Gallery Team", root["body"]["text"]);
                Assert.AreEqual(@"
<html>
<body>
    <p>
        The word on the street is you lost your password. Sorry to hear it!
    </p>
    <p>
        If you haven't forgotten your password you can safely ignore this email. Your password has not been changed.
    </p>
    <p>
        Click the following link within the next 12 hours to reset your password:
    </p>
    <a href='http://www.nuget.org/profile/password/reset'>Reset Password</a>
    <p>
        Thanks,<br>
        The NuGet Gallery Team
    </p>
</body>
</html>", root["body"]["html"]);
            }
            
        }

        
        [TestMethod]
        public async Task TestResetPasswordInstructions_InvalidBrand()
        {
            string fileContent = File.ReadAllText(TestJSONPath_InvalidBrand);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/resetPasswordInstructions", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, errorsJSON["error"]);
            Assert.AreEqual("ResetPasswordInstructions FAIL: FakeBrand is not a valid brand.  Options:  NuGet", errorsJSON["description"]);
            
        }

        [TestMethod]
        public async Task TestResetPasswordInstructions_InvalidAction()
        {
            string fileContent = File.ReadAllText(TestJSONPath_InvalidAction);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/resetPasswordInstructions", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, errorsJSON["error"]);
            Assert.AreEqual("ResetPasswordInstructions FAIL: fakeAction is not a valid action.", errorsJSON["description"]);
            
        }

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



}
