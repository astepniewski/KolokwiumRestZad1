using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Biblioteka2;
using System.Diagnostics;
using Biblioteka3;

namespace REST.Controllers
{
    public class Bib2Controller : ApiController
    {
        public HttpPostedFileBase FileUser { get; private set; }
        public static string PathFile { get; private set; }

        [System.Web.Http.HttpPost]
        public void UploadFile()
        {
            var file = HttpContext.Current.Request.Files.Count > 0 ?
                HttpContext.Current.Request.Files[0] : null;

            var fileName = Path.GetFileName(file.FileName);

            PathFile = Path.Combine(
                HttpContext.Current.Server.MapPath("~/uploads"),
                fileName
            );

            file.SaveAs(PathFile);
        }
        public HttpResponseMessage GetAES(string option)
        {
            Encryption encryption = new Encryption();

            if (option == "encrypt")
            {
                encryption.EncryptFile(PathFile, PathFile + "enc", "password");
            }
            else if (option == "decrypt")
            {
                encryption.DecryptFile(PathFile + "enc", PathFile, "password");

            }

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(PathFile + "enc", FileMode.Open);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            return result;
        }
        [System.Web.Http.HttpGet]
        public byte[] GetSum(string optionSum)
        {
            ControlSum controlSum = new ControlSum();
            if (optionSum == "MD5")
            {
                return controlSum.MD5SUm(PathFile + "enc");
            }
            else
                return controlSum.SHASUM(PathFile + "enc");
        }


    }
}
