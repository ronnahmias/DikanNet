using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DikanNetProject.Models
{
    public class ResetPasswordModel
    {
        [Display(Name = "סיסמא")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "סיסמא לא תקינה")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "יש להזין לפחות 8 תווים")]
        public string Password { get; set; }

        [Display(Name = "אימות סיסמא")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "אימות סיסמא לא תקין")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "יש להזין לפחות 8 תווים")]
        [Compare("Password", ErrorMessage = "יש להזין סיסמא תואמת באימות סיסמא")]
        public string ConfirmPassword { get; set; }

        public Guid? ResetPasswordCode { get; set; }
    }
}