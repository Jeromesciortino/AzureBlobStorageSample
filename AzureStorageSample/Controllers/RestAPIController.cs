using System;
using System.Web;
using System.Web.Mvc;
using AzureStorageSample.Business;

namespace AzureStorageSample.Controllers
{
    public class RestAPIController : Controller
    {
        readonly IBlobActions blobActions = new RestAPIBlobActions();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase postedFile)
        {
            if (postedFile == null || String.IsNullOrEmpty(postedFile.FileName))
            {
                return View();
            }

            blobActions.Upload(postedFile);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult DownloadBlob(string blobName)
        {
            var res = blobActions.Download(blobName);

            if (String.IsNullOrEmpty(res.Error)) return File(res.BlobStream, res.ContentType, res.BlobName);

            return RedirectToAction("Index");
        }
    }
}