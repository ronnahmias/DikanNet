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

        [Display(Name = "שם פרטי"),Required(ErrorMessage ="חובה להזין שם פרטי")]
        public string FirstName { get; set; }

        [Display(Name = "שם משפחה"), Required(ErrorMessage = "חובה להזין שם משפחה")]
        public string LastName { get; set; }

        [Display(Name = "אימייל - במידה ותשנה את כתובת המייל שלך תצטרך לאמת אותה מחדש"), Required(ErrorMessage = "חובה להזין אימייל")]
        public string Email { get; set; }

        [Display(Name = "עיר"), Required(ErrorMessage = "חובה להזין עיר מגורים")]
        public int City { get; set; }

        [Display(Name = "רחוב"), Required(ErrorMessage = "חובה להזין רחוב")]
        public string Street { get; set; }

        [Display(Name = "מספר בית"), Required(ErrorMessage = "חובה להזין מספר בית")]
        public int? HouseNo { get; set; }

     
        public string PathId { get; set; }

        [Display(Name = "קובץ תעודת זהות"),NotMapped]
        public HttpPostedFileBase FileId { get; set; }

        [Display(Name = "מגמה"),Required(ErrorMessage = "חובה לבחור מגמה")]
        public int? MajorId { get; set; }

        [Display(Name = "סוג מסלול"), Required(ErrorMessage = "חובה לבחור מסלול לימודים ")]
        public string LearnPath { get; set; }

        [Display(Name = "תאריך לידה")]
        [Required(ErrorMessage = "חובה להזין תאריך לידה")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true,ConvertEmptyStringToNull =true)]
        public string BirthDay { get; set; }

        [Display(Name = "מגדר"), Required(ErrorMessage = "חובה לבחור מגדר")]
        public string Gender { get; set; }

        [Display(Name = "מספר פלאפון"), Required(ErrorMessage = "חובה להזין מספר פלאפון ")]
        public string CellphoneNo { get; set; }

        [Display(Name = "טלפון")]
        public string PhoneNo { get; set; }

        [Display(Name = "ארץ לידה"), Required(ErrorMessage = "חובה להזין ארץ לידה")]
        public int? CountryBirthId { get; set; }

        [Display(Name = "מצב משפחתי"), Required(ErrorMessage = "חובה לבחור מצב משפחתי")]
        public string MaritalStatus { get; set; }

        public string Uniquee { get; set; }

        [NotMapped]
        [Display(Name ="שם מלא")]
        public string StudentRow { get; set; } // shows the student id + full name together

        public virtual Cities Cities { get; set; }
        public virtual Countries Country { get; set; }
        public virtual Major Major { get; set; }

        public virtual ICollection<SpSocio> Socioeconomics { get; set; }
        public virtual ICollection<SpHalacha> Halacha { get; set; }
        public virtual ICollection<SpExcellence> ExcellenceStudents { get; set; }
        public virtual ICollection<CarStudent> CarStudents { get; set; }
        public virtual ICollection<FamilyMember> FamilyMembers { get; set; }
        public virtual ICollection<StudentFinance> StudentFinances { get; set; }
        public virtual ICollection<Funding> Fundings { get; set; }

    }
}
