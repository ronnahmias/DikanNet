using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using System.Web;

namespace DataEntities
{
    [Table("StudentFinance")]
    public class StudentFinance
    {
        [Key, Column(Order = 0)]
        public string StudentId { get; set; }

        [Key, Column("Yearr", Order = 1), Display(Name = "שנה")]
        public int Year { get; set; }

        [Key, Column("Monthh", Order = 2), Display(Name = "חודש")]
        public int Month { get; set; }

        [Display(Name = "סכום הכנסה חודשית")]
        public int Salary { get; set; }

        public string PathSalary { get; set; }

        [Display(Name = "סכום הוצאה חודשית")]
        public int Expense { get; set; }

        public string PathExpense { get; set; }

        [NotMapped, Display(Name = "קובץ הכנסה חודשי")]
        public HttpPostedFileBase FileSalary { get; set; }

        [NotMapped, Display(Name = "קובץ הוצאה חודשי")]
        public HttpPostedFileBase FileExpense { get; set; }

        public int SpId { get; set; }

        public int FinNo { get; set; }


        public Student Student { get; set; }

        public SpDefinition SpDefinition { get; set; }
    }
}
