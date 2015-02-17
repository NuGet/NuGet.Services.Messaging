using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Services.Messaging
{

    // Interface for all branding constants.  Each new brand must contain these values in their constants file.

    public interface IConstants
    {
        string SiteRoot { get; }
        string Brand { get; }
        string EntityName { get; }
        string SupportTeamEmail { get; }
        string EntityURL { get; }
        string EntityVersionURL { get; }
        string ChangeEmailNotificationsURL { get; }
        string ConfirmPackageOwnershipInviteURL { get; }


        string ContactOwners_EmailSubject { get; }
        string ContactOwners_EmailBody_Text { get; }
        string ContactOwners_EmailBody_HTML { get; }

        string ReportAbuse_EmailSubject { get; }
        string ReportAbuse_EmailBody_Text { get; }
        string ReportAbuse_EmailBody_HTML { get; }

        string ContactSupport_EmailSubject { get; }
        string ContactSupport_EmailBody_Text { get; }
        string ContactSupport_EmailBody_HTML { get; }

        string InvitePackageOwner_EmailSubject { get; }
        string InvitePackageOwner_EmailBody_Text { get; }
        string InvitePackageOwner_EmailBody_HTML { get; }

    }
}
