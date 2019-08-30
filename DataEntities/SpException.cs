using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEntities
{
    [Table("SpException")]
    public class SpException
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public int SpId { get; set; }
        public DateTime LockDate { get; set; }
    }
}
