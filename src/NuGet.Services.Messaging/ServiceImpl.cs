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
            //TODO: add authorization

            // just shows how to return a message.  
            await context.Response.WriteAsync("ContactOwners OK");


            // read posted json file
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
            
            
            //  gather data
            
            //================================================================

            
            // is this on the group, or on the owner themself?  Get from AAD?
            bool contactAllowed = true; 

            //==============================================================

            if (!contactAllowed)
            {
                // TODO:  do something different
                throw new Exception("Owner does not want to be contacted.");
            }



            List<string> ownersAddressesList = await ServiceHelper.GetOwnerEmailAddressesFromPackageID(packageId);
            string ownersAddresses = string.Join(",", ownersAddressesList.ToArray()); 
            string fromUserAddress = await ServiceHelper.GetUserEmailAddressFromUsername(fromUsername);

            string subject = String.Format(CultureInfo.CurrentCulture, Constants.ContactOwners_EmailSubject, brand, packageId);
            string bodyText = String.Format(
                CultureInfo.CurrentCulture,
                Constants.ContactOwners_EmailBody_Text, 
                fromUsername,
                fromUserAddress, 
                packageId, 
                message, 
                brand,
                Constants.ChangeEmailNotificationsURL);
            string bodyHTML = String.Format(
                CultureInfo.CurrentCulture,
                Constants.ContactOwners_EmailBody_HTML,
                fromUsername,
                fromUserAddress,
                packageId,
                message,
                brand,
                Constants.ChangeEmailNotificationsURL);





            //  compose email
            
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
            
            //ServiceHelper.SaveEmail(email as Stream, ??);

            //====================================
            

            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }


        /// <summary>
        /// Creates and formats ReportAbuse email, and stores it in an Azure Storage Blob.
        /// Uses an AAD connection to obtain required information.
        /// </summary>
        /// <param name="context">Request body contains JSON file of required data, passed from front-end.</param>
        /// <returns></returns>
        public static async Task ReportAbuse(IOwinContext context)
        {
            //TODO: add authorization

            //TODO: grab data from context and spool into folder/storage

            await context.Response.WriteAsync("ReportAbuse OK");

            // read posted file
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
                // TODO:  do this better
                throw new Exception("Both fromUsername and fromAddress cannot be null.");
            }

            if (!ownersContacted)
            {
                // We should never get here, but just in case
                // TODO:  do this better
                throw new Exception("Try contacting owners first.");
            }
            


            // gather data 
            
            string packageURL = Constants.NuGetURL + "/packages/" + packageId;
            string versionURL = packageURL + "/" + packageVersion;

            if (String.IsNullOrEmpty(fromAddress))
            {
                fromAddress = await ServiceHelper.GetUserEmailAddressFromUsername(fromUsername);
            }




            string subject = String.Format(CultureInfo.CurrentCulture, Constants.ReportAbuse_EmailSubject, brand, packageId, packageVersion, reason);
            string bodyText = String.Format(
                CultureInfo.CurrentCulture,
                Constants.ReportAbuse_EmailBody_Text,
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
                Constants.ReportAbuse_EmailBody_HTML,
                fromUsername,
                fromAddress,
                packageId,
                packageURL,
                packageVersion,
                versionURL,
                reason,
                ownersContacted ? "Yes" : "No",
                message);




            //  compose message

            MailMessage email = new MailMessage();
            email.From = new MailAddress(fromAddress);
            email.To.Add(Constants.SupportTeamEmail);
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

            //====================================

            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }

        /// <summary>
        /// Creates and formats ContactSupport email, and stores it in an Azure Storage Blob.
        /// Uses an AAD connection to obtain required information.
        /// </summary>
        /// <param name="context">Request body contains JSON file of required data, passed from front-end.</param>
        /// <returns></returns>
        public static async Task ContactSupport(IOwinContext context)
        {
            //TODO: add authorization

            await context.Response.WriteAsync("ContactSupport OK");

            // read posted file
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



            // gather data 
            string packageURL = Constants.NuGetURL + "/packages/" + packageId;
            string versionURL = packageURL + "/" + packageVersion;

            string fromAddress = await ServiceHelper.GetUserEmailAddressFromUsername(fromUsername);
            

            string subject = String.Format(CultureInfo.CurrentCulture, Constants.ContactSupport_EmailSubject, brand, packageId, packageVersion, reason);
            string bodyText = String.Format(
                CultureInfo.CurrentCulture,
                Constants.ContactSupport_EmailBody_Text,
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
                Constants.ContactSupport_EmailBody_HTML,
                fromUsername,
                fromAddress,
                packageId,
                packageURL,
                packageVersion,
                versionURL,
                reason,
                message);




            //  compose message

            MailMessage email = new MailMessage();
            email.From = new MailAddress(fromAddress);
            email.To.Add(Constants.SupportTeamEmail);
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

            //====================================

            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }


        /// <summary>
        /// Creates and formats ConfirmOwnerInvite email, and stores it in an Azure Storage Blob.
        /// Uses an AAD connection to obtain required information.
        /// </summary>
        /// <param name="context">Request body contains JSON file of required data, passed from front-end.</param>
        /// <returns></returns>
        public static async Task InvitePackageOwner(IOwinContext context)
        {
            //TODO: add authorization

            await context.Response.WriteAsync("ConfirmOwnerInvite OK");

            // read posted file
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



            // gather data 
            string toAddress = await ServiceHelper.GetUserEmailAddressFromUsername(toUsername);
            string fromAddress = await ServiceHelper.GetUserEmailAddressFromUsername(fromUsername);



            string subject = String.Format(CultureInfo.CurrentCulture, Constants.ConfirmOwnerInvite_EmailSubject, brand, fromUsername, packageId);
            string bodyText = String.Format(
                CultureInfo.CurrentCulture,
                Constants.ConfirmOwnerInvite_EmailBody_Text,
                fromUsername,
                packageId,
                brand,
                Constants.ConfirmOwnershipURL, 
                brand);
            string bodyHTML = String.Format(
                CultureInfo.CurrentCulture,
                Constants.ConfirmOwnerInvite_EmailBody_HTML,
                fromUsername,
                packageId,
                brand,
                Constants.ConfirmOwnershipURL,
                brand);




            //  compose message

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

            //====================================

            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }
        
    }
}