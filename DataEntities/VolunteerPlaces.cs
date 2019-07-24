using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataEntities
{
    [Table("volunteerPlaces")]
    public class VolunteerPlaces
    {
        public VolunteerPlaces()
        {
            
        }

        [Key]
        public int Id { get; set; }

        [Display(Name = "שם התנדבות"),Column("Namee")]
        public string Name { get; set; }

        [Display(Name = "תיאור התנדבות"),Column("Descc")]
        public string Desc { get; set; }

        [NotMapped]
        public string Name_desc { get; set; }


        public virtual ICollection<InPractice> InPracticess { get; set; }

    }
}
