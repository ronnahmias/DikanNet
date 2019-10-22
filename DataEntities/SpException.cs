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
        [Key, Column(Order =0)]
        [Display(Name ="בחר סטודנט")]
        [Required(ErrorMessage ="חובה לבחור סטודנט")]
        public string UserId { get; set; }

        [Key, Column(Order = 1)]
        [Display(Name = "בחר מלגה")]
        [Required(ErrorMessage = "חובה לבחור מלגה")]
        public int SpId { get; set; }

        [Display(Name = "בחר תאריך נעילה")]
        [Required(ErrorMessage = "חובה לבחור מלגה")]
        [DataType(DataType.Date)]
        //[Range(typeof(DateTime), "1/2/2019", "3/12/2019", ErrorMessage = "{0} חייב להיות בין התאריכים {1} - {2}")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true)]
        public DateTime LockDate { get; set; }
    }
}
