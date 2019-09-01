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
        public string HeadMajorId { get; set; }
        public string HeadMajorName { get; set; }
        public string HeadMajorEmail { get; set; }
        public virtual ICollection<Major> Majors { get; set; }
        public virtual ICollection<DisciplineCommittee> DisciplineCommittees { get; set; }
    }
}
