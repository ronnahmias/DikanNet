using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEntities
{
    public class Finance
    {
        public List<StudentFinance> StudentFinancesList { get; set; }

        public List<FamilyStudentFinance> FamilyStudentFinancesList { get; set; }

        public Dictionary<string,string> FamilyMembersIdNames { get; set; }
    }
}
