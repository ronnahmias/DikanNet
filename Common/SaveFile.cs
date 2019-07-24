using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Common
{
    public static class SaveFile
    {
        public static string SaveFileInServer(HttpPostedFileBase pFile, string pFileName, string pId,string pOldFile)
        {
           
            var fileExt = Path.GetExtension(pFile.FileName);
            var serverpathsave = Path.Combine(HttpContext.Current.Server.MapPath("~/UsersFiles/") + pId);
            if (!Directory.Exists(serverpathsave))
                Directory.CreateDirectory(serverpathsave);
            if (!string.IsNullOrEmpty(pOldFile))
                File.Delete(Path.Combine(serverpathsave ,pOldFile));
            pFile.SaveAs(Path.Combine(serverpathsave, pFileName + fileExt)); // save file to server                                                                      
            return pFileName+fileExt;
        }
    }
}
