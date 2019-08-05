using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace DataEntities
{
    [Table("Student")]
    public class Student
    {
        [Key, Display(Name ="תעודת זהות")]
        public string StudentId { get; set; }

        [Display(Name = "שם פרטי")]
        public string FirstName { get; set; }

        [Display(Name = "שם משפחה")]
        public string LastName { get; set; }

        [Display(Name = "אימייל - במידה ותשנה את כתובת המייל שלך תצטרך לאמת אותה מחדש")]
        public string Email { get; set; }

        [Display(Name = "עיר")]
        public string City { get; set; }

        [Display(Name = "רחוב")]
        public string Street { get; set; }

        [Display(Name = "מספר בית")]
        public int? HouseNo { get; set; }

     
        public string PathId { get; set; }

        [Display(Name = "קובץ תעודת זהות"),NotMapped]
        public HttpPostedFileBase FileId { get; set; }

        [Display(Name = "מגמה")]
        public int? MajorId { get; set; }

        [Display(Name = "שנת לימוד")]
        public string LearnPath { get; set; }

        [Display(Name = "תאריך לידה")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime BirthDay { get; set; }

        [Display(Name = "מגדר")]
        public string Gender { get; set; }

        [Display(Name = "מספר פלאפון")]
        public string CellphoneNo { get; set; }

        [Display(Name = "טלפון")]
        public string PhoneNo { get; set; }

        [Display(Name = "ארץ לידה")]
        public int? CountryBirthId { get; set; }

        [Display(Name = "מצב משפחתי")]
        public string MaritalStatus { get; set; }

        public Guid Uniquee { get; set; }

        public virtual Countries Country { get; set; }
        public virtual Major Major { get; set; }

        public virtual ICollection<SpSocio> Socioeconomics { get; set; }
        public virtual ICollection<SpHalacha> InPractice { get; set; }
        public virtual ICollection<SpExcellence> ExcellenceStudents { get; set; }
        public virtual ICollection<CarStudent> CarStudents { get; set; }
        public virtual ICollection<FamilyMember> FamilyMembers { get; set; }
        public virtual ICollection<StudentFinance> StudentFinances { get; set; }
        public virtual ICollection<Funding> Fundings { get; set; }

    }
}
