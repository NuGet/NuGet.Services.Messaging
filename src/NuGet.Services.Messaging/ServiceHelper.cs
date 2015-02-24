using Microsoft.Owin;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;


namespace NuGet.Services.Messaging
{
    public static class ServiceHelper
    {

        private static string[] _brandsOptions = { "NuGet", "PowerShellGallery" };
        private static string[] _credentialTypes = { "APIKey", "password", "MSAccount" };


        public static string[] BrandsOptions()
        {
            return _brandsOptions;
        }

        public static string[] CredentialTypes()
        {
            return _credentialTypes;
        }



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

        public static Task<string> GetEmail(string username)
        {
            

            // for now, return dummy values
            return Task<string>.Run(() => { return "someuser@live.com"; });
        }

        
        public static Task<List<string>> GetOwnersEmails(string packageID)
        {
            

            // for now, return dummy values
            return Task<List<string>>.Run(() => { return new List<string> { "user1@gmail.com", "user2@gmail.com", "user3@gmail.com" }; });

        }


        public static Task<bool> IsContactAllowed(string packageID)
        {

            // for now, return dummy values
            return Task<bool>.Run(() => { return true; });
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
                        brandValues = null;
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



        public static bool IsValidEmail(string email)
        {
            try
            {
                MailAddress addr = new MailAddress(email);
                return addr.Address.Equals(email);
            }
            catch
            {
                return false;
            }
        }


    }
}