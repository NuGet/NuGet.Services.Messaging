using Microsoft.Owin;
using System.Net;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using NuGet.Services.Metadata.Catalog.Persistence;

namespace NuGet.Services.Messaging
{
    
    public static class ServiceImpl
    {

        /// <summary>
        /// Create and format ContactOwners message (in JSON), and then store it in an Azure Storage Blob.
        /// Use an AAD connection to obtain required information.
        /// </summary>
        /// <param name="context">Context's request body contains JSON file of parameters, passed from front-end.</param>
        /// <param name="storageManager">Use to store message.</param>
        /// <returns></returns>
        public static async Task ContactOwners(IOwinContext context, StorageManager storageManager)
        {
            StreamReader reader = new StreamReader(context.Request.Body);
            string bodyContent = reader.ReadToEnd();
            JObject root = JObject.Parse(bodyContent);
            

            // verify parameters
            String[] requiredParams = { "packageId", "packageVersion", "copyMe", "message", "fromUsername", "brand" };
            List<string> missingParams = ServiceHelper.VerifyRequiredParameters(root, requiredParams);
            if (missingParams.Count > 0)
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "ContactOwners FAIL: Insufficient parameters.");
                JArray missingParamsArr = new JArray(missingParams);
                errorObject.Add("missingParameters", missingParamsArr);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }


            // pull data
            string packageId = root.Value<string>("packageId");
            string packageVersion = root.Value<string>("packageVersion");
            bool copyMe = root.Value<bool>("copyMe");
            string message = root.Value<string>("message");
            string fromUsername = root.Value<string>("fromUsername");
            string brand = root.Value<string>("brand");


            // TODO:  validate parameters
            

            IConstants brandValues = ServiceHelper.GetBrandConstants(brand);

            if (brandValues == null)
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "ContactOwners FAIL: "+brand+" is not a valid brand.  Options:  "+ServiceHelper.BrandsOptions());
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }
            

            List<string> ownersAddressesList = await ServiceHelper.GetOwnerEmailAddressesFromPackageID(packageId);
            string ownersAddresses = string.Join(",", ownersAddressesList.ToArray()); 

            //================================================================

            // TODO: Get from AAD (somewhere)
            bool contactAllowed = true; 

            //==============================================================

            if (!contactAllowed)
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "ContactOwners FAIL: Contact owners not allowed.");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
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



            // compose JSON
            JObject emailJSON = new JObject();
            emailJSON.Add("to", ownersAddresses);
            emailJSON.Add("from", fromUserAddress);
            if (copyMe)
            {
                emailJSON.Add("cc",fromUserAddress);
            }
            emailJSON.Add("subject", subject);
            JObject body = new JObject();
            body.Add("text", bodyText);
            body.Add("html", bodyHTML);
            emailJSON.Add("body", body);



            // enqueue message
            bool result = await storageManager.Save(new StringStorageContent(emailJSON.ToString(), "application/json"), "email1");
            
            if (result)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync("ContactOwners OK");
                return;
            }
            else
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.ServiceUnavailable);
                errorObject.Add("description", "ContactOwners FAIL: Storage unavailable.  Email not sent.");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }

        }


        /// <summary>
        /// Create and format ReportAbuse email, and then store it in an Azure Storage Blob.
        /// Use an AAD connection to obtain required information.
        /// </summary>
        /// <param name="context">Context's request body contains JSON file of parameters, passed from front-end.</param>
        /// <param name="storageManager">Use to store message.</param>
        /// <returns></returns>
        public static async Task ReportAbuse(IOwinContext context, StorageManager storageManager)
        {
            StreamReader reader = new StreamReader(context.Request.Body);
            string bodyContent = reader.ReadToEnd();
            JObject root = JObject.Parse(bodyContent);


            // verify parameters
            String[] requiredParams = { "packageId", "packageVersion", "copyMe", "reason", "message", "ownersContacted", "fromUsername", "fromAddress", "brand" };
            List<string> missingParams = ServiceHelper.VerifyRequiredParameters(root, requiredParams);
            if (missingParams.Count > 0)
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "ReportAbuse FAIL: Insufficient parameters.");
                JArray missingParamsArr = new JArray(missingParams);
                errorObject.Add("missingParameters", missingParamsArr);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }


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

            // TODO:  validate parameters

            if (String.IsNullOrEmpty(fromUsername) && String.IsNullOrEmpty(fromAddress))
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "ReportAbuse FAIL: Insufficient parameters.  Need either fromUsername or fromAddress.");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }

            if (!ownersContacted)
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "ReportAbuse FAIL: Try ContactOwners first.");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }
            
            IConstants brandValues = ServiceHelper.GetBrandConstants(brand);
            if (brandValues == null)
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "ReportAbuse FAIL: "+brand+" is not a valid brand.  Options:  "+ServiceHelper.BrandsOptions());
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }




            string packageURL = brandValues.SiteRoot + "/packages/" + packageId;
            string versionURL = packageURL + "/" + packageVersion;

            if (String.IsNullOrEmpty(fromAddress))
            {
                fromAddress = await ServiceHelper.GetUserEmailAddressFromUsername(fromUsername);
            }


            string subject = String.Format(
                CultureInfo.CurrentCulture, 
                brandValues.ReportAbuse_EmailSubject, 
                packageId, 
                packageVersion, 
                reason);
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


            // compose JSON
            JObject emailJSON = new JObject();
            emailJSON.Add("to", brandValues.SupportTeamEmail);
            emailJSON.Add("from", fromAddress);
            if (copyMe)
            {
                emailJSON.Add("cc", fromAddress);
            }
            emailJSON.Add("subject", subject);
            JObject body = new JObject();
            body.Add("text", bodyText);
            body.Add("html", bodyHTML);
            emailJSON.Add("body", body);



            // enqueue message
            bool result = await storageManager.Save(new StringStorageContent(emailJSON.ToString(), "application/json"), "email1");

            if (result)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync("ReportAbuse OK");
                return;
            }
            else
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.ServiceUnavailable);
                errorObject.Add("description", "ReportAbuse FAIL: Storage unavailable.  Email not sent.");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }
        }

        /// <summary>
        /// Create and format ContactSupport email, and then store it in an Azure Storage Blob.
        /// Use an AAD connection to obtain required information.
        /// </summary>
        /// <param name="context">Context's request body contains JSON file of parameters, passed from front-end.</param>
        /// <param name="storageManager">Use to store message.</param>
        /// <returns></returns>
        public static async Task ContactSupport(IOwinContext context, StorageManager storageManager)
        {
            StreamReader reader = new StreamReader(context.Request.Body);
            string bodyContent = reader.ReadToEnd();
            JObject root = JObject.Parse(bodyContent);


            // verify parameters
            String[] requiredParams = { "packageId", "packageVersion", "copyMe", "reason", "message", "fromUsername", "brand" };
            List<string> missingParams = ServiceHelper.VerifyRequiredParameters(root, requiredParams);
            if (missingParams.Count > 0)
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "ContactSupport FAIL: Insufficient parameters.");
                JArray missingParamsArr = new JArray(missingParams);
                errorObject.Add("missingParameters", missingParamsArr);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }


            // pull data
            string packageId = root.Value<string>("packageId");
            string packageVersion = root.Value<string>("packageVersion");
            bool copyMe = root.Value<bool>("copyMe");
            string reason = root.Value<string>("reason");
            string message = root.Value<string>("message");
            string fromUsername = root.Value<string>("fromUsername");
            string brand = root.Value<string>("brand");

            // TODO:  validate parameters

            IConstants brandValues = ServiceHelper.GetBrandConstants(brand);
            if (brandValues == null)
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "ContactSupport FAIL: " + brand + " is not a valid brand.  Options:  " + ServiceHelper.BrandsOptions());
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }
 
            string packageURL = brandValues.SiteRoot + "/packages/" + packageId;
            string versionURL = packageURL + "/" + packageVersion;
            string fromAddress = await ServiceHelper.GetUserEmailAddressFromUsername(fromUsername);

            
            string subject = String.Format(
                CultureInfo.CurrentCulture, 
                brandValues.ContactSupport_EmailSubject, 
                packageId, 
                packageVersion, 
                reason);
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




            // compose JSON
            JObject emailJSON = new JObject();
            emailJSON.Add("to", brandValues.SupportTeamEmail);
            emailJSON.Add("from", fromAddress);
            if (copyMe)
            {
                emailJSON.Add("cc", fromAddress);
            }
            emailJSON.Add("subject", subject);
            JObject body = new JObject();
            body.Add("text", bodyText);
            body.Add("html", bodyHTML);
            emailJSON.Add("body", body);


            
            // enqueue message
            bool result = await storageManager.Save(new StringStorageContent(emailJSON.ToString(), "application/json"), "email1");
            
            if (result)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync("ContactSupport OK");
                return;
            }
            else
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.ServiceUnavailable);
                errorObject.Add("description", "ContactSupport FAIL: Storage unavailable.  Email not sent.");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }
        }

        /// <summary>
        /// Create and format ConfirmOwnerInvite email, and then store it in an Azure Storage Blob.
        /// Use an AAD connection to obtain required information.
        /// </summary>
        /// <param name="context">Context's request body contains JSON file of parameters, passed from front-end.</param>
        /// <param name="storageManager">Use to store message.</param>
        /// <returns></returns>
        public static async Task InvitePackageOwner(IOwinContext context, StorageManager storageManager)
        {
            StreamReader reader = new StreamReader(context.Request.Body);
            string bodyContent = reader.ReadToEnd();
            JObject root = JObject.Parse(bodyContent);


            // verify parameters
            String[] requiredParams = { "packageId", "packageVersion", "message", "toUsername", "fromUsername", "brand" };
            List<string> missingParams = ServiceHelper.VerifyRequiredParameters(root, requiredParams);
            if (missingParams.Count > 0)
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "InvitePackageOwner FAIL: Insufficient parameters.");
                JArray missingParamsArr = new JArray(missingParams);
                errorObject.Add("missingParameters", missingParamsArr);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }


            // pull data
            string packageId = root.Value<string>("packageId");
            string packageVersion = root.Value<string>("packageVersion");
            string message = root.Value<string>("message");
            string toUsername = root.Value<string>("toUsername");
            string fromUsername = root.Value<string>("fromUsername");
            string brand = root.Value<string>("brand");

            // TODO:  validate parameters

            IConstants brandValues = ServiceHelper.GetBrandConstants(brand);
            if (brandValues == null)
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "InvitePackageOwner FAIL: " + brand + " is not a valid brand.  Options:  " + ServiceHelper.BrandsOptions());
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }


            string toAddress = await ServiceHelper.GetUserEmailAddressFromUsername(toUsername);
            string fromAddress = await ServiceHelper.GetUserEmailAddressFromUsername(fromUsername);

            string subject = String.Format(
                CultureInfo.CurrentCulture, 
                brandValues.InvitePackageOwner_EmailSubject, 
                fromUsername, 
                packageId);
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



            // compose JSON
            JObject emailJSON = new JObject();
            emailJSON.Add("to", toAddress);
            emailJSON.Add("from", fromAddress);
            emailJSON.Add("subject", subject);
            JObject body = new JObject();
            body.Add("text", bodyText);
            body.Add("html", bodyHTML);
            emailJSON.Add("body", body);


            
            // enqueue message
            bool result = await storageManager.Save(new StringStorageContent(emailJSON.ToString(), "application/json"), "email1");
            
            if (result)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync("InvitePackageOwner OK");
                return;
            }
            else
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.ServiceUnavailable);
                errorObject.Add("description", "InvitePackageOwner FAIL: Storage unavailable.  Email not sent.");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }
        }

        


        /// <summary>
        /// Obtain reasons for specified action, formatted using the brand's entity name.
        /// </summary>
        /// <param name="context">Use to send back response.</param>
        /// <param name="brand">The this brand to obtain entity.</param>
        /// <param name="action">The action we want reasons for.</param>
        /// <returns></returns>
        public static Task GetReasons(IOwinContext context, string brand, string action)
        {
            IConstants brandValues = ServiceHelper.GetBrandConstants(brand);
            String[] reasons = ServiceHelper.GetReasonsList(action);

            // replace entity with brand's entity name
            for (int i = 0; i < reasons.Length - 1; i++)
            {
                reasons[i] = String.Format(CultureInfo.CurrentCulture, reasons[i], brandValues.EntityName);
            }

            // format as JSON
            JObject reasonsJSON = new JObject();
            reasonsJSON.Add("reasons", new JArray(reasons));

            // return response with body containing JSON
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "application/json";
            context.Response.Headers.Add("Cache-Control", new string[] { "no-cache" });
            return context.Response.WriteAsync(reasonsJSON.ToString());
        }



        /// <summary>
        /// Create and format NewAccountWelcome email, and then store it in an Azure Storage Blob.
        /// Use an AAD connection to obtain required information.
        /// This email is only sent for NuGet users.
        /// </summary>
        /// <param name="context">Context's request body contains JSON file of parameters, passed from front-end.</param>
        /// <param name="_storageManager">Use to store message.</param>
        /// <returns></returns>
        public static async Task NewAccountWelcome(IOwinContext context, StorageManager storageManager)
        {
            StreamReader reader = new StreamReader(context.Request.Body);
            string bodyContent = reader.ReadToEnd();
            JObject root = JObject.Parse(bodyContent);


            // verify parameters
            String[] requiredParams = { "username", "email", "brand" };
            List<string> missingParams = ServiceHelper.VerifyRequiredParameters(root, requiredParams);
            if (missingParams.Count > 0)
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "NewAccountWelcome FAIL: Insufficient parameters.");
                JArray missingParamsArr = new JArray(missingParams);
                errorObject.Add("missingParameters", missingParamsArr);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }


            // pull data
            string username = root.Value<string>("username");
            string toAddress = root.Value<string>("email");
            string brand = root.Value<string>("brand");


            // TODO:  validate parameters


            // Always use NuGet constants, email only sent for NuGet users
            Brand.NuGet.Constants brandValues = (Brand.NuGet.Constants)ServiceHelper.GetBrandConstants(brand);
            if (brandValues == null)
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "NewAccountWelcome FAIL: " + brand + " is not a valid brand.  Options:  " + ServiceHelper.BrandsOptions());
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }


            string fromAddress = brandValues.SupportTeamEmail;

            string subject = String.Format(
                CultureInfo.CurrentCulture,
                brandValues.NewAccountWelcome_EmailSubject);
            string bodyText = String.Format(
                CultureInfo.CurrentCulture,
                brandValues.NewAccountWelcome_EmailBody_Text,
                brandValues.VerifyEmailURL);
            string bodyHTML = String.Format(
                CultureInfo.CurrentCulture,
                brandValues.NewAccountWelcome_EmailBody_HTML,
                brandValues.VerifyEmailURL);



            // compose JSON
            JObject emailJSON = new JObject();
            emailJSON.Add("to", toAddress);
            emailJSON.Add("from", fromAddress);
            emailJSON.Add("subject", subject);
            JObject body = new JObject();
            body.Add("text", bodyText);
            body.Add("html", bodyHTML);
            emailJSON.Add("body", body);



            // enqueue message
            bool result = await storageManager.Save(new StringStorageContent(emailJSON.ToString(), "application/json"), "email1");

            if (result)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync("NewAccountWelcome OK");
                return;
            }
            else
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.ServiceUnavailable);
                errorObject.Add("description", "NewAccountWelcome FAIL: Storage unavailable.  Email not sent.");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }
            
        }

        /// <summary>
        /// Create and format ChangeEmailNotice email, and then store it in an Azure Storage Blob.
        /// Use an AAD connection to obtain required information.
        /// This email is only sent for NuGet users.
        /// </summary>
        /// <param name="context">Context's request body contains JSON file of parameters, passed from front-end.</param>
        /// <param name="_storageManager">Use to store message.</param>
        /// <returns></returns>
        public static async Task ChangeEmailNotice(IOwinContext context, StorageManager storageManager)
        {
            StreamReader reader = new StreamReader(context.Request.Body);
            string bodyContent = reader.ReadToEnd();
            JObject root = JObject.Parse(bodyContent);


            // verify parameters
            String[] requiredParams = { "username", "oldEmail", "newEmail", "brand" };
            List<string> missingParams = ServiceHelper.VerifyRequiredParameters(root, requiredParams);
            if (missingParams.Count > 0)
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "ChangeEmailNotice FAIL: Insufficient parameters.");
                JArray missingParamsArr = new JArray(missingParams);
                errorObject.Add("missingParameters", missingParamsArr);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }


            // pull data
            string username = root.Value<string>("username");
            string oldAddress = root.Value<string>("oldEmail");
            string newAddress = root.Value<string>("newEmail");
            string brand = root.Value<string>("brand");


            // TODO:  validate parameters

            // Always use NuGet constants, email only sent for NuGet users
            Brand.NuGet.Constants brandValues = (Brand.NuGet.Constants)ServiceHelper.GetBrandConstants(brand);
            if (brandValues == null)
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "ChangeEmailNotice FAIL: " + brand + " is not a valid brand.  Options:  " + ServiceHelper.BrandsOptions());
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }


            string fromAddress = brandValues.SupportTeamEmail;


            // construct two emails:

            // old address email
            string subject_oldAddress = String.Format(
                CultureInfo.CurrentCulture,
                brandValues.ChangeEmailNotice_oldEmail_EmailSubject);
            string bodyText_oldAddress = String.Format(
                CultureInfo.CurrentCulture,
                brandValues.ChangeEmailNotice_oldEmail_EmailBody_Text,
                oldAddress,
                newAddress);
            string bodyHTML_oldAddress = String.Format(
                CultureInfo.CurrentCulture,
                brandValues.ChangeEmailNotice_oldEmail_EmailBody_HTML,
                oldAddress,
                newAddress);


            // new address email
            string subject_newAddress = String.Format(
                CultureInfo.CurrentCulture,
                brandValues.ChangeEmailNotice_newEmail_EmailSubject);
            string bodyText_newAddress = String.Format(
                CultureInfo.CurrentCulture,
                brandValues.ChangeEmailNotice_newEmail_EmailBody_Text,
                brandValues.VerifyEmailURL);
            string bodyHTML_newAddress = String.Format(
                CultureInfo.CurrentCulture,
                brandValues.ChangeEmailNotice_newEmail_EmailBody_HTML,
                brandValues.VerifyEmailURL);



            // compose JSON

            // old email message
            JObject oldAddress_emailJSON = new JObject();
            oldAddress_emailJSON.Add("to", oldAddress);
            oldAddress_emailJSON.Add("from", fromAddress);
            oldAddress_emailJSON.Add("subject", subject_oldAddress);
            JObject body_old = new JObject();
            body_old.Add("text", bodyText_oldAddress);
            body_old.Add("html", bodyHTML_oldAddress);
            oldAddress_emailJSON.Add("body", body_old);

            // new email message
            JObject newAddress_emailJSON = new JObject();
            newAddress_emailJSON.Add("to", newAddress);
            newAddress_emailJSON.Add("from", fromAddress);
            newAddress_emailJSON.Add("subject", subject_newAddress);
            JObject body_new = new JObject();
            body_new.Add("text", bodyText_newAddress);
            body_new.Add("html", bodyHTML_newAddress);
            newAddress_emailJSON.Add("body", body_new);



            // enqueue message
            bool result_oldEmail = await storageManager.Save(new StringStorageContent(oldAddress_emailJSON.ToString(), "application/json"), "email1");
            bool result_newEmail = await storageManager.Save(new StringStorageContent(newAddress_emailJSON.ToString(), "application/json"), "email2");

            if (result_oldEmail && result_newEmail)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync("ChangeEmailNotice OK");
                return;
            }
            else
            {
                if (result_oldEmail)
                {
                    // attempt rollback: delete saved old email
                    bool res = await storageManager.Delete("email1");
                }
                if (result_newEmail)
                {
                    // attempt rollback: delete saved new email
                    bool res = await storageManager.Delete("email2");
                }
                
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.ServiceUnavailable);
                errorObject.Add("description", "ChangeEmailNotice FAIL: Storage unavailable.  Emails not sent.");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }
            
        }

        /// <summary>
        /// Create and format ResetPasswordInstructions email, and then store it in an Azure Storage Blob.
        /// Use an AAD connection to obtain required information.
        /// This email is only sent for NuGet users.
        /// </summary>
        /// <param name="context">Context's request body contains JSON file of parameters, passed from front-end.</param>
        /// <param name="_storageManager">Use to store message.</param>
        /// <returns></returns>
        public static async Task ResetPasswordInstructions(IOwinContext context, StorageManager storageManager)
        {
            StreamReader reader = new StreamReader(context.Request.Body);
            string bodyContent = reader.ReadToEnd();
            JObject root = JObject.Parse(bodyContent);


            // verify parameters
            String[] requiredParams = { "username", "action", "brand" };
            List<string> missingParams = ServiceHelper.VerifyRequiredParameters(root, requiredParams);
            if (missingParams.Count > 0)
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "ResetPasswordInstructions FAIL: Insufficient parameters.");
                JArray missingParamsArr = new JArray(missingParams);
                errorObject.Add("missingParameters", missingParamsArr);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }


            // pull data
            string username = root.Value<string>("username");
            string action = root.Value<string>("action");
            string brand = root.Value<string>("brand");


            // TODO:  validate parameters


            // Always use NuGet constants, email only sent for NuGet users
            Brand.NuGet.Constants brandValues = (Brand.NuGet.Constants)ServiceHelper.GetBrandConstants(brand);
            if (brandValues == null)
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "ResetPasswordInstructions FAIL: " + brand + " is not a valid brand.  Options:  " + ServiceHelper.BrandsOptions());
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }



            string toAddress = await ServiceHelper.GetUserEmailAddressFromUsername(username);
            string fromAddress = brandValues.SupportTeamEmail;



            string subject;
            string bodyText;
            string bodyHTML;

            if (action.Equals("forgot"))
            {
                subject = String.Format(
                    CultureInfo.CurrentCulture,
                    brandValues.ResetPasswordInstructions_forgot_EmailSubject);
                bodyText = String.Format(
                    CultureInfo.CurrentCulture,
                    brandValues.ResetPasswordInstructions_forgot_EmailBody_Text,
                    brandValues.ResetPasswordExpirationTime,
                    brandValues.ResetPasswordURL);
                bodyHTML = String.Format(
                    CultureInfo.CurrentCulture,
                    brandValues.ResetPasswordInstructions_forgot_EmailBody_HTML,
                    brandValues.ResetPasswordExpirationTime,
                    brandValues.ResetPasswordURL);
            }
            else if (action.Equals("reset"))
            {
                subject = String.Format(
                    CultureInfo.CurrentCulture,
                    brandValues.ResetPasswordInstructions_reset_EmailSubject);
                bodyText = String.Format(
                    CultureInfo.CurrentCulture,
                    brandValues.ResetPasswordInstructions_reset_EmailBody_Text,
                    brandValues.ResetPasswordExpirationTime,
                    brandValues.ResetPasswordURL);
                bodyHTML = String.Format(
                    CultureInfo.CurrentCulture,
                    brandValues.ResetPasswordInstructions_reset_EmailBody_HTML,
                    brandValues.ResetPasswordExpirationTime,
                    brandValues.ResetPasswordURL);
            }
            else
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "ResetPasswordInstructions FAIL: 'action' was not a valid option.");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }




            // compose JSON
            JObject emailJSON = new JObject();
            emailJSON.Add("to", toAddress);
            emailJSON.Add("from", fromAddress);
            emailJSON.Add("subject", subject);
            JObject body = new JObject();
            body.Add("text", bodyText);
            body.Add("html", bodyHTML);
            emailJSON.Add("body", body);



            // enqueue message
            bool result = await storageManager.Save(new StringStorageContent(emailJSON.ToString(), "application/json"), "email1");

            if (result)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync("ResetPasswordInstructions OK");
                return;
            }
            else
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.ServiceUnavailable);
                errorObject.Add("description", "ResetPasswordInstructions FAIL: Storage unavailable.  Email not sent.");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }

        }

        /// <summary>
        /// Create and format EditCredential email, and then store it in an Azure Storage Blob.
        /// Use an AAD connection to obtain required information.
        /// This email is only sent for NuGet users.
        /// </summary>
        /// <param name="context">Context's request body contains JSON file of parameters, passed from front-end.</param>
        /// <param name="_storageManager">Use to store message.</param>
        /// <returns></returns>
        public static async Task EditCredential(IOwinContext context, StorageManager storageManager)
        {
            StreamReader reader = new StreamReader(context.Request.Body);
            string bodyContent = reader.ReadToEnd();
            JObject root = JObject.Parse(bodyContent);


            // verify parameters
            String[] requiredParams = { "username", "action", "brand" };
            List<string> missingParams = ServiceHelper.VerifyRequiredParameters(root, requiredParams);
            if (missingParams.Count > 0)
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "EditCredential FAIL: Insufficient parameters.");
                JArray missingParamsArr = new JArray(missingParams);
                errorObject.Add("missingParameters", missingParamsArr);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }


            // pull data
            string username = root.Value<string>("username");
            string action = root.Value<string>("action");
            string brand = root.Value<string>("brand");


            // TODO:  validate parameters



            // Always use NuGet constants, email only sent for NuGet users
            Brand.NuGet.Constants brandValues = (Brand.NuGet.Constants)ServiceHelper.GetBrandConstants(brand);
            if (brandValues == null)
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "EditCredential FAIL: " + brand + " is not a valid brand.  Options:  " + ServiceHelper.BrandsOptions());
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }



            string toAddress = await ServiceHelper.GetUserEmailAddressFromUsername(username);
            string fromAddress = brandValues.SupportTeamEmail;



            string subject;
            string bodyText;
            string bodyHTML;

            if (action.Equals("add"))
            {
                subject = String.Format(
                    CultureInfo.CurrentCulture,
                    brandValues.EditCredential_add_EmailSubject,
                    "Some credential?");
                bodyText = String.Format(
                    CultureInfo.CurrentCulture,
                    brandValues.EditCredential_add_EmailBody_Text,
                    "Some credential?");
                bodyHTML = String.Format(
                    CultureInfo.CurrentCulture,
                    brandValues.EditCredential_add_EmailBody_HTML,
                    "Some credential?");
            }
            else if (action.Equals("remove"))
            {
                subject = String.Format(
                    CultureInfo.CurrentCulture,
                    brandValues.EditCredential_remove_EmailSubject,
                    "Some credential?");
                bodyText = String.Format(
                    CultureInfo.CurrentCulture,
                    brandValues.EditCredential_remove_EmailBody_Text,
                    "Some credential?");
                bodyHTML = String.Format(
                    CultureInfo.CurrentCulture,
                    brandValues.EditCredential_remove_EmailBody_HTML,
                    "Some credential?");
            }
            else
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.BadRequest);
                errorObject.Add("description", "EditCredential FAIL: 'action' was not a valid option.");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }




            // compose JSON
            JObject emailJSON = new JObject();
            emailJSON.Add("to", toAddress);
            emailJSON.Add("from", fromAddress);
            emailJSON.Add("subject", subject);
            JObject body = new JObject();
            body.Add("text", bodyText);
            body.Add("html", bodyHTML);
            emailJSON.Add("body", body);



            // enqueue message
            bool result = await storageManager.Save(new StringStorageContent(emailJSON.ToString(), "application/json"), "email1");

            if (result)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync("EditCredential OK");
                return;
            }
            else
            {
                JObject errorObject = new JObject();
                errorObject.Add("error", (int)HttpStatusCode.ServiceUnavailable);
                errorObject.Add("description", "EditCredential FAIL: Storage unavailable.  Email not sent.");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                await context.Response.WriteAsync(errorObject.ToString());
                return;
            }
            
        }
    }
}