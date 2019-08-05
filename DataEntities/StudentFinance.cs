using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

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

        public int Salary { get; set; }

        public string PathSalary { get; set; }

        public int Expense { get; set; }

        public string PathExpense { get; set; }

        public Student Student { get; set; }
    }
}
