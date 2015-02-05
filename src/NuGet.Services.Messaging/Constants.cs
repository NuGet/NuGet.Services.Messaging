using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NuGet.Services.Messaging
{
    internal static class Constants
    {

        // TODO:  Figure these out

        public const string ChangeEmailNotificationsURL = "brand/profile/notifications/change";  // TODO:  figure out this URL, based on brand
        public const string NuGetURL = "https://www.nuget.org";   // TODO:  this should come from a config file
        public const string PowerShellGalleryURL = "https://www.powershellgallery.com";  // TODO:  this should populate from a config file
        public const string SupportTeamEmail = "support@powershellgallery.com";  // TODO:  this should come from a config file
        public const string ConfirmOwnershipURL = "brand/confirmOwnership";   // TODO:  figure out this URL, based on brand






        // ContactOwners
        public const string ContactOwners_EmailSubject = "[{0} Gallery] Message for owners of the module '{1}'";
        public const string ContactOwners_EmailBody_Text = @"User {0} &lt;{1}&gt; sends the following message to the owners of module '{2}':
            
            {3}

-----------------------------------------------
    To stop receiving contact emails as an owner of this module, sign in to the {4} Gallery and 
    [change your email notification settings]({5}).";
        public const string ContactOwners_EmailBody_HTML = @"User {0} &lt;{1}&gt; sends the following message to the owners of module '{2}':
            
            {3}

-----------------------------------------------
<em style=""font-size: 0.8em;"">
    To stop receiving contact emails as an owner of this module, sign in to the {4} Gallery and 
    [change your email notification settings]({5}).
</em>";





        // ReportAbuse
        public const string ReportAbuse_EmailSubject = "[{0} Gallery] Support Request for '{1}' version {2} (Reason: {3})";
        public const string ReportAbuse_EmailBody_Text = @"**Email:** {0} ({1})

**Module:** {2}
{3}

**Version:** {4}
{5}

**Reason:**
{6}

**Has the module owner been contacted?:**
{7}

**Message:**
{8}";
        public const string ReportAbuse_EmailBody_HTML = @"**Email:** {0} ({1})

**Module:** {2}
{3}

**Version:** {4}
{5}


**Reason:**
{6}

**Has the module owner been contacted?:**
{7}

**Message:**
{8}";







        // ContactSupport
        public const string ContactSupport_EmailSubject = "[{0} Gallery] Support Request for '{1}' version {2} (Reason: {3})";
        public const string ContactSupport_EmailBody_Text = @"**Email:** {0} ({1})

**Module:** {2}
{3}

**Version:** {4}
{5}

**Reason:**
{6}

**Message:**
{7}";
        public const string ContactSupport_EmailBody_HTML = @"**Email:** {0} ({1})

**Module:** {2}
{3}

**Version:** {4}
{5}

**Reason:**
{6}

**Message:**
{8}";







        // ConfirmOwnerInvite
        public const string ConfirmOwnerInvite_EmailSubject = "[{0} Gallery] The user '{1}' wants to add you as an owner of the module '{2}'.";
        public const string ConfirmOwnerInvite_EmailBody_Text = @"The user '{0}' wants to add you as an owner of the module '{1}'. 
If you do not want to be listed as an owner of this module, simply delete this email.

To accept this request and become a listed owner of the module, click the following URL:

[{2}]({3})

Thanks,
The {brand} Team";
        public const string ConfirmOwnerInvite_EmailBody_HTML = @"The user '{0}' wants to add you as an owner of the module '{1}'. 
If you do not want to be listed as an owner of this module, simply delete this email.

To accept this request and become a listed owner of the module, click the following URL:

[{2}]({3})

Thanks,
The {brand} Team";




    }
}