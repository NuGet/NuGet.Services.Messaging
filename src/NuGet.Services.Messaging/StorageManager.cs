using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using NuGet.Services.Metadata.Catalog.Persistence;
using Microsoft.WindowsAzure.Storage;
using System.Threading.Tasks;

namespace NuGet.Services.Messaging
{

    /*
     * The StorageManager encapsulates the storage decision.
     * To the outside, all actions are the same.  All switches are internal.
     * Inside, we decided where to create storage based on web.config values.
     * All actions default to Azure, unless Azure values are empty, 
     *      in which case it falls-back to file storage locally.
     */

    public class StorageManager
    {
        private Storage _storage;

        // Azure Storage
        static string azureStorage = ConfigurationManager.AppSettings["Storage.Primary"];
        static string azureConnectionString = ConfigurationManager.AppSettings["Storage.Primary.ConnectionString"];
        static string azureContainer = ConfigurationManager.AppSettings["Storage.Container.Messages"];

        // File Storage
        static string fileStorage = ConfigurationManager.AppSettings["Storage.Secondary"];
        static string fileAddress = ConfigurationManager.AppSettings["Storage.Directory.BaseAddress"];
        static string filePath = ConfigurationManager.AppSettings["Storage.Directory.Path"];



        public StorageManager(Storage storage = null) 
        { 
            if (storage != null)
            {
                _storage = storage;
            }

            else if (!String.IsNullOrEmpty(azureStorage))
            {
                // if azure, create azure storage
                CloudStorageAccount account = CloudStorageAccount.Parse(azureConnectionString);
                _storage = new AzureStorage(account, azureContainer);
            }
            else
            {
                // if file system values, find file storage (create if doesn't exist)
                _storage = new FileStorage(fileAddress, filePath);
            }
        }




        public async Task<bool> Save(StorageContent content, String contentName)
        {
            try
            {
                // This will work for both types of storage
                await _storage.Save(new Uri(_storage.BaseAddress, contentName), content);
            }
            catch
            {
                // something went wrong with save
                return false;
            }
            return true;
        }




        public async Task<StorageContent> Load(String contentName)
        {
            StorageContent content;
            try
            {
                content = await _storage.Load(new Uri(_storage.BaseAddress, contentName));
            }
            catch
            {
                content = null;
            }
            return content;
        }


       
        public async Task<bool> Delete(String contentName)
        {
            try
            {
                await _storage.Delete(new Uri(_storage.BaseAddress, contentName));
            }
            catch
            {
                return false;
            }
            
            return true;
        }



    }
}