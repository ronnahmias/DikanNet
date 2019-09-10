using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataEntities
{
    [Table("Major")]
    public class Major
    {
        [Key]
        public int MajorId { get; set; }

        [Display(Name = "תעודת זהות ראש מגמה")]
        public string HeadMajorId { get; set; }

        [Display(Name ="שם מגמה"), Required]
        public string MajorName { get; set; }
        public virtual HeadMajor HeadMajor { get; set; }
        public virtual ICollection<Student> Student { get; set; }
    }
}