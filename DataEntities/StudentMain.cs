using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEntities
{
    public class StudentMain
    {
        public List<ScholarshipDefinition> ScholarshipDefinitions { get; set; }
        public List<InPractice> InPracticeList { get; set; }

        public List<Socioeconomic> SocioList { get; set; }

        public List<ExcellenceStudent> ExcelList { get; set; }
    }
}
