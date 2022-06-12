using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web;

namespace AzureStorageSample.Business
{
    public class ClientSdkBlobActions : IBlobActions
    {
        private readonly string BlobStorageConnectionString = ConfigurationManager.AppSettings["BlobStorageConnectionString"];
        private readonly string BlobStorageAccountName = ConfigurationManager.AppSettings["BlobStorageAccountName"];
        private readonly string BlobStorageAccountKey = ConfigurationManager.AppSettings["BlobStorageAccountKey"];
        private readonly string BlobStorageContainerName = ConfigurationManager.AppSettings["BlobStorageContainerName"];

        public void Upload(HttpPostedFileBase postedFile)
        {
            string blobStorageConnectionString = String.Format(BlobStorageConnectionString, BlobStorageAccountName, BlobStorageAccountKey);

            var containerClient = new BlobContainerClient(blobStorageConnectionString, BlobStorageContainerName);
            var blobClient = containerClient.GetBlobClient(postedFile.FileName);

            blobClient.Upload(postedFile.InputStream);
        }
        public List<string> List()
        {
            string blobStorageConnectionString = String.Format(BlobStorageConnectionString, BlobStorageAccountName, BlobStorageAccountKey);

            var containerClient = new BlobContainerClient(blobStorageConnectionString, BlobStorageContainerName);

            var blobList = new List<string>();

            foreach (var blobItem in containerClient.GetBlobs())
            {
                blobList.Add(blobItem.Name);
            }

            return blobList;
        }

        public Models.FileResult.FileResult Download(string blobName)
        {
            string blobStorageConnectionString = String.Format(BlobStorageConnectionString, BlobStorageAccountName, BlobStorageAccountKey);

            var containerClient = new BlobContainerClient(blobStorageConnectionString, BlobStorageContainerName);

            if (!containerClient.Exists())
            {
                return new Models.FileResult.FileResult("Container does not exist");
            }

            var blobClient = containerClient.GetBlobClient(blobName);

            if (!blobClient.Exists())
            {
                return new Models.FileResult.FileResult("File does not exist");
            }

            MemoryStream ms = new MemoryStream();
            blobClient.DownloadToAsync(ms);

            Stream blobStream = blobClient.OpenReadAsync().Result;

            return new Models.FileResult.FileResult(blobStream, blobClient.GetProperties().Value.ContentType, blobName);
        }
    }
}