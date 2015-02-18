using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using NuGet.Services.Messaging.Resources;
using System.Threading.Tasks;

namespace NuGet.Services.Messaging
{
    public class ResourceManager
    {
        private IResource _resource;

        // AAD connection info
        private static string _graphResourceId = ConfigurationManager.AppSettings["ida:GraphResourceId"];
        private static string _aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string _clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string _appKey = ConfigurationManager.AppSettings["ida:AppKey"];
        private static string _tenant = ConfigurationManager.AppSettings["ida:Tenant"];


        // SQL connection info


        public ResourceManager(IResource resource = null)
        {
            if (resource != null)
            {
                _resource = resource;
            }
        }

        public ResourceManager(string brand)
        {
            switch (brand)
            {
                case "NuGet":
                    {
                        _resource = new SQLResource();
                        break;
                    }
                    
                case "PowerShellGallery":
                    {
                        _resource = new AADResource();
                        
                        break;
                    }
                default:
                    {
                        // not a valid brand
                        throw new Exception("InvalidBrandException");
                    }
            }
        }


        public async Task<string> GetEmail(string username)
        {
            string email;
            try
            {
                email = await _resource.GetEmail(username);
            }
            catch
            {
                email = null;
            }
            return email;
        }

        public async Task<string[]> GetOwnersEmails(string packageID)
        {
            try
            {
                string[] ownersEmails = await _resource.GetOwnersEmails(packageID);
                return ownersEmails;
            }
            catch
            {
                return null;
            }
        }

        
        public async Task<bool> IsContactAllowed(string packageID)
        {
            try
            {
                return await _resource.IsContactAllowed(packageID);
            }
            catch
            {
                // Default to no contact allowed.  On exceptions, no message is sent.
                // TODO:  find a better way to convey error
                return false;
            }
        }
    }
}