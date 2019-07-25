using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Web.Mvc;

namespace Common
{
    public class Enums
    {
        public enum Genders
            {
            גבר,
            אישה,
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
