using AzureStorageSample.Models.XMLModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;

namespace AzureStorageSample.Business
{
    public class RestAPIBlobActions : IBlobActions
    {
        private readonly string BlobStorageAccountName = ConfigurationManager.AppSettings["BlobStorageAccountName"];
        private readonly string BlobStorageAccountKey = ConfigurationManager.AppSettings["BlobStorageAccountKey"];
        private readonly string BlobStorageContainerName = ConfigurationManager.AppSettings["BlobStorageContainerName"];
        private const string StorageServiceVersion = "2012-02-12";

        public void Upload(HttpPostedFileBase postedFile)
        {
            string storageServiceVersion = StorageServiceVersion;

            string dateInRfc1123Format = DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture);
            string blobType = "BlockBlob";

            string canonicalizedHeaders = String.Format("x-ms-blob-type:{0}\nx-ms-date:{1}\nx-ms-version:{2}",
                    blobType,
                    dateInRfc1123Format,
                    storageServiceVersion);

            string blobName = Uri.EscapeUriString(postedFile.FileName);
            string urlPath = String.Format("{0}/{1}", BlobStorageContainerName, blobName);

            string canonicalizedResource = String.Format("/{0}/{1}", "exteststoragejs", urlPath);

            int blobLength = postedFile.ContentLength;
            string blobContentType = postedFile.ContentType;

            string stringToSign = String.Format(
                    "{0}\n\n\n{1}\n\n{2}\n\n\n\n\n\n\n{3}\n{4}",
                    "PUT",
                    blobLength,
                    blobContentType,
                    canonicalizedHeaders,
                    canonicalizedResource);

            string authorizationHeader = CreateAuthorizationHeader(stringToSign);

            Uri uri = new Uri($"https://{BlobStorageAccountName}.blob.core.windows.net/" + urlPath);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "PUT";
            request.Headers.Add("x-ms-blob-type", blobType);
            request.Headers.Add("x-ms-date", dateInRfc1123Format);
            request.Headers.Add("x-ms-version", storageServiceVersion);
            request.Headers.Add("Authorization", authorizationHeader);
            request.ContentLength = blobLength;
            request.ContentType = blobContentType;

            // Read postedFile content
            BinaryReader b = new BinaryReader(postedFile.InputStream);
            byte[] blobContent = b.ReadBytes(blobLength);

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(blobContent, 0, blobLength);
            }

            request.GetResponse();
        }

        public List<string> List()
        {
            var dateInRfc1123Format = DateTime.UtcNow.ToString("R");

            string StringToSign = String.Format("GET\n"
                + "\n" // content encoding
                + "\n" // content language
                + "\n" // content length
                + "\n" // content md5
                + "\n" // content type
                + "\n" // date
                + "\n" // if modified since
                + "\n" // if match
                + "\n" // if none match
                + "\n" // if unmodified since
                + "\n" // range
                + "x-ms-date:" + dateInRfc1123Format + $"\nx-ms-version:{StorageServiceVersion}\n" // headers
                + "/{0}/{1}\ncomp:list\nrestype:container", BlobStorageAccountName, BlobStorageContainerName);

            string authorizationHeader = CreateAuthorizationHeader(StringToSign);

            Uri uri = new Uri(string.Format("https://{0}.blob.core.windows.net/{1}?restype=container&comp=list", BlobStorageAccountName, BlobStorageContainerName));
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";
            request.Headers.Add("x-ms-date", dateInRfc1123Format);
            request.Headers.Add("x-ms-version", StorageServiceVersion);
            request.Headers.Add("Authorization", authorizationHeader);

            XmlSerializer serializer = new XmlSerializer(typeof(EnumerationResults));

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                EnumerationResults e = (EnumerationResults)serializer.Deserialize(response.GetResponseStream());
                return e.Blobs.Blob.Select(x => x.Name).ToList();
            }
        }

        public Models.FileResult.FileResult Download(string blobName)
        {
            var dateInRfc1123Format = DateTime.UtcNow.ToString("R");

            string blobNameEscaped = Uri.EscapeUriString(blobName);

            string stringToSign = String.Format("GET\n"
                + "\n" // content encoding
                + "\n" // content language
                + "\n" // content length
                + "\n" // content md5
                + "\n" // content type
                + "\n" // date
                + "\n" // if modified since
                + "\n" // if match
                + "\n" // if none match
                + "\n" // if unmodified since
                + "\n" // range
                + "x-ms-date:" + dateInRfc1123Format + $"\nx-ms-version:{StorageServiceVersion}\n" // headers
                + "/{0}/{1}/{2}", BlobStorageAccountName, BlobStorageContainerName, blobNameEscaped);

            string authorizationHeader = CreateAuthorizationHeader(stringToSign);

            Uri uri = new Uri(string.Format("https://{0}.blob.core.windows.net/{1}/{2}", BlobStorageAccountName, BlobStorageContainerName, blobNameEscaped));
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";
            request.Headers.Add("x-ms-date", dateInRfc1123Format);
            request.Headers.Add("x-ms-version", StorageServiceVersion);
            request.Headers.Add("Authorization", authorizationHeader);

            var response = request.GetResponse();
            return new Models.FileResult.FileResult(response.GetResponseStream(), response.ContentType, blobNameEscaped);
        }

        private String CreateAuthorizationHeader(string canonicalizedString)
        {
            string signature = String.Empty;

            using (HMACSHA256 hmacSha256 = new HMACSHA256(Convert.FromBase64String(BlobStorageAccountKey)))
            {
                Byte[] dataToHmac = Encoding.UTF8.GetBytes(canonicalizedString);
                signature = Convert.ToBase64String(hmacSha256.ComputeHash(dataToHmac));
            }

            String authorizationHeader = String.Format(
                CultureInfo.InvariantCulture,
                "{0} {1}:{2}",
                "SharedKey",
                BlobStorageAccountName,
                signature
            );

            return authorizationHeader;
        }
    }
}