using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataEntities
{
    [Table("Funding")]
    public class Funding
    {
        [Key]
        public int FundingId { get; set; }
        public string StudentId { get; set; }
        public string FinancingInstitution { get; set; }
        public int? YearFinancing { get; set; }
        public int? FinancingHeight { get; set; }
        public string FundingFile { get; set; }

        public virtual Student Student { get; set; }
    }
}
