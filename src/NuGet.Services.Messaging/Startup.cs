using Microsoft.Owin;
using Microsoft.Owin.Security.ActiveDirectory;
using Owin;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(NuGet.Services.Messaging.Startup))]

namespace NuGet.Services.Messaging
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseErrorPage();

            app.UseWindowsAzureActiveDirectoryBearerAuthentication(
                new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                {
                    Audience = ConfigurationManager.AppSettings["ida:Audience"],
                    Tenant = ConfigurationManager.AppSettings["ida:Tenant"]
                });

            app.Run(Invoke);
        }

        async Task Invoke(IOwinContext context)
        {
            switch (context.Request.Method)
            {
                case "GET":
                    await InvokeGET(context);
                    break;
                case "POST":
                    await InvokePOST(context);
                    break;
                default:
                    await context.Response.WriteAsync("NotFound");
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
            }
        }

        async Task InvokeGET(IOwinContext context)
        {
            switch (context.Request.Path.Value)
            {
                case "/":
                    {
                        await context.Response.WriteAsync("OK");
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        break;
                    }
                case "/reasons/contactSupport/NuGet":
                    { 
                        await ServiceImpl.ContactSupportReasons(context, "NuGet");
                        break;
                    }
                case "/reasons/contactSupport/PowerShellGallery":
                    {
                        await ServiceImpl.ContactSupportReasons(context, "PowerShellGallery");
                        break;
                    }
                case "/reasons/reportAbuse/NuGet":
                    { 
                        await ServiceImpl.ReportAbuseReasons(context, "NuGet");
                        break;
                    }
                case "/reasons/reportAbuse/PowerShellGallery":
                    {
                        await ServiceImpl.ReportAbuseReasons(context, "PowerShellGallery");
                        break;
                    }
                default:
                    {
                        await context.Response.WriteAsync("NotFound");
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    }
            }
        }

        async Task InvokePOST(IOwinContext context)
        {
            switch (context.Request.Path.Value)
            {
                case "/contactOwners":
                    {
                        await ServiceImpl.ContactOwners(context);
                        break;
                    }
                case "/reportAbuse":
                    {
                        await ServiceImpl.ReportAbuse(context);
                        break;
                    }
                case "/contactSupport":
                    {
                        await ServiceImpl.ContactSupport(context);
                        break;
                    }

                case "/invitePackageOwner":
                    {
                        await ServiceImpl.InvitePackageOwner(context);
                        break;
                    }
                default:
                    {
                        await context.Response.WriteAsync("NotFound");
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    }
            }
        }
    }
}