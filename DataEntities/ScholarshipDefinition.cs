using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEntities
{
    [Table("ScholarshipDefinition")]
    public class ScholarshipDefinition
    {
        [Key]
        public int ScholarshipID { get; set; }
        public string ScholarshipName { get; set; }
        public int ScholarshipAmount { get; set; }
        public string ScholarshipDetails { get; set; }

        [Column("Typee")]
        public int Type { get; set; }
        public DateTime DateOpenScholarship { get; set; }
        public DateTime DateDeadLine { get; set; }
        public virtual ICollection<Socioeconomic> Socioeconomicc { get; set; }
        public virtual ICollection<InPractice> InPractices { get; set; }
        public virtual ICollection<ExcellenceStudent> ExcellenceStudents { get; set; }

    }
}
