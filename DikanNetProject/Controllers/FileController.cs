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
        [HttpGet]
        [Authorize]
        public FileResult GetFile(string pFilePath) // return file acording to file path and user name
        {
            string path = Path.Combine(Server.MapPath("~/App_Data/UsersFiles/"), User.Identity.Name, pFilePath); // path of the file
            string contentType = MimeMapping.GetMimeMapping(path); // type of file
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            return File(fs, contentType); // return the file
        }
    }
}