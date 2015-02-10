
namespace NuGet.Services.Messaging.Brand.PowerShellGallery
{

    // PowerShell Gallery branded constants

    public class Constants : IConstants
    {

        private const string _siteRoot = "http://www.powershellgallery.com/";
        private const string _brand = "PowerShell";
        private const string _entityName = "module";
        private const string _supportTeamEmail = "support@powershellgallery.com";
        private const string _changeEmailNotificationsURL = "/profile/edit"; // TODO:  Get real path
        private const string _confirmPackageOwnershipInviteURL = "/owners/confirm";  // TODO:  Get real path

        

        // TODO:  Get real email formats
        private const string _contactOwners_EmailSubject = "[" + _brand + "Gallery] Message for owners of the module '{0}'";
        private const string _contactOwners_EmailBody_Text = @"User {0} &lt;{1}&gt; sends the following message to the owners of module '{2}':
            
            {3}

    -----------------------------------------------
    To stop receiving contact emails as an owner of this module, sign in to the PowerShell Gallery and 
    [change your email notification settings]({4}).";
        private const string _contactOwners_EmailBody_HTML = @"User {0} &lt;{1}&gt; sends the following message to the owners of module '{2}':
            
            {3}

    -----------------------------------------------
    <em>
    To stop receiving contact emails as an owner of this module, sign in to the PowerShell Gallery and 
    [change your email notification settings]({4}).
    </em>";

        private const string _reportAbuse_EmailSubject = "[" + _brand + " Gallery] Support Request for '{0}' version {1} (Reason: {2})";
        private const string _reportAbuse_EmailBody_Text = @"**Email:** {0} ({1})

**Module:** {2}
{3}

**Version:** {4}
{5}

**Reason:**
{6}

**Has the package owner been contacted?:**
{7}

**Message:**
{8}";
        private const string _reportAbuse_EmailBody_HTML = @"**Email:** {0} ({1})

**Module:** {2}
{3}

**Version:** {4}
{5}

**Reason:**
{6}

**Has the package owner been contacted?:**
{7}

**Message:**
{8}";

        private const string _contactSupport_EmailSubject = "["+_brand+" Gallery] Support Request for '{0}' version {1} (Reason: {2})";
        private const string _contactSupport_EmailBody_Text = @"**Email:** {0} ({1})

**Module:** {2}
{3}

**Version:** {4}
{5}

**Reason:**
{6}

**Message:**
{7}";
        private const string _contactSupport_EmailBody_HTML = @"**Email:** {0} ({1})

**Module:** {2}
{3}

**Version:** {4}
{5}

**Reason:**
{6}

**Message:**
{7}";

        private const string _invitePackageOwner_EmailSubject = "["+_brand+" Gallery] The user '{0}' wants to add you as an owner of the module '{1}'.";
        private const string _invitePackageOwner_EmailBody_Text = @"The user '{0}' wants to add you as an owner of the module '{1}'. 
If you do not want to be listed as an owner of this module, simply delete this email.

To accept this request and become a listed owner of the module, click the following URL:

[PowerShell Gallery]({3})

Thanks,
The PowerShell Gallery Team";
        private const string _invitePackageOwner_EmailBody_HTML = @"The user '{0}' wants to add you as an owner of the module '{1}'. 
If you do not want to be listed as an owner of this module, simply delete this email.

To accept this request and become a listed owner of the module, click the following URL:

[PowerShell Gallery]({3})

Thanks,
The PowerShell Gallery Team";





        public string SiteRoot
        {
            get { return _siteRoot; }
        }
        public string Brand
        {
            get { return _brand;  }
        }
        public string EntityName
        {
            get { return _entityName;  }
        }
        public string SupportTeamEmail
        {
            get { return _supportTeamEmail;  }
        }
        public string ChangeEmailNotificationsURL
        {
            get { return _changeEmailNotificationsURL;  }
        }
        public string ConfirmPackageOwnershipInviteURL
        {
            get { return _confirmPackageOwnershipInviteURL;  }
        }
        public string ContactOwners_EmailSubject
        {
            get { return _contactOwners_EmailSubject; }
        }
        public string ContactOwners_EmailBody_Text
        {
            get { return _contactOwners_EmailBody_Text; }
        }
        public string ContactOwners_EmailBody_HTML
        {
            get { return _contactOwners_EmailBody_HTML; }
        }
        public string ReportAbuse_EmailSubject
        {
            get { return _reportAbuse_EmailSubject; }
        }
        public string ReportAbuse_EmailBody_Text
        {
            get { return _reportAbuse_EmailBody_Text; }
        }
        public string ReportAbuse_EmailBody_HTML
        {
            get { return _reportAbuse_EmailBody_HTML; }
        }
        public string ContactSupport_EmailSubject
        {
            get { return _contactSupport_EmailSubject; }
        }
        public string ContactSupport_EmailBody_Text
        {
            get { return _contactSupport_EmailBody_Text; }
        }
        public string ContactSupport_EmailBody_HTML
        {
            get { return _contactSupport_EmailBody_HTML; }
        }
        public string InvitePackageOwner_EmailSubject
        {
            get { return _invitePackageOwner_EmailSubject; }
        }
        public string InvitePackageOwner_EmailBody_Text
        {
            get { return _invitePackageOwner_EmailBody_Text; }
        }
        public string InvitePackageOwner_EmailBody_HTML
        {
            get { return _invitePackageOwner_EmailBody_HTML; }
        }


    }
}