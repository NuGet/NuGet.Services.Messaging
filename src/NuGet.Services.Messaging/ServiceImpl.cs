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


            IConstants brandValues = ServiceHelper.GetBrandConstants(brand);
            

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


            IConstants brandValues = ServiceHelper.GetBrandConstants(brand);

 
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

            IConstants brandValues = ServiceHelper.GetBrandConstants(brand);

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

    }
}