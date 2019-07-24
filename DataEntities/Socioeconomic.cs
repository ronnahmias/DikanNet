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
    [Table("Socioeconomic")]
    public class Socioeconomic
    {
        [Key, Column(Order = 0)]
        public int? ScholarshipId { get; set; }

        [Key, Column(Order = 1)]
        public string StudentId { get; set; }

        [Display(Name = "שנת לימוד")]
        public string SchoolYear { get; set; }
        public string Statuss { get; set; }
        public DateTime? StatusUpdateDate { get; set; }
        public DateTime? DateSubmitScholarship { get; set; }

        [Display(Name = "האם הנך משכיר\\ה או בעל\\ת דירה?")]
        public bool Apartment { get; set; }

        [Display(Name = "\\ בעלות דירהקובץ - שכירות מגורים"),NotMapped,RequiredIf("Apartment",true,ErrorMessage ="חובה לצרף קובץ מתאים")]
        public HttpPostedFileBase ApartmentLeaseFile { get; set; }
        public string FileApartmentLease { get; set; }

        [Display(Name = "עולה חדש?")]
        public bool Newcomer { get; set; }

        [Display(Name = "תאריך עלייה"), RequiredIf("Newcomer", true, ErrorMessage = "חובה להזין תאריך עלייה")]
        public DateTime? DateImmigration { get; set; }

        [Display(Name = "קובץ - עולה חדש"),NotMapped, RequiredIf("Newcomer", true, ErrorMessage = "חובה לצרף קובץ מתאים")]
        public HttpPostedFileBase NewcomerFile { get; set; }
        public string FileNewcomer { get; set; }


        [Display(Name = "משפחה חד-הורית?")]
        public bool SingleParent { get; set; }


        [Display(Name = "קובץ - משפחה חד-הורית"), NotMapped, RequiredIf("SingleParent", true, ErrorMessage = "חובה לצרף קובץ מתאים")]
        public HttpPostedFileBase SingleParentFile { get; set; }
        public string FileSingleParent { get; set; }


        [Display(Name = "משפחה שכולה?")]
        public bool BereavedFam { get; set; }


        [Display(Name = "קובץ - משפחה שכולה"), NotMapped, RequiredIf("BereavedFam", true, ErrorMessage = "חובה לצרף קובץ מתאים")]
        public HttpPostedFileBase BereavedFamFile { get; set; }
        public string FileBereavedFam { get; set; }


        [Display(Name = "האם קיבלת מלגה בעבר?")]
        public bool ReceiveScholarship { get; set; }

        [Display(Name = "שירות צבאי")]
        public string MilitaryService { get; set; }

        [Display(Name = "קובץ - תעודת שחרור"), NotMapped, RequiredIfNotEmpty("MilitaryService", ErrorMessage = "חובה לצרף קובץ מתאים")]
        public HttpPostedFileBase MilitaryServiceFile { get; set; }
        public string FileMilitaryService { get; set; }


        [Display(Name = "שירות מילואים")]
        public bool ReserveMilitaryService { get; set; }

        [Display(Name = "קובץ - שירות מילואים"), NotMapped, RequiredIf("ReserveMilitaryService", true, ErrorMessage = "חובה לצרף קובץ מתאים")]
        public HttpPostedFileBase ReserveMilitaryServiceFile { get; set; }
        public string FileReserveMilitaryService { get; set; }


        [Display(Name = "האם יש לך מימון נוסף?")]
        public bool HasFunding { get; set; }

        [Display(Name = "האם הנך בעל רכב?")]
        public bool CarOwner { get; set; }

        [Display(Name = "סוג נכות")]
        public string DisabilityType { get; set; }

        [Display(Name = "קובץ - סוג נכות"), NotMapped, RequiredIfNotEmpty("DisabilityType", ErrorMessage = "חובה לצרף קובץ מתאים")]
        public HttpPostedFileBase FileDisabilityType { get; set; }
        public string DisabilityTypeFile { get; set; }

        public virtual ScholarshipDefinition ScholarshipDefinition { get; set; }
        public virtual Student Student { get; set; }
    }
}
