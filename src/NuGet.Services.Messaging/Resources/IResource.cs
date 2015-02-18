using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

namespace NuGet.Services.Messaging
{

    /*
     * Defines the minimum requirements for a Resource.
     */

    public interface IResource
    {



        Task<string> GetEmail(string username);

        Task<string[]> GetOwnersEmails(string packageID);

        Task<bool> IsContactAllowed(string packageID);

    }
}