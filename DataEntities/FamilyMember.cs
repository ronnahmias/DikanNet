using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataEntities
{
    [Table("FamilyMember")]
    public class FamilyMember
    {
        [Key]
        public string FamilyMemberId { get; set; }
        public string StudentId { get; set; }
        public string FmIdFile { get; set; }
        public string Name { get; set; }
        public string Realationship { get; set; }
        public DateTime? BirthDay { get; set; }

        public virtual Student Student { get; set; }
        public virtual ICollection<FamilyStudentFinance> FamilyStudentFinance { get; set; }
    }
}
