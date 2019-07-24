using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Foolproof;

namespace DataEntities
{
    [Table("ExcellenceStudent")]
    public class ExcellenceStudent
    {

        [Key]
        public int? ScholarshipId { get; set; }
        public string StudentId { get; set; }

        [Display(Name = "שנת לימוד"),Required(ErrorMessage ="חובה לבחור שנת לימוד")]
        public string SchoolYear { get; set; }
        public string Statuss { get; set; }
        public DateTime? StatusUpdateDate { get; set; }
        public DateTime? DateSubmitScholarship { get; set; }

        [Display(Name = "האם אתה עובד?")]
        public bool Iswork { get; set; }

        [Display(Name = "תפקיד בעבודה"), RequiredIf("Iswork",true,ErrorMessage ="חובה למלא תפקיד בעבודה")]
        public string WorkJob { get; set; }

        [Display(Name = "מקום עבודה"), RequiredIf("Iswork", true, ErrorMessage = "חובה למלא מקום עבודה")]
        public string WorkPlace { get; set; }

        [Display(Name = "מעוניין במלגה?")]
        public bool WantSp { get; set; }

        [Display(Name = "תחומי חונכות"),RequiredIf("WantSp", true, ErrorMessage = "חובה למלא תחומי חונכות")]
        public string MentorSub { get; set; }

        [Display(Name = "הערות נוספות")]
        public string Comments { get; set; }

        [Display(Name = "ספר על עצמך"), Required(ErrorMessage = "יש לרשום קצת על עצמך")]
        public string AboutMe { get; set; }

        [Display(Name = "סיכום ראיון")]
        public string InterviewSummary { get; set; }


        public virtual ScholarshipDefinition ScholarshipDefinition { get; set; }
        public virtual Student Student { get; set; }
    }
}
