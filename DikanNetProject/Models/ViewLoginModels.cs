using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DikanNetProject.Models
{
    public class RegisterUserModel
    {
        [Display(Name = "שם פרטי"), Required(AllowEmptyStrings = false, ErrorMessage = "אנא הזן פרטי")]
        public string FirstName { get; set; }

        [Display(Name = "שם משפחה"),Required(AllowEmptyStrings = false, ErrorMessage = "אנא הזן שם משפחה")]
        public string LastName { get; set; }

        [Display(Name = "תעודת זהות")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "אנא הזן תעודת זהות")]
        [MinLength(9)]
        [MaxLength(9)]
        public string UserName { get; set; }

        [Display(Name = "אימייל")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "אנא הזן אימייל")]
        public string Email { get; set; }

        [Display(Name = "סיסמא")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "אנא הזן סיסמא")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "אימות סיסמא")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "אנא הזן סיסמא")]
        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage ="סיסמא לא תואמות")]
        public string ConfirmPassword { get; set; }
    }
    public class UserLogin
    {
        [Display(Name ="תעודת זהות")]
        [Required(AllowEmptyStrings =false, ErrorMessage ="אנא הזן תעודת זהות")]
        [MinLength(9)]
        [MaxLength(9)]
        public string UserName { get; set; }

        [Display(Name = "סיסמא")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "אנא הזן סיסמא")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name ="זכור אותי")]
        public bool RememberMe { get; set; }
    }
}