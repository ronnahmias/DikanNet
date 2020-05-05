using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DataEntities
{
    public class Finance
    {
        public string Id { get; set; }

        public int Year { get; set; }

        public int Month { get; set; }

        public int? Salary { get; set; }

        public string PathSalary { get; set; }

        public HttpPostedFileBase FileSalary { get; set; }

        public int SpId { get; set; }

        public int FinNo { get; set; }
    }
}
