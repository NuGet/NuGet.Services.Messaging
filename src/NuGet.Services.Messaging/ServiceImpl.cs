using Microsoft.Owin;
using System.Net;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Net.Mail;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;

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

        /// <summary>
        /// Creates and formats ContactOwners email, and stores it in an Azure Storage Blob.
        /// Uses an AAD connection to obtain required information.
        /// </summary>
        /// <param name="context">Request body contains JSON file of required data, passed from front-end.</param>
        /// <returns></returns>
        public static async Task ContactOwners(IOwinContext context)
        {
            StreamReader reader = new StreamReader(context.Request.Body);
            string bodyContent = reader.ReadToEnd();
            JObject root = JObject.Parse(bodyContent);
            

            //TODO: verify required fields


            // pull data
            string packageId = root.Value<string>("packageId");
            string packageVersion = root.Value<string>("packageVersion");
            bool copyMe = root.Value<bool>("copyMe");
            string message = root.Value<string>("message");
            string fromUsername = root.Value<string>("fromUsername");
            string brand = root.Value<string>("brand");

            IConstants brandValues = ServiceHelper.GetBrandConstants(brand);
            

            List<string> ownersAddressesList = await ServiceHelper.GetOwnerEmailAddressesFromPackageID(packageId);
            string ownersAddresses = string.Join(",", ownersAddressesList.ToArray()); 

            //================================================================

            // TODO
            // Get from AAD (somewhere)
            bool contactAllowed = true; 

            //==============================================================

            if (!contactAllowed)
            {
                await context.Response.WriteAsync("ContactOwners FAIL: ContactNotAllowed");
                //context.Response.StatusCode = (int)HttpStatusCode.OK;
                // TODO:  verify returned message protocol
                return;
            }

            string fromUserAddress = await ServiceHelper.GetUserEmailAddressFromUsername(fromUsername);
            
            
            //  compose email
            string subject = String.Format(CultureInfo.CurrentCulture, brandValues.ContactOwners_EmailSubject, packageId);
            string bodyText = String.Format(
                CultureInfo.CurrentCulture,
                brandValues.ContactOwners_EmailBody_Text, 
                fromUsername,
                fromUserAddress, 
                packageId, 
                message, 
                brandValues.ChangeEmailNotificationsURL);

            string bodyHTML = String.Format(
                CultureInfo.CurrentCulture,
                brandValues.ContactOwners_EmailBody_HTML,
                fromUsername,
                fromUserAddress,
                packageId,
                message,
                brandValues.ChangeEmailNotificationsURL);


            MailMessage email = new MailMessage();
            email.From = new MailAddress(fromUserAddress);
            email.To.Add(ownersAddresses);
            if (copyMe)
            {
                email.CC.Add(fromUserAddress);
            }
            email.Subject = subject;

            AlternateView plainMessage = AlternateView.CreateAlternateViewFromString(bodyText, null, "text/plain");
            AlternateView htmlMessage = AlternateView.CreateAlternateViewFromString(bodyHTML, null, "text/html");
            email.AlternateViews.Add(plainMessage);
            email.AlternateViews.Add(htmlMessage);

            
            

           
            //===================================
            // TODO:  enqueue message

            // Use Azure Storage Blob
            
            bool result = ServiceHelper.SaveEmail(email);

            //====================================
            
            if (result)
            {
                await context.Response.WriteAsync("ContactOwners OK");
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                return;
            }
            else
            {
                await context.Response.WriteAsync("ContactOwners FAIL: EmailNotSent");
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                return;
            }

        }


        /// <summary>
        /// Creates and formats ReportAbuse email, and stores it in an Azure Storage Blob.
        /// Uses an AAD connection to obtain required information.
        /// </summary>
        /// <param name="context">Request body contains JSON file of required data, passed from front-end.</param>
        /// <returns></returns>
        public static async Task ReportAbuse(IOwinContext context)
        {
            StreamReader reader = new StreamReader(context.Request.Body);
            string bodyContent = reader.ReadToEnd();
            JObject root = JObject.Parse(bodyContent);


            //TODO: verify required fields


            // pull data
            string packageId = root.Value<string>("packageId");
            string packageVersion = root.Value<string>("packageVersion");
            bool copyMe = root.Value<bool>("copyMe");
            string reason = root.Value<string>("reason");
            string message = root.Value<string>("message");
            bool ownersContacted = root.Value<bool>("ownersContacted");
            string fromUsername = root.Value<string>("fromUsername");
            string fromAddress = root.Value<string>("fromAddress");
            string brand = root.Value<string>("brand");

            if (String.IsNullOrEmpty(fromUsername) && String.IsNullOrEmpty(fromAddress))
            {
                await context.Response.WriteAsync("ReportAbuse FAIL: Insufficient parameters.");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                // TODO:  use better status code
                return;
            }

            if (!ownersContacted)
            {
                await context.Response.WriteAsync("ReportAbuse FAIL: Try ContactOwners first.");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                // TODO:  use better status code
                return;
                
            }
            
            IConstants brandValues = ServiceHelper.GetBrandConstants(brand);

            string packageURL = brandValues.SiteRoot + "/packages/" + packageId;
            string versionURL = packageURL + "/" + packageVersion;

            if (String.IsNullOrEmpty(fromAddress))
            {
                fromAddress = await ServiceHelper.GetUserEmailAddressFromUsername(fromUsername);
            }



            // compose message
            string subject = String.Format(CultureInfo.CurrentCulture, brandValues.ReportAbuse_EmailSubject, packageId, packageVersion, reason);
            string bodyText = String.Format(
                CultureInfo.CurrentCulture,
                brandValues.ReportAbuse_EmailBody_Text,
                fromUsername,
                fromAddress,
                packageId,
                packageURL,
                packageVersion,
                versionURL,
                reason,
                ownersContacted ? "Yes" : "No",
                message);
            string bodyHTML = String.Format(
                CultureInfo.CurrentCulture,
                brandValues.ReportAbuse_EmailBody_HTML,
                fromUsername,
                fromAddress,
                packageId,
                packageURL,
                packageVersion,
                versionURL,
                reason,
                ownersContacted ? "Yes" : "No",
                message);


            MailMessage email = new MailMessage();
            email.From = new MailAddress(fromAddress);
            email.To.Add(brandValues.SupportTeamEmail);
            if (copyMe)
            {
                email.CC.Add(fromAddress);
            }
            email.Subject = subject;

            AlternateView plainMessage = AlternateView.CreateAlternateViewFromString(bodyText, null, "text/plain");
            AlternateView htmlMessage = AlternateView.CreateAlternateViewFromString(bodyHTML, null, "text/html");
            email.AlternateViews.Add(plainMessage);
            email.AlternateViews.Add(htmlMessage);





            //===================================
            // TODO:  enqueue message

            // Use Azure Storage Blob

            bool result = ServiceHelper.SaveEmail(email);

            //====================================

            if (result)
            {
                await context.Response.WriteAsync("ReportAbuse OK");
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                return;
            }
            else
            {
                await context.Response.WriteAsync("ReportAbuse FAIL: EmailNotSent");
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                return;
            }
        }

        /// <summary>
        /// Creates and formats ContactSupport email, and stores it in an Azure Storage Blob.
        /// Uses an AAD connection to obtain required information.
        /// </summary>
        /// <param name="context">Request body contains JSON file of required data, passed from front-end.</param>
        /// <returns></returns>
        public static async Task ContactSupport(IOwinContext context)
        {
            StreamReader reader = new StreamReader(context.Request.Body);
            string bodyContent = reader.ReadToEnd();
            JObject root = JObject.Parse(bodyContent);


            //TODO: verify required fields


            // pull data
            string packageId = root.Value<string>("packageId");
            string packageVersion = root.Value<string>("packageVersion");
            bool copyMe = root.Value<bool>("copyMe");
            string reason = root.Value<string>("reason");
            string message = root.Value<string>("message");
            string fromUsername = root.Value<string>("fromUsername");
            string brand = root.Value<string>("brand");

            IConstants brandValues = ServiceHelper.GetBrandConstants(brand);

 
            string packageURL = brandValues.SiteRoot + "/packages/" + packageId;
            string versionURL = packageURL + "/" + packageVersion;

            string fromAddress = await ServiceHelper.GetUserEmailAddressFromUsername(fromUsername);
            


            //  compose message
            string subject = String.Format(CultureInfo.CurrentCulture, brandValues.ContactSupport_EmailSubject, packageId, packageVersion, reason);
            string bodyText = String.Format(
                CultureInfo.CurrentCulture,
                brandValues.ContactSupport_EmailBody_Text,
                fromUsername,
                fromAddress,
                packageId,
                packageURL,
                packageVersion,
                versionURL,
                reason,
                message);
            string bodyHTML = String.Format(
                CultureInfo.CurrentCulture,
                brandValues.ContactSupport_EmailBody_HTML,
                fromUsername,
                fromAddress,
                packageId,
                packageURL,
                packageVersion,
                versionURL,
                reason,
                message);


            MailMessage email = new MailMessage();
            email.From = new MailAddress(fromAddress);
            email.To.Add(brandValues.SupportTeamEmail);
            if (copyMe)
            {
                email.CC.Add(fromAddress);
            }
            email.Subject = subject;

            AlternateView plainMessage = AlternateView.CreateAlternateViewFromString(bodyText, null, "text/plain");
            AlternateView htmlMessage = AlternateView.CreateAlternateViewFromString(bodyHTML, null, "text/html");
            email.AlternateViews.Add(plainMessage);
            email.AlternateViews.Add(htmlMessage);



            //===================================
            // TODO:  enqueue message

            // Use Azure Storage Blob

            bool result = ServiceHelper.SaveEmail(email);

            //====================================

            if (result)
            {
                await context.Response.WriteAsync("ContactSupport OK");
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                return;
            }
            else
            {
                await context.Response.WriteAsync("ContactSupport FAIL: EmailNotSent");
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                return;
            }
        }


        /// <summary>
        /// Creates and formats ConfirmOwnerInvite email, and stores it in an Azure Storage Blob.
        /// Uses an AAD connection to obtain required information.
        /// </summary>
        /// <param name="context">Request body contains JSON file of required data, passed from front-end.</param>
        /// <returns></returns>
        public static async Task InvitePackageOwner(IOwinContext context)
        {
            StreamReader reader = new StreamReader(context.Request.Body);
            string bodyContent = reader.ReadToEnd();
            JObject root = JObject.Parse(bodyContent);


            //TODO: verify required fields


            // pull data
            string packageId = root.Value<string>("packageId");
            string packageVersion = root.Value<string>("packageVersion");
            string message = root.Value<string>("message");
            string toUsername = root.Value<string>("toUsername");
            string fromUsername = root.Value<string>("fromUsername");
            string brand = root.Value<string>("brand");

            IConstants brandValues = ServiceHelper.GetBrandConstants(brand);


            //  compose message
            string toAddress = await ServiceHelper.GetUserEmailAddressFromUsername(toUsername);
            string fromAddress = await ServiceHelper.GetUserEmailAddressFromUsername(fromUsername);

            string subject = String.Format(CultureInfo.CurrentCulture, brandValues.InvitePackageOwner_EmailSubject, fromUsername, packageId);
            string bodyText = String.Format(
                CultureInfo.CurrentCulture,
                brandValues.InvitePackageOwner_EmailBody_Text,
                fromUsername,
                packageId,
                brandValues.ConfirmPackageOwnershipInviteURL);
            string bodyHTML = String.Format(
                CultureInfo.CurrentCulture,
                brandValues.InvitePackageOwner_EmailBody_HTML,
                fromUsername,
                packageId,
                brandValues.ConfirmPackageOwnershipInviteURL);


            MailMessage email = new MailMessage();
            email.From = new MailAddress(fromAddress);
            email.To.Add(toAddress);
            email.Subject = subject;

            AlternateView plainMessage = AlternateView.CreateAlternateViewFromString(bodyText, null, "text/plain");
            AlternateView htmlMessage = AlternateView.CreateAlternateViewFromString(bodyHTML, null, "text/html");
            email.AlternateViews.Add(plainMessage);
            email.AlternateViews.Add(htmlMessage);



            //===================================
            // TODO:  enqueue message

            // Use Azure Storage Blob

            bool result = ServiceHelper.SaveEmail(email);

            //====================================

            if (result)
            {
                await context.Response.WriteAsync("InvitePackageOwner OK");
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                return;
            }
            else
            {
                await context.Response.WriteAsync("InvitePackageOwner FAIL: EmailNotSent");
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                return;
            }
        }
        
    }
}