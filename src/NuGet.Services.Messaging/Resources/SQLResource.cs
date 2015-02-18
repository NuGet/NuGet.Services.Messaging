using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;



namespace NuGet.Services.Messaging.Resources
{

    /*
     * Used for all interaction with SQL DB to obtain information about entities and users.
     */
    public class SQLResource : IResource
    {
        // private SQLConnection _connector;


        public SQLResource() { }


        public void Initialize()
        {
            // create the connector using some connection info
        }


        public Task<string> GetEmail(string username)
        {
            throw new NotImplementedException();
        }

        public Task<string[]> GetOwnersEmails(string packageID)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsContactAllowed(string packageID)
        {
            throw new NotImplementedException();
        }


    }
}