using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace DataEntities
{
    [Table("InPractice")]
    public class InPractice
    {
        [Key , Column(Order = 0)]
        public int? ScholarshipId { get; set; }

        [Key, Column(Order = 1)]
        public string StudentId { get; set; }

        [Display(Name = "שנת לימוד"),Required(ErrorMessage ="חובה למלא שנת לימוד")]
        public string SchoolYear { get; set; }

        public string Statuss { get; set; }
        public DateTime? StatusUpdateDate { get; set; }
        public DateTime? DateSubmitScholarship { get; set; }

        [Display(Name = "פרט האם יש לך ניסיון בהתנדבויות בעבר"), DataType(DataType.MultilineText), Required(ErrorMessage = "חובה למלא תיבה זו - במידה ואין ניסיון נא לרשום")]
        public string VolunteerExp { get; set; }

        [Display(Name = "זמינות להתנדבות"), DataType(DataType.MultilineText), Required(ErrorMessage = "חובה למלא זמינות להתנדבות")]
        public string VoluntaryAvailability { get; set; }

        [Display(Name = "מקום התנדבות ראשון"), Required(ErrorMessage = "חובה למלא לפחות אופציה אחת להתנדבות")]
        public int? Volunteer1Id { get; set; }

        [Display(Name = "מקום התנדבות שני")]
        public int? Volunteer2Id { get; set; }

        [Display(Name = "ספר קצת על עצמך"),DataType(DataType.MultilineText), Required(ErrorMessage = "יש לרשום קצת על עצמך")]
        public string AboutMe { get; set; }

        public string InterviewSummary { get; set; }

        public virtual ScholarshipDefinition ScholarshipDefinition { get; set; }
        public virtual Student Student { get; set; }
        public virtual VolunteerPlaces VolunteerPlacess { get; set; }
    }
}
