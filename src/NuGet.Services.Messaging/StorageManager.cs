using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using NuGet.Services.Metadata.Catalog.Persistence;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;
using System.IO;
using System.Text;

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
        private string _storageType;    // options:  file, azure

        // Azure Storage
        private static string _azureConnectionString = ConfigurationManager.AppSettings["Storage.Primary.ConnectionString"];
        private static string _azureContainerName = ConfigurationManager.AppSettings["Storage.Primary.ContainerName"];
        private static string _azureContainerURI = ConfigurationManager.AppSettings["Storage.Primary.ContainerURI"];


        // File Storage
        private static string _fileStorage = ConfigurationManager.AppSettings["Storage.Secondary"];
        private static string _fileAddress = ConfigurationManager.AppSettings["Storage.Secondary.BaseAddress"];
        private static string _filePath = ConfigurationManager.AppSettings["Storage.Secondary.Path"];
        private static string _fileQueuePath = ConfigurationManager.AppSettings["Storage.Secondary.Queue"];



        public StorageManager(Storage storage = null, string storageType = "file") 
        { 
            if (storage != null)
            {
                _storage = storage;
                _storageType = storageType;
            }

            else if (!String.IsNullOrEmpty(_azureConnectionString))
            {
                _storageType = "azure";
                // create azure storage
                CloudStorageAccount account = CloudStorageAccount.Parse(_azureConnectionString);
                _storage = new AzureStorage(account, _azureContainerName);
            }
            else
            {
                _storageType = "file";
                _storage = new FileStorage(_fileAddress, _filePath);
                
                // create file queue
                if (!File.Exists(_fileQueuePath))
                {
                    using (StreamWriter sw = File.CreateText(_fileQueuePath))
                    {
                        sw.WriteLineAsync("Queue");
                        sw.WriteLineAsync("======");
                    }
                }
            }
        }

        public async Task<bool> Save(StorageContent content, String contentName)
        {
            try
            {
                if (_storageType.Equals("file"))
                {
                    // add contentName to fileQueue
                    AddContentName(contentName);
                }

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
                if (_storageType.Equals("file"))
                {
                    // remove contentName from fileQueue
                    RemoveContentName(contentName);
                }

                await _storage.Delete(new Uri(_storage.BaseAddress, contentName));
            }
            catch
            {
                return false;
            }
            
            return true;
        }


        // Add content name to file queue
        private async Task AddContentName(String contentName)
        {
            if (File.Exists(_fileQueuePath))
            {
                using (StreamWriter sw = File.AppendText(_fileQueuePath))
                {
                    await sw.WriteLineAsync(contentName);
                }
            }
        }


        // Remove content name from file queue
        private async Task RemoveContentName(String contentName)
        {
            if (File.Exists(_fileQueuePath))
            {
                StringBuilder sb = new StringBuilder();
                
                // generate new file content
                using (StreamReader sr = File.OpenText(_fileQueuePath))
                {
                    string line = await sr.ReadLineAsync();
                    while (line != null)
                    {
                        if (line != contentName)
                        {
                            sb.AppendLine(line);
                        }
                        line = await sr.ReadLineAsync();
                    }
                }
                
                // write new content to file
                using (StreamWriter sw = new StreamWriter(_fileQueuePath))
                {
                    await sw.WriteAsync(sb.ToString());
                }
            }
        }
    }
}