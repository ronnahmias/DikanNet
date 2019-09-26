﻿using System;
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
            if (string.IsNullOrEmpty(pFilePath)) return null;
            string path = Path.Combine(Server.MapPath("~/App_Data/UsersFiles/"), User.Identity.Name, pFilePath); // path of the file
            string contentType = MimeMapping.GetMimeMapping(path); // type of file
            //FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] filedata = System.IO.File.ReadAllBytes(path);
            if (filedata == null)
                return null;
            return File(filedata, contentType); // return the file
        }
    }
}