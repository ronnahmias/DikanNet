using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEntities
{
    public class StudentMain
    {
        public List<SpDefinition> ScholarshipDefinitions { get; set; }
        public List<SpHalacha> InPracticeList { get; set; }

        public List<SpSocio> SocioList { get; set; }

        public List<SpExcellence> ExcelList { get; set; }
    }
}
