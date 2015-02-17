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
        private const string TestJSONPath_InvalidBrand = "../../sampleJSON/ContactOwnersSamples/ContactOwners_InvalidBrand.json";
        
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
            Assert.AreEqual("support@powershellgallery.com", root["from"]);
            Assert.AreEqual("someuser@live.com", root["cc"]);
            Assert.AreEqual("[PowerShell Gallery] Message for owners of the module 'SomeTestPackage'", root["subject"]);
            Assert.AreEqual(@"User rebro-1 &lt;someuser@live.com&gt; sends the following message to the owners of module 'SomeTestPackage':
            
            Hello owners, I would like to be an owner too.  Please add me!

        To stop receiving contact emails as an owner of this module, sign in to the PowerShell Gallery and change your email notification settings: http://www.powershellgallery.com/profile/notifications.", root["body"]["text"]);
            Assert.AreEqual(@"User rebro-1 &lt;someuser@live.com&gt; sends the following message to the owners of module 'SomeTestPackage':
            
            Hello owners, I would like to be an owner too.  Please add me!

    -----------------------------------------------
    <em>
    To stop receiving contact emails as an owner of this module, sign in to the PowerShell Gallery and [change your email notification settings](http://www.powershellgallery.com/profile/notifications).
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
            Assert.AreEqual("support@powershellgallery.com", root["from"]);
            Assert.AreEqual("someuser@live.com", root["cc"]);
            Assert.AreEqual("[PowerShell Gallery] Message for owners of the module 'SomeTestPackage'", root["subject"]);
            Assert.AreEqual(@"User rebro-1 &lt;someuser@live.com&gt; sends the following message to the owners of module 'SomeTestPackage':
            
            Hello owners, I would like to be an owner too.  Please add me!

        To stop receiving contact emails as an owner of this module, sign in to the PowerShell Gallery and change your email notification settings: http://www.powershellgallery.com/profile/notifications.", root["body"]["text"]);
            Assert.AreEqual(@"User rebro-1 &lt;someuser@live.com&gt; sends the following message to the owners of module 'SomeTestPackage':
            
            Hello owners, I would like to be an owner too.  Please add me!

    -----------------------------------------------
    <em>
    To stop receiving contact emails as an owner of this module, sign in to the PowerShell Gallery and [change your email notification settings](http://www.powershellgallery.com/profile/notifications).
    </em>", root["body"]["html"]);

        }


        
        [TestMethod]
        public async Task TestContactOwners_InvalidBrand()
        {
            string fileContent = File.ReadAllText(TestJSONPath_InvalidBrand);
            StringContent postContent = new StringContent(fileContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _server.HttpClient.PostAsync("/contactOwners", postContent);
            Stream errors = response.Content.ReadAsStreamAsync().Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            StreamReader errorsReader = new StreamReader(errors);
            string errorsString = errorsReader.ReadToEnd();
            JObject errorsJSON = JObject.Parse(errorsString);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, errorsJSON["error"]);
            Assert.AreEqual("ContactOwners FAIL: FakeBrand is not a valid brand.  Options:  NuGet, PowerShellGallery", errorsJSON["description"]);
            
        }
        
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


 


}
