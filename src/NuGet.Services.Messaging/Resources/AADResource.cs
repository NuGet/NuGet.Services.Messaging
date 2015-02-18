using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace NuGet.Services.Messaging
{

    /*
     * Used for all interaction with AAD to obtain information about entities or users.
     */
    public class AADResource : IResource
    {

        private ActiveDirectoryClient _client;

        public AADResource() { }


        public async Task Initialize(string aadInstance, string tenant, string clientId, string appKey, string graphResourceId) 
        {
            // Taken from NuGet.Services.Publish.ServiceHelpers.cs

            string authority = string.Format(aadInstance, tenant);
            AuthenticationContext authContext = new AuthenticationContext(authority);

            ClientCredential clientCredential = new ClientCredential(clientId, appKey);
            AuthenticationResult result = await authContext.AcquireTokenAsync(graphResourceId, clientCredential);

            string accessToken = result.AccessToken;

            Uri serviceRoot = new Uri(new Uri(graphResourceId), tenant);

            _client = new ActiveDirectoryClient(serviceRoot, () => { return Task.FromResult(accessToken); });

        }



        public Task<string> GetEmail(string username)
        {
            
            //IUser user = await _client.Users.GetByObjectId(username).ExecuteAsync();
            //string emailAddress = user.Mail;  // assuming email is stored in mail
            //return emailAddress;
            
            throw new NotImplementedException();
        }

        public Task<string[]> GetOwnersEmails(string packageID)
        {
            // get owners

            // for each owner, get email address
            
            throw new NotImplementedException();
        }

        public Task<bool> IsContactAllowed(string packageID)
        {


            throw new NotImplementedException();
        }


    }
}