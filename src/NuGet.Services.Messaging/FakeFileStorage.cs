using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NuGet.Services.Metadata.Catalog.Persistence;
using System.IO;
using System.Threading.Tasks;

namespace NuGet.Services.Messaging
{
    public class FakeFileStorage : Storage
    {
        public FakeFileStorage(string baseAddress, string path)
            : this(new Uri(baseAddress), path) { }

        public FakeFileStorage(Uri baseAddress, string path) 
            : base(baseAddress)
        {
            Path = path;
            // don't validate path
            ResetStatistics();
        }

        public string Path
        { 
            get;
            set;
        }

        //  save

        protected override Task OnSave(Uri resourceUri, StorageContent content)
        {
            
            TraceMethod("SAVE", resourceUri);
            
            // Don't save.  Instead, return throw an error. This will cause a connection failure result in storage manager
            throw new DirectoryNotFoundException("Connection Error.");
        }

        //  load

        protected override Task<StorageContent> OnLoad(Uri resourceUri)
        {
            // not used
            return Task.Run(() => { return (StorageContent)(new StringStorageContent("")); });
        }

        //  delete

        protected override Task OnDelete(Uri resourceUri)
        {
            // not used
            return Task.Run(() => { return; }); ;
        }
    }
}