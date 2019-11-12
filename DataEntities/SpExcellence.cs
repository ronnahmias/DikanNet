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
    [Table("SpExcellence")]
    public class SpExcellence
    {

        [Key,Column(Order = 0)]
        public int? ScholarshipId { get; set; }

        [Key, Column(Order = 1)]
        public string StudentId { get; set; }

        [Display(Name = "שנת לימוד"),Required(ErrorMessage ="חובה לבחור שנת לימוד")]
        public string SchoolYear { get; set; }

        [Display(Name = "סטטוס")]
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

        [Display(Name = "הערות נוספות"), DataType(DataType.MultilineText)]
        public string Comments { get; set; }

        [Display(Name = "ספר על עצמך"), DataType(DataType.MultilineText), Required(ErrorMessage = "יש לרשום קצת על עצמך")]
        public string AboutMe { get; set; }

        [Display(Name = "סיכום ראיון")]
        public string InterviewSummary { get; set; }

        // forgein keys
        public virtual SpDefinition ScholarshipDefinition { get; set; }
        public virtual Student Student { get; set; }
    }
}
