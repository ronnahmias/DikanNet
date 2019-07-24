using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Common
{
    public class Enums
    {
       public enum Gender
            {
            גבר,
            אישה,
            [Display(Name = "לא מוגדר")]
            מסרב
            }
        public enum MatrialStatus
        {
            רווק,
            נשוי,
            גרוש,
            אלמן
        }

        public enum SchoolYear
        {
            א,
            ב,
            ג,
            ד
        }

        public enum LearningPath
        {
            בוקר,
            ערב
        }

        public enum Status
        {
            בטיפול,
            נדחה,
            מאושר
        }

    }
}
