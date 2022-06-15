using System;
using System.IO;
using System.Collections.Generic;

namespace AzureStorageSample.Models.FileResult
{
    public class FileResult
    {
        public FileResult(Stream blobStream, string contentType, string blobName, TimeSpan? downloadDuration = null, List<string> downloadedBlobsSizes = null)
        {
            BlobStream = blobStream;
            ContentType = contentType;
            BlobName = blobName;
            DownloadDuration = downloadDuration;
            DownloadedBlobsSizes = downloadedBlobsSizes;
        }

        public FileResult(string error)
        {
            Error = error;
        }

        public Stream BlobStream
        {
            get; set;
        }

        public string ContentType
        {
            get; set;
        }

        public string BlobName
        {
            get; set;
        }

        public string Error
        {
            get; set;
        }

        public TimeSpan? DownloadDuration { get; set; }

        public List<string> DownloadedBlobsSizes { get; set; }
    }
}