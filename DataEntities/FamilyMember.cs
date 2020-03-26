﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace DataEntities
{
    [Table("FamilyMember")]
    public class FamilyMember
    {
        [Key,Display(Name ="תעודת זהות"),MinLength(9),MaxLength(9),Required]
        public string FamilyMemberId { get; set; }

        public string StudentId { get; set; }

        [Display(Name = "קובץ שמור")]
        public string PathFmId { get; set; }

        [Required,Column("Namee"),Display(Name = "שם מלא")]
        public string Name { get; set; }

        [Display(Name ="סוג קרבה"),Required]
        public string Realationship { get; set; }

        [Display(Name = "תאריך לידה")]
        [DataType(DataType.Date),Required]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true)]
        public DateTime? BirthDay { get; set; }

        [Display(Name = "סוג עבודה")]
        public string WorkSt { get; set; }

        [Display(Name = "מגדר"),Required]
        public string Gender { get; set; }

        [NotMapped, Display(Name = "קובץ תז בן משפחה")]
        public HttpPostedFileBase FileFamId { get; set; }

        public virtual Student Student { get; set; }
        public virtual IList<FamilyStudentFinance> FamilyStudentFinances { get; set; }
    }
}
