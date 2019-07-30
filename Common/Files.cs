using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Common
{
    public static class Files
    {
        public static string SaveFileInServer(HttpPostedFileBase pFile, string pFileName, string pId,string pOldFile)
        {
            int fileSize = pFile.ContentLength;
            var fileExt = Path.GetExtension(pFile.FileName);
            var serverpathsave = Path.Combine(HttpContext.Current.Server.MapPath("~/UsersFiles/") + pId);
            if (!Directory.Exists(serverpathsave))
                Directory.CreateDirectory(serverpathsave);
            if (!string.IsNullOrEmpty(pOldFile))
                File.Delete(Path.Combine(serverpathsave ,pOldFile));
            pFile.SaveAs(Path.Combine(serverpathsave, pFileName + fileExt)); // save file to server                                                                      
            return pFileName+fileExt;
        }

        /* The function get a file name and id of student
         * and delete the file from the server database
         */
        public static bool Delete(string pFileName, string pId)
        {
            if (string.IsNullOrEmpty(pFileName) || string.IsNullOrEmpty(pId))
                return false;

            var serverpath = Path.Combine(HttpContext.Current.Server.MapPath("~/UsersFiles/") + pId);
            if (!Directory.Exists(serverpath)) return false;

            File.Delete(Path.Combine(serverpath, pFileName));
            return true;
        }
    }
}
