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
        StorageManager _storageManager;

        public void Configuration(IAppBuilder app)
        {
            app.UseErrorPage();

            if (_storageManager == null)
            {
                _storageManager = new StorageManager();
            }
            
            app.UseWindowsAzureActiveDirectoryBearerAuthentication(
                new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                {
                    Audience = ConfigurationManager.AppSettings["ida:Audience"],
                    Tenant = ConfigurationManager.AppSettings["ida:Tenant"]
                });

            app.Run(Invoke);
        }


        // used to inject faults in storage, for unit testing
        public void ConfigureStorageManager(StorageManager sm)
        {
            _storageManager = sm;
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
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.WriteAsync("NotFound");
                    break;
            }
        }

        async Task InvokeGET(IOwinContext context)
        {
            switch (context.Request.Path.Value)
            {
                case "/":
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        await context.Response.WriteAsync("OK");
                        break;
                    }
                case "/reasons/contactSupport/NuGet":
                    { 
                        await ServiceImpl.GetReasons(context, "NuGet", "contactSupport");
                        break;
                    }
                case "/reasons/contactSupport/PowerShellGallery":
                    {
                        await ServiceImpl.GetReasons(context, "PowerShellGallery", "contactSupport");
                        break;
                    }
                case "/reasons/reportAbuse/NuGet":
                    {
                        await ServiceImpl.GetReasons(context, "NuGet", "reportAbuse");
                        break;
                    }
                case "/reasons/reportAbuse/PowerShellGallery":
                    {
                        await ServiceImpl.GetReasons(context, "PowerShellGallery", "reportAbuse");
                        break;
                    }
                default:
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        await context.Response.WriteAsync("NotFound");
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
                        await ServiceImpl.ContactOwners(context, _storageManager);
                        break;
                    }
                case "/reportAbuse":
                    {
                        await ServiceImpl.ReportAbuse(context, _storageManager);
                        break;
                    }
                case "/contactSupport":
                    {
                        await ServiceImpl.ContactSupport(context, _storageManager);
                        break;
                    }

                case "/invitePackageOwner":
                    {
                        await ServiceImpl.InvitePackageOwner(context, _storageManager);
                        break;
                    }
                case "/newAccountWelcome":
                    {
                        await ServiceImpl.NewAccountWelcome(context, _storageManager);
                        break;
                    }
                case "/changeEmailNotice":
                    {
                        await ServiceImpl.ChangeEmailNotice(context, _storageManager);
                        break;
                    }
                case "/resetPasswordInstructions":
                    {
                        await ServiceImpl.ResetPasswordInstructions(context, _storageManager);
                        break;
                    }
                case "/editCredential":
                    {
                        await ServiceImpl.EditCredential(context, _storageManager);
                        break;
                    }
                default:
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        await context.Response.WriteAsync("NotFound");
                        break;
                    }
            }
        }
    }
}