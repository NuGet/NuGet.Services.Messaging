using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;


namespace NuGet.Services.Messaging
{
    public static class ServiceHelper
    {

        static string graphResourceId = ConfigurationManager.AppSettings["ida:GraphResourceId"];
        static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        static string appKey = ConfigurationManager.AppSettings["ida:AppKey"];
        static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];


        // needed??
        public static async Task Test(IOwinContext context)
        {
            
            // and the AAD user id ...

            //  Claim userClaim = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");
            //  string userId = (userClaim != null) ? userClaim.Value : string.Empty;
            

            // The Scope claim tells you what permissions the client application has in the service.
            // In this case we look for a scope value of user_impersonation, or full access to the service as the user.
            
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
            // Taken from NuGet.Services.Publish.ServiceHelpers.cs
            
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
            //ActiveDirectoryClient activeDirectoryClient = await GetActiveDirectoryClient();
            //IUser user = await activeDirectoryClient.Users.GetByObjectId(username).ExecuteAsync();
            //string emailAddress = user.Mail;  // assuming email is stored in mail
            //return emailAddress;


            // for now, return dummy values
            return "someuser@live.com";
        }

        
        public static async Task<List<string>> GetOwnerEmailAddressesFromPackageID(string packageID)
        {
            //ActiveDirectoryClient activeDirectoryClient = await GetActiveDirectoryClient();
            //IGroup package = await activeDirectoryClient.Groups.GetByObjectId(packageID).ExecuteAsync();
            //IPagedCollection<IDirectoryObject> owners = package.Owners;

            //owners.CurrentPage
            
            //List<string> ownerEmails = new List<string>();
            // for each owner, call GetUserEmailAddressFromUsername(ownerUsername)
            // store each email in a list
            


            // for now, return dummy values
            return new List<string> { "user1@gmail.com", "user2@gmail.com", "user3@gmail.com" };

        }


        public static IConstants GetBrandConstants(string brand)
        {
            IConstants brandValues;
            switch (brand)
            {
                case "NuGet":
                    {
                        brandValues = new NuGet.Services.Messaging.Brand.NuGet.Constants();
                        break;
                    }
                case "PowerShellGallery":
                    {
                        brandValues = new NuGet.Services.Messaging.Brand.PowerShellGallery.Constants();
                        break;
                    }
                default:
                    {
                        brandValues = new NuGet.Services.Messaging.Brand.NuGet.Constants();
                        break;
                    }
            }
            return brandValues;
        }


        public static String[] GetReasonsList(string action)
        {
            switch (action)
            {
                case "contactSupport":
                    {
                        String[] reasons = {
                            "The {0} contains private/confidential data",
                            "The {0} was published as the wrong version",
                            "The {0} was not intended to be published publically on this gallery",
                            "The {0} contains malicious code",
                            "Other" };
                        return reasons;
                    }
                case "reportAbuse":
                    {
                        String[] reasons = {
                            "The {0} owner is fraudulently claiming authorship",
                            "The {0} violates a license I own",
                            "The {0} contains malicious code",
                            "The {0} has a bug/failed to install",
                            "Other" };
                        return reasons;
                    }
                default:
                    {
                        String[] reasons = { "Other" };
                        return reasons;
                    }
            }
            
        }


        public static List<string> VerifyRequiredParameters(JObject root, String[] requiredParams)
        {
            List<string> missingParams = new List<string>();

            for (int i = 0; i < requiredParams.Length; i++)
            {
                if (root[requiredParams[i]] == null)
                {
                    missingParams.Add(requiredParams[i]);
                }
            }
            return missingParams;
        }


    }
}