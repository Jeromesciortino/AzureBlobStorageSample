using System;
using System.IO;
using System.Web;
using System.Threading;
using System.Diagnostics;
using Azure.Storage.Blobs;
using System.Configuration;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        Models.FileResult.FileResult IBlobActions.DownloadAllBlobs(List<string> blobNames)
        {
            string blobStorageConnectionString = String.Format(BlobStorageConnectionString, BlobStorageAccountName, BlobStorageAccountKey);

            var containerClient = new BlobContainerClient(blobStorageConnectionString, BlobStorageContainerName);

            var semaphore = new SemaphoreSlim(100);

            List<Task> downloadTasks = new List<Task>();
            List<Models.FileResult.FileResult> fileResults = new List<Models.FileResult.FileResult>();

            List<string> downloadedBlobsSizes = new List<string>();

            foreach (var blobName in blobNames)
            {
                semaphore.Wait();

                downloadTasks.Add(Task.Run(() =>
                {
                    var blobClient = containerClient.GetBlobClient(blobName);

                    MemoryStream ms = new MemoryStream();
                    blobClient.DownloadToAsync(ms);

                    Stream blobStream = blobClient.OpenReadAsync().Result;
                    downloadedBlobsSizes.Add($"({System.Text.Encoding.UTF8.GetByteCount(blobName)})");

                    fileResults.Add(new Models.FileResult.FileResult(blobStream, blobClient.GetProperties().Value.ContentType, blobName));

                    semaphore.Release();
                }));
            }

            var elapsed = TimeAction(() =>
            {
                Task.WhenAll(downloadTasks).Wait();
            });

            var memoryStream = new MemoryStream();

            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var file in fileResults)
                {
                    string filename = file.BlobName;

                    var entry = archive.CreateEntry(filename);

                    using (var entryStream = entry.Open())
                    {
                        file.BlobStream.CopyTo(entryStream);
                    }
                }
            }

            memoryStream.Position = 0;

            return new Models.FileResult.FileResult(memoryStream, "application/zip", $"{DateTime.Now}.zip", elapsed, downloadedBlobsSizes);
        }

        Models.FileResult.FileResult IBlobActions.TestDownloadAllBlobs(List<string> blobNames)
        {
            string blobStorageConnectionString = String.Format(BlobStorageConnectionString, BlobStorageAccountName, BlobStorageAccountKey);

            var containerClient = new BlobContainerClient(blobStorageConnectionString, BlobStorageContainerName);

            var semaphore = new SemaphoreSlim(100);

            List<Task> downloadTasks = new List<Task>();
            List<Models.FileResult.FileResult> fileResults = new List<Models.FileResult.FileResult>();

            List<string> downloadedBlobsSizes = new List<string>();

            foreach (var blobName in blobNames)
            {
                semaphore.Wait();

                downloadTasks.Add(Task.Run(() =>
                {
                    var blobClient = containerClient.GetBlobClient(blobName);

                    MemoryStream ms = new MemoryStream();
                    blobClient.DownloadToAsync(ms);

                    Stream blobStream = blobClient.OpenReadAsync().Result;
                    downloadedBlobsSizes.Add($"({blobStream.Length})");

                    fileResults.Add(new Models.FileResult.FileResult(blobStream, blobClient.GetProperties().Value.ContentType, blobName));

                    semaphore.Release();
                }));
            }

            var elapsed = TimeAction(() =>
            {
                Task.WhenAll(downloadTasks).Wait();
            });

            return new Models.FileResult.FileResult(null, null, null, elapsed, downloadedBlobsSizes);
        }

        TimeSpan TimeAction(Action blockingAction)
        {
            Stopwatch stopWatch = Stopwatch.StartNew();
            blockingAction();
            stopWatch.Stop();
            return stopWatch.Elapsed;
        }
    }
}