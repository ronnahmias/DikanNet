using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DikanNetProject.Controllers
{
    public class FileController : Controller
    {
        // GET: File
        public FileResult GetFile()
        {
            return File(Path.Combine(Server.MapPath("~/App_Data/"),"id.jpg"), MimeMapping.GetMimeMapping("id.jpg"), "id.jpg");

        }
    }
}