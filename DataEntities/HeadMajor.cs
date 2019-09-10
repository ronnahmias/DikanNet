using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataEntities
{
    [Table("HeadMajor")]
    public class HeadMajor
    {
        [Key]
        [Display(Name = "תעודת זהות ראש מגמה"),Required]
        public string HeadMajorId { get; set; }

        [Display(Name = "שם ראש מגמה"), Required]
        public string HeadMajorName { get; set; }

        [Display(Name = "מייל ראש מגמה"), Required]
        public string HeadMajorEmail { get; set; }
        public virtual ICollection<Major> Majors { get; set; }
        public virtual ICollection<DisciplineCommittee> DisciplineCommittees { get; set; }
    }
}
