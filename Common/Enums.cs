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
            אלמן,
            יתום,
            בודד_בארץ
        }

        public enum SchoolYear
        {
            א,
            ב,
            ג,
            ד,
            ה
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

        public enum MilitaryTypes
        {
            צבאי,
            לאומי,
            אחר
        }

        public enum WorkingStatus
        {
            שכיר = 3,
            עצמאי,
            חבר_קיבוץ,
            לא_עובד,
            פנסיונר,
            נפטר,
            נכה,
            אחר
        }

    }
}
