using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEntities
{
    public class SocioAdd
    {
        public SocioAdd(string pMatrialStatus) // constructor to get matrial status from student model
        {
            MatrialStatus = pMatrialStatus;
        }
        public SpSocio SocioMod { get; set; }
        public List<CarStudent> ListCarStudent { get; set; }
        public List<Funding> ListFundings { get; set; }
        public List<StudentFinance> ListStudentFinances { get; set; }
        public List<FamilyMember> ListFamMemFin { get; set; }

        public string MatrialStatus { get; set; }

    }
}
