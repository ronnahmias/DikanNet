using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DikanNetProject.Models
{
    public class StaticVar
    {
        public static int Idd = 1;
        public static int DivIdd = 1;
        public static int getIdd
        {
            get
            {
                if (Idd == 0)
                    Idd = 1;
                else
                    Idd = 0;
                return Idd;
            }
        }

   

        public static int getDivId
        {
            get
            {
                if (DivIdd == 0)
                    DivIdd = 1;
                else
                    DivIdd = 0;
                return DivIdd;
            }
        }
    }
}