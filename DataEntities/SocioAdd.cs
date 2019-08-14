﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEntities
{
    public class SocioAdd
    {
        public SpSocio SocioMod { get; set; }
        public List<CarStudent> ListCarStudent { get; set; }
        public List<Funding> ListFundings { get; set; }
        public List<StudentFinance> ListStudentFinances { get; set; }
        public List<FamilyMember> ListFamMemFin { get; set; }
        public string MatrialStatus { get; set; }

    }
}
