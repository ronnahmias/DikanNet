using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DataEntities
{
    public class DateStudentAttribute : RangeAttribute
    {
        public DateStudentAttribute()
          : base(typeof(DateTime), "1930-01-01", DateTime.Now.AddYears(-15).ToShortDateString()) { }
    }

    public class DateMinNowAttribute : RangeAttribute
    {
        public DateMinNowAttribute()
          : base(typeof(DateTime), DateTime.Now.ToShortDateString(), DateTime.Now.AddYears(100).ToShortDateString()) { }
    }
}