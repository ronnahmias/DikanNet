using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace DataEntities
{
    [Table("Funding")]
    public class Funding
    {
        [Key]
        public int FundingId { get; set; }
        public string StudentId { get; set; }

        [Display(Name ="גוף מממן"),Required(ErrorMessage ="חובה להזין גוף מממן")]
        public string FinancingInstitution { get; set; }

        [Display(Name = "שנת מימון"), Required(ErrorMessage = "חובה לבחור שנת מימון")]
        public int? YearFinancing { get; set; }

        [Display(Name = "גובה מימון"), Required(ErrorMessage = "חובה להזין גובה מימון")]
        public int? FinancingHeight { get; set; }

        [Display(Name = "קובץ שמור")]
        public string PathFunding { get; set; }

        [NotMapped, Display(Name = "קובץ מימון")]
        public HttpPostedFileBase FileFunding { get; set; }

        public virtual Student Student { get; set; }
    }
}
