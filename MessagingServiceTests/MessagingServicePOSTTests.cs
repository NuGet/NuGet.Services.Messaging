using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace MessagingServiceTests
{
    /// <summary>
    /// Summary description for MessagingServicePOSTTests
    /// </summary>
    [TestClass]
    public class ContactOwnersTests
    {
        private const string TestJSONPath = "ContactOwners.json";
        private const string TestJSONPath_InsufficientParameters = "ContactOwners_InsufficientParams.json";
        private const string TestJSONPath_ExtraParameters = "ContactOwners_ExtraParams.json";
        private const string TestJSONPath_InvalidParameters = "ContactOwners_InvalidParams.json";


        [TestMethod]
        public void TestContactOwners()
        {
            // 1) Send request (containing json parameters)

            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/contactOwners");
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/json";

            // file content as a string
            string postData = File.ReadAllText(TestJSONPath);
            Stream streamWriter = request.GetRequestStream();
            WebResponse response = request.GetResponse();

            // CHECK:  status code is OK
            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.OK);
            



            // Get email JSON from storage
            // parse JSON
            // CHECK:  email to 
            // CHECK:  email from
            // CHECK:  email cc
            // CHECK:  email subject is correct
            // CHECK:  email body is formatted correctly

        }

        [TestMethod]
        public void TestContactOwners_InsufficientParameters()
        {
            // 1) Send request (containing json parameters)

            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/contactOwners");
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/json";

            // file content as a string
            string postData = File.ReadAllText(TestJSONPath_InsufficientParameters);

            //byte[] bytePostData = Encoding.
            Stream streamWriter = request.GetRequestStream();
            //streamWriter.Write(postData, 0, postData.Length);




            WebResponse response = request.GetResponse();

            // CHECK:  status code is not OK (something else)
            Assert.AreNotEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.OK);

        }

        [TestMethod]
        public void TestContactOwners_ExtraParameters()
        {
            

        }

        [TestMethod]
        public void TestContactOwners_InvalidParameters()
        {
            
        }

        [TestMethod]
        public void TestContactOwners_AADConnectionFailed()
        {
            // Generate AAD connection failure (change connection strings so connection cannot be made?)

            // Check status is ServiceUnavailable

            //HttpStatusCode.ServiceUnavailable
        }

        [TestMethod]
        public void TestContactOwners_StorageConnectionFailed()
        {
            // Generate Storage connection failure (change connection strings so connection cannot be made?)

            // Check status is ServiceUnavailable

            //HttpStatusCode.ServiceUnavailable
        }

    }


    [TestClass]
    public class ReportAbuseTests
    {
        private const string TestJSONPath = "ReportAbuse.json";
        private const string TestJSONPath_InsufficientParameters = "ReportAbuse_InsufficientParams.json";
        private const string TestJSONPath_ExtraParameters = "ReportAbuse_ExtraParams.json";
        private const string TestJSONPath_InvalidParameters = "ReportAbuse_InvalidParams.json";


        [TestMethod]
        public void TestReportAbuse()
        {
            // 1) Send request (containing json parameters)

            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/ReportAbuse");
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/json";

            // file content as a string
            string postData = File.ReadAllText(TestJSONPath);

            //byte[] bytePostData = Encoding.
            Stream streamWriter = request.GetRequestStream();
            //streamWriter.Write(postData, 0, postData.Length);




            WebResponse response = request.GetResponse();

            // CHECK:  status code is OK
            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.OK);




            // Get email blob from storage

            // CHECK:  email to 
            // CHECK:  email from
            // CHECK:  email subject is correct
            // CHECK:  email body is formatted correctly

        }

        [TestMethod]
        public void TestReportAbuse_InsufficientParameters()
        {
            // 1) Send request (containing json parameters)

            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/ReportAbuse");
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/json";

            // file content as a string
            string postData = File.ReadAllText(TestJSONPath_InsufficientParameters);

            //byte[] bytePostData = Encoding.
            Stream streamWriter = request.GetRequestStream();
            //streamWriter.Write(postData, 0, postData.Length);




            WebResponse response = request.GetResponse();

            // CHECK:  status code is OK
            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.OK);




            // Get email blob from storage

            // CHECK:  email to 
            // CHECK:  email from
            // CHECK:  email subject is correct
            // CHECK:  email body is formatted correctly

        }

        [TestMethod]
        public void TestReportAbuse_ExtraParameters()
        {
            // 1) Send request (containing json parameters)

            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/ReportAbuse");
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/json";

            // file content as a string
            string postData = File.ReadAllText(TestJSONPath_ExtraParameters);

            //byte[] bytePostData = Encoding.
            Stream streamWriter = request.GetRequestStream();
            //streamWriter.Write(postData, 0, postData.Length);




            WebResponse response = request.GetResponse();

            // CHECK:  status code is OK
            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.OK);




            // Get email blob from storage

            // CHECK:  email to 
            // CHECK:  email from
            // CHECK:  email subject is correct
            // CHECK:  email body is formatted correctly

        }

        [TestMethod]
        public void TestReportAbuse_InvalidParameters()
        {
            // 1) Send request (containing json parameters)

            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/ReportAbuse");
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/json";

            // file content as a string
            string postData = File.ReadAllText(TestJSONPath_InvalidParameters);

            //byte[] bytePostData = Encoding.
            Stream streamWriter = request.GetRequestStream();
            //streamWriter.Write(postData, 0, postData.Length);




            WebResponse response = request.GetResponse();

            // CHECK:  status code is OK
            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.OK);




            // Get email blob from storage

            // CHECK:  email to 
            // CHECK:  email from
            // CHECK:  email subject is correct
            // CHECK:  email body is formatted correctly

        }

        [TestMethod]
        public void TestReportAbuse_AADConnectionFailed()
        {

        }

        [TestMethod]
        public void TestReportAbuse_StorageConnectionFailed()
        {

        }


    }


    [TestClass]
    public class ContactSupportTests
    {
        private const string TestJSONPath = "ContactSupport.json";
        private const string TestJSONPath_InsufficientParameters = "ContactSupport_InsufficientParams.json";
        private const string TestJSONPath_ExtraParameters = "ContactSupport_ExtraParams.json";
        private const string TestJSONPath_InvalidParameters = "ContactSupport_InvalidParams.json";


        [TestMethod]
        public void TestContactSupport()
        {
            // 1) Send request (containing json parameters)

            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/ContactSupport");
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/json";

            // file content as a string
            string postData = File.ReadAllText(TestJSONPath);

            //byte[] bytePostData = Encoding.
            Stream streamWriter = request.GetRequestStream();
            //streamWriter.Write(postData, 0, postData.Length);




            WebResponse response = request.GetResponse();

            // CHECK:  status code is OK
            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.OK);




            // Get email blob from storage

            // CHECK:  email to 
            // CHECK:  email from
            // CHECK:  email subject is correct
            // CHECK:  email body is formatted correctly

        }

        [TestMethod]
        public void TestContactSupport_InsufficientParameters()
        {
            // 1) Send request (containing json parameters)

            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/ContactSupport");
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/json";

            // file content as a string
            string postData = File.ReadAllText(TestJSONPath_InsufficientParameters);

            //byte[] bytePostData = Encoding.
            Stream streamWriter = request.GetRequestStream();
            //streamWriter.Write(postData, 0, postData.Length);




            WebResponse response = request.GetResponse();

            // CHECK:  status code is OK
            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.OK);




            // Get email blob from storage

            // CHECK:  email to 
            // CHECK:  email from
            // CHECK:  email subject is correct
            // CHECK:  email body is formatted correctly

        }

        [TestMethod]
        public void TestContactSupport_ExtraParameters()
        {
            // 1) Send request (containing json parameters)

            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/ContactSupport");
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/json";

            // file content as a string
            string postData = File.ReadAllText(TestJSONPath_ExtraParameters);

            //byte[] bytePostData = Encoding.
            Stream streamWriter = request.GetRequestStream();
            //streamWriter.Write(postData, 0, postData.Length);




            WebResponse response = request.GetResponse();

            // CHECK:  status code is OK
            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.OK);




            // Get email blob from storage

            // CHECK:  email to 
            // CHECK:  email from
            // CHECK:  email subject is correct
            // CHECK:  email body is formatted correctly

        }

        [TestMethod]
        public void TestContactSupport_InvalidParameters()
        {
            // 1) Send request (containing json parameters)

            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/ContactSupport");
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/json";

            // file content as a string
            string postData = File.ReadAllText(TestJSONPath_InvalidParameters);

            //byte[] bytePostData = Encoding.
            Stream streamWriter = request.GetRequestStream();
            //streamWriter.Write(postData, 0, postData.Length);




            WebResponse response = request.GetResponse();

            // CHECK:  status code is OK
            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.OK);




            // Get email blob from storage

            // CHECK:  email to 
            // CHECK:  email from
            // CHECK:  email subject is correct
            // CHECK:  email body is formatted correctly

        }

        [TestMethod]
        public void TestContactSupport_AADConnectionFailed()
        {

        }

        [TestMethod]
        public void TestContactSupport_StorageConnectionFailed()
        {

        }
    }


    [TestClass]
    public class InvitePackageOwnerTests
    {
        private const string TestJSONPath = "InvitePackageOwner.json";
        private const string TestJSONPath_InsufficientParameters = "InvitePackageOwner_InsufficientParams.json";
        private const string TestJSONPath_ExtraParameters = "InvitePackageOwner_ExtraParams.json";
        private const string TestJSONPath_InvalidParameters = "InvitePackageOwner_InvalidParams.json";


        [TestMethod]
        public void TestInvitePackageOwner()
        {
            // 1) Send request (containing json parameters)

            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/InvitePackageOwner");
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/json";

            // file content as a string
            string postData = File.ReadAllText(TestJSONPath);

            //byte[] bytePostData = Encoding.
            Stream streamWriter = request.GetRequestStream();
            //streamWriter.Write(postData, 0, postData.Length);




            WebResponse response = request.GetResponse();

            // CHECK:  status code is OK
            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.OK);




            // Get email blob from storage

            // CHECK:  email to 
            // CHECK:  email from
            // CHECK:  email subject is correct
            // CHECK:  email body is formatted correctly

        }

        [TestMethod]
        public void TestInvitePackageOwner_InsufficientParameters()
        {
            // 1) Send request (containing json parameters)

            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/InvitePackageOwner");
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/json";

            // file content as a string
            string postData = File.ReadAllText(TestJSONPath_InsufficientParameters);

            //byte[] bytePostData = Encoding.
            Stream streamWriter = request.GetRequestStream();
            //streamWriter.Write(postData, 0, postData.Length);




            WebResponse response = request.GetResponse();

            // CHECK:  status code is OK
            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.OK);




            // Get email blob from storage

            // CHECK:  email to 
            // CHECK:  email from
            // CHECK:  email subject is correct
            // CHECK:  email body is formatted correctly

        }

        [TestMethod]
        public void TestInvitePackageOwner_ExtraParameters()
        {
            // 1) Send request (containing json parameters)

            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/InvitePackageOwner");
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/json";

            // file content as a string
            string postData = File.ReadAllText(TestJSONPath_ExtraParameters);

            //byte[] bytePostData = Encoding.
            Stream streamWriter = request.GetRequestStream();
            //streamWriter.Write(postData, 0, postData.Length);




            WebResponse response = request.GetResponse();

            // CHECK:  status code is OK
            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.OK);




            // Get email blob from storage

            // CHECK:  email to 
            // CHECK:  email from
            // CHECK:  email subject is correct
            // CHECK:  email body is formatted correctly

        }

        [TestMethod]
        public void TestInvitePackageOwner_InvalidParameters()
        {
            // 1) Send request (containing json parameters)

            WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:11717/InvitePackageOwner");
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/json";

            // file content as a string
            string postData = File.ReadAllText(TestJSONPath_InvalidParameters);

            //byte[] bytePostData = Encoding.
            Stream streamWriter = request.GetRequestStream();
            //streamWriter.Write(postData, 0, postData.Length);




            WebResponse response = request.GetResponse();

            // CHECK:  status code is OK
            Assert.AreEqual(((HttpWebResponse)response).StatusCode, HttpStatusCode.OK);




            // Get email blob from storage

            // CHECK:  email to 
            // CHECK:  email from
            // CHECK:  email subject is correct
            // CHECK:  email body is formatted correctly

        }

        [TestMethod]
        public void TestInvitePackageOwner_AADConnectionFailed()
        {

        }

        [TestMethod]
        public void TestInvitePackageOwner_StorageConnectionFailed()
        {

        }
    }



}
