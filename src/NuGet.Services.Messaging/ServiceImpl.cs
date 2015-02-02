using Microsoft.Owin;
using System.Net;
using System.Threading.Tasks;

namespace NuGet.Services.Messaging
{
    // authorization looks like this:

    //  Claim scopeClaim = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope");
    //  bool authorized = (scopeClaim != null && scopeClaim.Value == "user_impersonation");

    // and the AAD user id ...

    //  Claim userClaim = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");
    //  string userId = (userClaim != null) ? userClaim.Value : string.Empty;

    public static class ServiceImpl
    {
        public static async Task ContactOwners(IOwinContext context)
        {
            //TODO: add authorization

            //TODO: grab data from context and spooll into folder/storage

            await context.Response.WriteAsync("ContactOwners OK");
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }

        public static async Task ReportAbuse(IOwinContext context)
        {
            //TODO: add authorization

            //TODO: grab data from context and spooll into folder/storage

            await context.Response.WriteAsync("ReportAbuse OK");
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }

        public static async Task ContactSupport(IOwinContext context)
        {
            //TODO: add authorization

            //TODO: grab data from context and spooll into folder/storage

            await context.Response.WriteAsync("ContactSupport OK");
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }
    }
}