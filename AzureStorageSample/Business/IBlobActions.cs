using System.Web;
using System.Collections.Generic;

namespace AzureStorageSample.Business
{
    public interface IBlobActions
    {
        void Upload(HttpPostedFileBase postedFile);

        List<string> List();

        Models.FileResult.FileResult Download(string blobName);

        Models.FileResult.FileResult TestDownloadAllBlobs(List<string> blobNames);

        Models.FileResult.FileResult DownloadAllBlobs(List<string> blobNames);
    }
}