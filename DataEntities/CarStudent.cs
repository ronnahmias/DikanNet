using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace DataEntities
{
    [Table("CarStudent")]
    public class CarStudent
    {
        [Key, Display(Name ="מספר רכב"),Required(ErrorMessage ="חובה להזין מספר רכב - ללא תווים")]
        public int CarNumber { get; set; }

        public string StudentId { get; set; }

        [Display(Name ="שם יצרן"),Required(ErrorMessage ="חובה להזין יצרן רכב")]
        public string CarCompany { get; set; }

        [Display(Name = "דגם רכב"), Required(ErrorMessage = "חובה להזין דגם רכב")]
        public string CarModel { get; set; }

        [NotMapped,Display(Name ="קובץ - רישיון/ אחזקת רכב")]
        public HttpPostedFileBase CarLicenseFile { get; set; }

        public string FileCarLicense { get; set; }

        [Display(Name = "שנת ייצור"), Required(ErrorMessage = "חובה להזין שנת ייצור")]
        public int CarYear { get; set; }

        [Display(Name = "הערות")]
        public string CarComment { get; set; }

        [NotMapped]
        public virtual Student Student { get; set; }
    }
}
