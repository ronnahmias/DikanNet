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
        public DateTime DateOpenScholarship { get; set; }

        [Display(Name = "תאריך אחרון להגשה")]
        public DateTime DateDeadLine { get; set; }

        public virtual ICollection<SpSocio> Socioeconomicc { get; set; }
        public virtual ICollection<SpHalacha> InPractices { get; set; }
        public virtual ICollection<SpExcellence> ExcellenceStudents { get; set; }
        public virtual ICollection<StudentFinance> StudentFinances { get; set; }
        public virtual ICollection<FamilyStudentFinance> FamilyStudentFinances { get; set; }
    }
}
