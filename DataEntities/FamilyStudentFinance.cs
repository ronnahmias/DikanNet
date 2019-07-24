using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public int Salary { get; set; }
        public string SalaryFile { get; set; }
        public int Expense { get; set; }
        public string ExpenseFile { get; set; }

        public virtual FamilyMember FamilyMember { get; set; }
    }
}
