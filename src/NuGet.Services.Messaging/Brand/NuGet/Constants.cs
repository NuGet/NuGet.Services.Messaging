
using System.IO;
namespace NuGet.Services.Messaging.Brand.NuGet
{

    // NuGet Gallery branded constants

    public class Constants : IConstants
    {

        private const string _siteRoot = "http://www.nuget.org";
        private const string _brand = "NuGet";
        private const string _entityName = "package";
        private const string _supportTeamEmail = "support@nuget.org";
        private const string _packageURL = _siteRoot + "/packages/{0}";
        private const string _packageVersionURL = _packageURL + "/{1}";
        private const string _changeEmailNotificationsURL = _siteRoot + "/profile/notifications";
        private const string _confirmPackageOwnershipInviteURL = _packageURL + "/owners/confirm";

        private const string _verifyEmailURL = _siteRoot + "/profile/email/verify";  
        private const string _resetPasswordExpirationTime = "12";        
        private const string _resetPasswordURL = _siteRoot + "/profile/password/reset";    




        public string SiteRoot
        {
            get { return _siteRoot; }
        }
        public string Brand
        {
            get { return _brand; }
        }
        public string EntityName
        {
            get { return _entityName; }
        }
        public string SupportTeamEmail
        {
            get { return _supportTeamEmail; }
        }
        public string EntityURL
        {
            get { return _packageURL; }
        }
        public string EntityVersionURL
        {
            get { return _packageVersionURL; }
        }
        public string ChangeEmailNotificationsURL
        {
            get { return _changeEmailNotificationsURL; }
        }
        public string ConfirmPackageOwnershipInviteURL
        {
            get { return _confirmPackageOwnershipInviteURL; }
        }
        public string VerifyEmailURL
        {
            get { return _verifyEmailURL; }
        }
        public string ResetPasswordExpirationTime
        {
            get { return _resetPasswordExpirationTime; }
        }
        public string ResetPasswordURL
        {
            get { return _resetPasswordURL; }
        }






        // TODO:  Get real email formats
        private const string _contactOwners_EmailSubject = "NuGet Gallery: Message for owners of the package '{0}'";
        private const string _contactOwners_EmailBody_Text = @"User {0} <{1}> sends the following message to the owners of package '{2}':
            
            {3}

    To stop receiving contact emails as an owner of this package, sign in to the NuGet Gallery and change your email notification settings:
    {4}.";
        private const string _contactOwners_EmailBody_HTML = @"
<html>
<body>
    <p>User {0} &lt;{1}&gt; sends the following message to the owners of package '{2}':</p>
    <p>{3}</p>
    <em>
        To stop receiving contact emails as an owner of this package, sign in to the NuGet Gallery and 
        <a href='{4}'>change your email notification settings</a>.
    </em>
</body>
</html>";


        private const string _reportAbuse_EmailSubject = "NuGet Gallery: Support Request for '{0}' version {1} (Reason: {2})";
        private const string _reportAbuse_EmailBody_Text = @"
Email: 
{0} <{1}>

Package: 
{2}: {3}

Version: 
{4}:  {5}

Reason:
{6}

Has the package owner been contacted?:
{7}

Message:
{8}";
        private const string _reportAbuse_EmailBody_HTML = @"
<html>
<body>
    <div>
        <h3>Email:</h3>
        <p>
            {0} &lt;{1}&gt;
        </p>
    </div>
    <div>
        <h3>Package:</h3>
        <p>
            <a href='{2}'>{3}</a>
        </p>
    </div>
    <div>
        <h3>Version:</h3>
        <a href='{4}'>{5}</a>
    </div>
    <div>
        <h3>Reason:</h3>
        <p>{6}</p>
    </div>
    <div>
        <h3>Has the package owner been contacted?:</h3>
        <p>{7}</p>
    </div>
    <div>
        <h3>Message:</h3>
        <p>{8}</p>
    </div>
</body>
</html>";



        private const string _contactSupport_EmailSubject = "NuGet Gallery: Support Request for '{0}' version {1} (Reason: {2})";
        private const string _contactSupport_EmailBody_Text = @"
Email: 
{0} <{1}>

Package: 
{2}:  {3}

Version: 
{4}:  {5}

Reason:
{6}

Message:
{7}";
        private const string _contactSupport_EmailBody_HTML = @"
<html>
<body>
    <div>
        <h3>Email:</h3>
        <p>
            {0} &lt;{1}&gt;
        </p>
    </div>
    <div>
        <h3>Package:</h3>
        <p>
            <a href='{2}'>{3}</a>
        </p>
    </div>
    <div>
        <h3>Version:</h3>
        <a href='{4}'>{5}</a>
    </div>
    <div>
        <h3>Reason:</h3>
        <p>{6}</p>
    </div>
    <div>
        <h3>Message:</h3>
        <p>{7}</p>
    </div>
</body>
</html>";



        private const string _invitePackageOwner_EmailSubject = "NuGet Gallery: The user '{0}' wants to add you as an owner of the package '{1}'.";
        private const string _invitePackageOwner_EmailBody_Text = @"The user '{0}' wants to add you as an owner of the package '{1}'. 
If you do not want to be listed as an owner of this package, simply delete this email.

To accept this request and become a listed owner of the package, click the following URL:

{2}

Thanks,
The NuGet Gallery Team";
        private const string _invitePackageOwner_EmailBody_HTML = @"
<html >
<body>
    <p>The user '{0}' wants to add you as an owner of the package '{1}'.</p>
    <p>If you do not want to be listed as an owner of this package, simply delete this email.</p>
    <p>To accept this request and become a listed owner of the package, click the following URL:</p>
    <p><a href='{2}'>Accept Ownership Invitation</a></p>
    <p>Thanks,<br />
    The NuGet Gallery Team</p>
</body>
</html>";



        private const string _newAccountWelcome_EmailSubject = "NuGet Gallery: Please verify your account.";
        private const string _newAccountWelcome_EmailBody_Text = @"Thank you for registering with the NuGet Gallery! We can't wait to see what packages you'll upload.

So we can be sure to contact you, please verify your email address and click the following link:

{0}

Thanks,
The NuGet Gallery Team";
        private const string _newAccountWelcome_EmailBody_HTML = @"
<html>
<body>
    <p>Thank you for registering with the NuGet Gallery! We can't wait to see what packages you'll upload.</p>

    <p>So we can be sure to contact you, please verify your email address and click the following link:</p>
    <a href='{0}'>Verify Email</a>

    <p>Thanks,<br>
    The NuGet Gallery Team</p>
    
</body>
</html>";




        private const string _changeEmailNotice_oldEmail_EmailSubject = "NuGet Gallery: Recent changes to your account.";
        private const string _changeEmailNotice_oldEmail_EmailBody_Text = @"Hi there,

The email address associated to your NuGet account was recently changed from {0} to {1}.

Thanks,
The NuGet Gallery Team";
        private const string _changeEmailNotice_oldEmail_EmailBody_HTML = @"
<html>
<body>
    <p>
        Hi there,
    </p>
    <p>
        The email address associated to your NuGet account was recently changed from &lt;{0}&gt; to &lt;{1}&gt;.
    </p>
    <p>
        Thanks,<br>
        The NuGet Gallery Team
    </p>
</body>
</html>";





        private const string _changeEmailNotice_newEmail_EmailSubject = "NuGet Gallery: Please verify your new email address.";
        private const string _changeEmailNotice_newEmail_EmailBody_Text = @"You recently changed your NuGet email address. 

To verify your new email address, please click the following link:

{0}

Thanks,
The NuGet Gallery Team";
        private const string _changeEmailNotice_newEmail_EmailBody_HTML = @"
<html>
<body>
    <p>
        You recently changed your NuGet email address.
    </p>
    <p>
        To verify your new email address, please click the following link:
    </p>
    <a href='{0}'>Verify Email</a>
    <p>
        Thanks,<br>
        The NuGet Gallery Team
    </p>
</body>
</html>";




        private const string _resetPasswordInstructions_forgot_EmailSubject = "NuGet Gallery: Please reset your password.";
        private const string _resetPasswordInstructions_forgot_EmailBody_Text = @"The word on the street is you lost your password. Sorry to hear it!
If you haven't forgotten your password you can safely ignore this email. Your password has not been changed.

Click the following link within the next {0} hours to reset your password:

{1}

Thanks,
The NuGet Gallery Team";
        private const string _resetPasswordInstructions_forgot_EmailBody_HTML = @"
<html>
<body>
    <p>
        The word on the street is you lost your password. Sorry to hear it!
    </p>
    <p>
        If you haven't forgotten your password you can safely ignore this email. Your password has not been changed.
    </p>
    <p>
        Click the following link within the next {0} hours to reset your password:
    </p>
    <a href='{1}'>Reset Password</a>
    <p>
        Thanks,<br>
        The NuGet Gallery Team
    </p>
</body>
</html>";





        private const string _resetPasswordInstructions_reset_EmailSubject = "NuGet Gallery: Please set your password.";
        private const string _resetPasswordInstructions_reset_EmailBody_Text = @"The word on the street is you want to set a password for your account.
If you didn't request a password, you can safely ignore this message. A password has not yet been set.

Click the following link within the next {0} hours to set your password:

{1}

Thanks,
The NuGet Gallery Team";
        private const string _resetPasswordInstructions_reset_EmailBody_HTML = @"
<html>
<body>
    <p>
        The word on the street is you want to set a password for your account.
    </p>
    <p>
        If you didn't request a password, you can safely ignore this message. A password has not yet been set.
    </p>
    <p>
        Click the following link within the next {0} hours to set your password:
    </p>
    <a href='{1}'>Reset Password</a>
    <p>
        Thanks,<br>
        The NuGet Gallery Team
    </p>
</body>
</html>";




        private const string _editCredential_add_EmailSubject = "NuGet Gallery: {0} added to your account";
        private const string _editCredential_add_EmailBody_Text = @"
A {0} was added to your account and can now be used to log in. 
If you did not request this change, please reply to this email to contact support.";
        private const string _editCredential_add_EmailBody_HTML = @"
<html>
<body>
    <p>
        A {0} was added to your account and can now be used to log in. 
    </p>
    <p>
        If you did not request this change, please reply to this email to contact support.
    </p>
</body>
</html>";




        private const string _editCredential_remove_EmailSubject = "NuGet Gallery: {0} removed from your account";
        private const string _editCredential_remove_EmailBody_Text = @"
A {0} was removed from your account and can no longer be used to log in. 
If you did not request this change, please reply to this email to contact support.";
        private const string _editCredential_remove_EmailBody_HTML = @"
<html>
<body>
    <p>
        A {0} was removed from your account and can no longer be used to log in. 
    </p>
    <p>
        If you did not request this change, please reply to this email to contact support.
    </p>
</body>
</html>";






        
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
        public string NewAccountWelcome_EmailSubject
        {
            get { return _newAccountWelcome_EmailSubject; }
        }
        public string NewAccountWelcome_EmailBody_Text
        {
            get { return _newAccountWelcome_EmailBody_Text; }
        }
        public string NewAccountWelcome_EmailBody_HTML
        {
            get { return _newAccountWelcome_EmailBody_HTML; }
        }
        public string ChangeEmailNotice_oldEmail_EmailSubject
        {
            get { return _changeEmailNotice_oldEmail_EmailSubject; }
        }
        public string ChangeEmailNotice_oldEmail_EmailBody_Text
        {
            get { return _changeEmailNotice_oldEmail_EmailBody_Text; }
        }
        public string ChangeEmailNotice_oldEmail_EmailBody_HTML
        {
            get { return _changeEmailNotice_oldEmail_EmailBody_HTML; }
        }
        public string ChangeEmailNotice_newEmail_EmailSubject
        {
            get { return _changeEmailNotice_newEmail_EmailSubject; }
        }
        public string ChangeEmailNotice_newEmail_EmailBody_Text
        {
            get { return _changeEmailNotice_newEmail_EmailBody_Text; }
        }
        public string ChangeEmailNotice_newEmail_EmailBody_HTML
        {
            get { return _changeEmailNotice_newEmail_EmailBody_HTML; }
        }
        public string ResetPasswordInstructions_forgot_EmailSubject
        {
            get { return _resetPasswordInstructions_forgot_EmailSubject; }
        }
        public string ResetPasswordInstructions_forgot_EmailBody_Text
        {
            get { return _resetPasswordInstructions_forgot_EmailBody_Text; }
        }
        public string ResetPasswordInstructions_forgot_EmailBody_HTML
        {
            get { return _resetPasswordInstructions_forgot_EmailBody_HTML; }
        }
        public string ResetPasswordInstructions_reset_EmailSubject
        {
            get { return _resetPasswordInstructions_reset_EmailSubject; }
        }
        public string ResetPasswordInstructions_reset_EmailBody_Text
        {
            get { return _resetPasswordInstructions_reset_EmailBody_Text; }
        }
        public string ResetPasswordInstructions_reset_EmailBody_HTML
        {
            get { return _resetPasswordInstructions_reset_EmailBody_HTML; }
        }
        public string EditCredential_add_EmailSubject
        {
            get { return _editCredential_add_EmailSubject; }
        }
        public string EditCredential_add_EmailBody_Text
        {
            get { return _editCredential_add_EmailBody_Text; }
        }
        public string EditCredential_add_EmailBody_HTML
        {
            get { return _editCredential_add_EmailBody_HTML; }
        }
        public string EditCredential_remove_EmailSubject
        {
            get { return _editCredential_remove_EmailSubject; }
        }
        public string EditCredential_remove_EmailBody_Text
        {
            get { return _editCredential_remove_EmailBody_Text; }
        }
        public string EditCredential_remove_EmailBody_HTML
        {
            get { return _editCredential_remove_EmailBody_HTML; }
        }

    }
}