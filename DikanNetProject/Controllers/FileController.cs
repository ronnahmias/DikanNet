using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DikanNetProject.Controllers
{
    [RequireHttps]
    public class FileController : Controller
    {
        [Authorize]
        [HttpPost]
        public ActionResult GetFile(string pFilePath)
        {
            //return File(Path.Combine(Server.MapPath("~/App_Data/UsersFiles/"), User.Identity.Name, pFilePath), MimeMapping.GetMimeMapping(pFilePath), pFilePath);

            string path = Path.Combine(Server.MapPath("~/App_Data/UsersFiles/"), User.Identity.Name, pFilePath);
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);

            string contentType = MimeMapping.GetMimeMapping(path);

            var result = new FileStreamResult(stream, contentType);

            return result;
        }
    }
}