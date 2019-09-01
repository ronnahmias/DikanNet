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
        public DateTime CommitteeDate { get; set; }

        [Display(Name = "ראש מגמה")]
        public string HeadMajorId { get; set; }

        [Display(Name = "תעודת זהות סטודנט")]
        public string StudentId { get; set; }

        [Display(Name = "שם פרטי סטודנט")]
        public string StudentFirstName { get; set; }

        [Display(Name = "שם משפחה סטודנט")]
        public string StudentLastName { get; set; }

        [Display(Name = "אימייל סטודנט")]
        public string StudentMail { get; set; }

        [Display(Name = "אימייל סטודנט")]
        public string CommitteeType { get; set; }

        [Display(Name = "אימייל סטודנט")]
        public string CommitteeSummary { get; set; }

        // forgein key
        public HeadMajor HeadMajor { get; set; }
    }
}
