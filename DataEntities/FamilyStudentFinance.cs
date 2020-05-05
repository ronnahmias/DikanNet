using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace DataEntities
{
    [Table("FamilyStudentFinance")]
    public class FamilyStudentFinance
    {
        [Key,Column("FamilyMemberId",Order = 0)]
        public string FamilyMemberId { get; set; }

        [Key, Column("Yearr", Order = 1), Display(Name = "שנה")]
        public int Year { get; set; }

        [Key, Column("Monthh", Order = 2), Display(Name = "חודש")]
        public int Month { get; set; }

        [Display(Name = "סכום הכנסה חודשית")]
        public int Salary { get; set; }

        [Display(Name = "קובץ שמור - הכנסה")]
        public string PathSalary { get; set; }

        [NotMapped, Display(Name = "קובץ הכנסה חודשי")]
        public HttpPostedFileBase FileSalary { get; set; }

        [Key, Column("SpId", Order = 3)]
        public int SpId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FinNo { get; set; }

        public virtual FamilyMember FamilyMember { get; set; }

        public virtual SpDefinition SpDefinition { get; set; }
    }
}
