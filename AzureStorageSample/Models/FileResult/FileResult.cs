using System.IO;

namespace AzureStorageSample.Models.FileResult
{
    public class FileResult
    {
        public FileResult(Stream blobStream, string contentType, string blobName)
        {
            BlobStream = blobStream;
            ContentType = contentType;
            BlobName = blobName;
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
    }
}