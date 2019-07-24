using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataEntities
{
    [Table("Countries")]
    public class Countries
    {
        [Key]
        public int CountryId { get; set; }
        public string CountryName { get; set; }

        public virtual ICollection<Student> Student { get; set; }
    }
}
