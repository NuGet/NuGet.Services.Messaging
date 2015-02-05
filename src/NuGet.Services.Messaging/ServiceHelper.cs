using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

using Microsoft.Owin;
//using Microsoft.WindowsAzure.Storage;
//using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;


namespace NuGet.Services.Messaging
{
    public static class ServiceHelper
    {

        static string graphResourceId = ConfigurationManager.AppSettings["ida:GraphResourceId"];
        static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        static string appKey = ConfigurationManager.AppSettings["ida:AppKey"];
        static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];



        public static async Task Test(IOwinContext context)
        {
            //
            // The Scope claim tells you what permissions the client application has in the service.
            // In this case we look for a scope value of user_impersonation, or full access to the service as the user.
            //
            Claim scopeClaim = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope");
            bool authorized = (scopeClaim != null && scopeClaim.Value == "user_impersonation");

            if (authorized)
            {
                Claim claim = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier);
                string msg = string.Format("OK claim.Subject.Name = {0} Value = {1}", claim.Subject.Name, claim.Value);
                await context.Response.WriteAsync(msg);
                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                await context.Response.WriteAsync("The Scope claim does not contain 'user_impersonation' or scope claim not found");
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
        }



        
        public static async Task<ActiveDirectoryClient> GetActiveDirectoryClient()
        {
            string authority = string.Format(aadInstance, tenant);

            AuthenticationContext authContext = new AuthenticationContext(authority);

            ClientCredential clientCredential = new ClientCredential(clientId, appKey);
            AuthenticationResult result = await authContext.AcquireTokenAsync(graphResourceId, clientCredential);

            string accessToken = result.AccessToken;

            Uri serviceRoot = new Uri(new Uri(graphResourceId), tenant);

            ActiveDirectoryClient activeDirectoryClient = new ActiveDirectoryClient(serviceRoot, () => { return Task.FromResult(accessToken); });

            return activeDirectoryClient;
        }




        public static async Task<string> GetUserEmailAddressFromUsername(string username)
        {
            ActiveDirectoryClient activeDirectoryClient = await GetActiveDirectoryClient();
            IUser user = await activeDirectoryClient.Users.GetByObjectId(username).ExecuteAsync();
            string emailAddress = user.Mail;
            return emailAddress;
        }



        
        public static async Task<List<string>> GetOwnerEmailAddressesFromPackageID(string packageID)
        {
            ActiveDirectoryClient activeDirectoryClient = await GetActiveDirectoryClient();

            IGroup package = await activeDirectoryClient.Groups.GetByObjectId(packageID).ExecuteAsync();


            // TODO:  Pull owners using module name


            //IUserCollection owners = activeDirectoryClient.Users.Where(usr => usr.MemberOf(package.Owners) );
            //package.Owners;

            return new List<string> { "user1@gmail.com", "user2@gmail.com", "user3@gmail.com" };

        }




        /*
        public static async Task<Uri> SaveEmail(Stream email, string name)
        {
            string storagePrimary = System.Configuration.ConfigurationManager.AppSettings.Get("Storage.Primary");
            CloudStorageAccount account = CloudStorageAccount.Parse(storagePrimary);

            CloudBlobClient client = account.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("emails");
            await container.CreateIfNotExistsAsync();

            CloudBlockBlob blob = container.GetBlockBlobReference(name);
            blob.Properties.ContentType = "application/octet-stream";  // email/mailmessage?
            blob.Properties.ContentDisposition = name;
            email.Seek(0, SeekOrigin.Begin);
            await blob.UploadFromStreamAsync(email);

            return blob.Uri;
        }
        */







    }
}