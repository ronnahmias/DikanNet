using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataEntities
{
    [Table("Cities")]
    public class Cities
    {
        [Key]
        public int Id { get; set; }

        [Column("Namee")]
        public string Name { get; set; }
    }
}
