using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace AzureStorageSample.Business
{
    public interface IBlobActions
    {
        void Upload(HttpPostedFileBase postedFile);

        List<string> List();

        Models.FileResult.FileResult Download(string blobName);
    }
}