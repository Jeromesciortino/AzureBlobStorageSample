using System;
using System.Web;
using System.Web.Mvc;
using AzureStorageSample.Business;

namespace AzureStorageSample.Controllers
{
    public class ClientSdkController : Controller
    {
        readonly IBlobActions blobActions = new ClientSdkBlobActions();

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

        [HttpGet]
        public ActionResult DownloadAll()
        {
            IBlobActions blobActions = new ClientSdkBlobActions();
            var res = blobActions.DownloadAllBlobs(blobActions.List());

            return File(res.BlobStream, res.ContentType, res.BlobName);
        }

        [HttpGet]
        public ActionResult TestDownloadAll()
        {
            IBlobActions blobActions = new ClientSdkBlobActions();
            var res = blobActions.TestDownloadAllBlobs(blobActions.List());

            ViewBag.DownloadInfo = $"DOWNLOADED {res.DownloadedBlobsSizes.Count} FILE(s)\nSIZE(s): {String.Join(", ", res.DownloadedBlobsSizes)} bytes\nDURATION: {res.DownloadDuration}";

            return View("Index");
        }
    }
}