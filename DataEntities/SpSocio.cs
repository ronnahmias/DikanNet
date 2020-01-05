using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using Foolproof;

namespace DataEntities
{
    [Table("SpSocio")]
    public class SpSocio
    {
        [Key, Column(Order = 0)]
        public int ScholarshipId { get; set; }

        [Key, Column(Order = 1)]
        public string StudentId { get; set; }

        [Display(Name = "שנת לימוד")]
        public string SchoolYear { get; set; }

        [Display(Name = "סטטוס")]
        public string Statuss { get; set; }
        public DateTime? StatusUpdateDate { get; set; }
        public DateTime? DateSubmitScholarship { get; set; }

        [Display(Name = "האם הנך משכיר\\ה או בעל\\ת דירה?")]
        public bool Apartment { get; set; }

        [Display(Name = "צירוף קובץ חוזה דירה"),NotMapped]
        public HttpPostedFileBase FileApartmentLease { get; set; }
        public string PathApartmentLease { get; set; }

        [Display(Name = "עולה חדש?")]
        public bool Newcomer { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true)]
        [Display(Name = "תאריך עלייה"), RequiredIf("Newcomer", true, ErrorMessage = "חובה להזין תאריך עלייה")]
        public DateTime? DateImmigration { get; set; }

        [Display(Name = "צירוף קובץ תעודת עולה חדש"),NotMapped]
        public HttpPostedFileBase FileNewcomer { get; set; }
        public string PathNewcomer { get; set; }


        [Display(Name = "משפחה חד-הורית?")]
        public bool SingleParent { get; set; }


        [Display(Name = "צירוף קובץ אישור משפחה חד הורית"), NotMapped]
        public HttpPostedFileBase FileSingleParent { get; set; }
        public string PathSingleParent { get; set; }


        [Display(Name = "משפחה שכולה?")]
        public bool BereavedFam { get; set; }


        [Display(Name = "צירוף קובץ אישור משפחה שכולה"), NotMapped]
        public HttpPostedFileBase FileBereavedFam { get; set; }
        public string PathBereavedFam { get; set; }


        [Display(Name = "האם קיבלת מלגה בעבר?")]
        public bool ReceiveScholarship { get; set; }

        [Display(Name = "שירות צבאי"),Required(ErrorMessage ="חובה להזין סוג שירות צבאי")]
        public string MilitaryService { get; set; }

        [Display(Name = "צירוף קובץ תעודת שחרור"), NotMapped]
        public HttpPostedFileBase FileMilitaryService { get; set; }
        public string PathMilitaryService { get; set; }


        [Display(Name = "שירות מילואים")]
        public bool ReserveMilitaryService { get; set; }

        [Display(Name = "צירוף קובץ שירות מילואים"), NotMapped]
        public HttpPostedFileBase FileReserveMilitaryService { get; set; }
        public string PathReserveMilitaryService { get; set; }


        [Display(Name = "האם יש לך מימון נוסף?")]
        public bool HasFunding { get; set; }

        [Display(Name = "האם הנך בעל רכב?")]
        public bool CarOwner { get; set; }

        [Display(Name = "סוג נכות")]
        public string DisabilityType { get; set; }

        [Display(Name = "צירוף קובץ אישות נכות"), NotMapped]
        public HttpPostedFileBase FileDisabilityType { get; set; }
        public string PathDisabilityType { get; set; }

        [Display(Name = "צירוף קובץ חשבון בנק 3 חודשים"), NotMapped]
        public HttpPostedFileBase FileBankAccount { get; set; }
        public string PathBankAccount { get; set; }

        [Display(Name = "מצב חשבון בנק"),Required(ErrorMessage ="חובה להזין מצב חשבון בנק")]
        public int? BankStatus { get; set; }

        [Display(Name ="סטטוס עבודה"), Required(ErrorMessage = "חובה לבחור סטטוס עבודה")]
        public string WorkSt { get; set; }

        [Display(Name = "הערות נוספות")]
        public string Comments { get; set; }



        // avg of sp for list
        [NotMapped]
        public int AvgIncome { get; set; }

        [NotMapped]
        public int NumOfPersons { get; set; }

        [NotMapped]
        public string fundingList { get; set; }

        //FORGEIN KEYS
        public virtual SpDefinition ScholarshipDefinition { get; set; }
        public virtual Student Student { get; set; }

    }
}
