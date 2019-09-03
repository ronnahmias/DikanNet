using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEntities
{
    [Table("DisciplineCommittee")]
    public class DisciplineCommittee
    {
        [Key]
        public int CommitteeId { get; set; }

        [Display(Name ="תאריך ועדת משמעת")]
        [Required(ErrorMessage = "חובה להזין תאריך ועדת משמעת")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true)]
        public DateTime CommitteeDate { get; set; }

        [Display(Name = "מגמה")]
        [Required(ErrorMessage = "חובה לבחור מגמה")]
        public string HeadMajorId { get; set; }

        [Display(Name = "תעודת זהות סטודנט")]
        [MinLength(9),MaxLength(9)]
        [Required(ErrorMessage = "חובה להזין תעודת זהות סטודנט")]
        public string StudentId { get; set; }

        [Display(Name = "שם פרטי סטודנט")]
        [Required(ErrorMessage = "חובה להזין שם פרטי סטודנט")]
        public string StudentFirstName { get; set; }

        [Display(Name = "שם משפחה סטודנט")]
        [Required(ErrorMessage = "חובה להזין שם משפחה סטודנט")]
        public string StudentLastName { get; set; }

        [Display(Name = "אימייל סטודנט")]
        [Required(ErrorMessage = "חובה להזין מייל סטודנט")]
        public string StudentMail { get; set; }

        [Display(Name = "כותרת ועדה")]
        [Required(ErrorMessage = "חובה להזין כותרת ועדה")]
        public string CommitteeType { get; set; }

        [Display(Name = "סיכום ועדה")]
        public string CommitteeSummary { get; set; }

        // forgein key
        public HeadMajor HeadMajor { get; set; }
    }
}
