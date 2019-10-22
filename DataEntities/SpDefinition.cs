using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEntities
{
    [Table("SpDefinition")]
    public class SpDefinition
    {
        [Key]
        public int ScholarshipID { get; set; }

        [Display(Name ="שם מלגה")]
        public string ScholarshipName { get; set; }

        [Display(Name = "סכום מלגה")]
        public int ScholarshipAmount { get; set; }

        [Display(Name = "תיאור מלגה")]
        public string ScholarshipDetails { get; set; }

        [Column("Typee"),Display(Name = "סוג מלגה")]
        public string Type { get; set; }

        [Display(Name = "תאריך תחילת הגשה")]
        [DataType(DataType.Date)]
        //[Range(typeof(DateTime), "1/2/2019", "3/12/2019", ErrorMessage = "{0} חייב להיות בין התאריכים {1} - {2}")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true)]
        public DateTime DateOpenScholarship { get; set; }

        [Display(Name = "תאריך אחרון להגשה")]
        [DataType(DataType.Date)]
        //[Range(typeof(DateTime), "1/2/2019", "3/12/2019", ErrorMessage = "{0} חייב להיות בין התאריכים {1} - {2}")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true)]
        public DateTime DateDeadLine { get; set; }

        [NotMapped]
        public string SpRow { get; set; } // show the sp parameters in one row for drop down

        public virtual ICollection<SpSocio> Socioeconomicc { get; set; }
        public virtual ICollection<SpHalacha> InPractices { get; set; }
        public virtual ICollection<SpExcellence> ExcellenceStudents { get; set; }
        public virtual ICollection<StudentFinance> StudentFinances { get; set; }
        public virtual ICollection<FamilyStudentFinance> FamilyStudentFinances { get; set; }
        public virtual ICollection<Funding> Fundings { get; set; }
        public virtual ICollection<CarStudent> CarStudents { get; set; }
    }
}
