using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DikanNetProject.Models
{
    public class UsersView
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Id { get; set; }
    }

    public class CreateUser
    {
        [Display(Name = "שם פרטי"), Required(AllowEmptyStrings = false, ErrorMessage = "אנא הזן פרטי")]
        public string FirstName { get; set; }

        [Display(Name = "שם משפחה"), Required(AllowEmptyStrings = false, ErrorMessage = "אנא הזן שם משפחה")]
        public string LastName { get; set; }

        [Display(Name = "תעודת זהות")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "אנא הזן תעודת זהות")]
        [MinLength(9)]
        [MaxLength(9)]
        public string UserName { get; set; }

        [Display(Name = "אימייל")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "אנא הזן אימייל")]
        public string Email { get; set; }

        [Display(Name = "אימות אימייל")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "אנא הזן אימייל")]
        [Compare("Email", ErrorMessage = "אימייל אינו תואם")]
        public string ConfirmEmail { get; set; }

        [Display(Name = "תפקיד")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "אנא בחר תפקיד")]
        public string Role { get; set; }
    }

    public class ViewExceptionUsers
    {
        public string UserId { get; set; }

        [Display(Name = "שם מלא")]
        public string Name { get; set; }

        [Display(Name = "תעודת זהות")]
        [MinLength(9)]
        [MaxLength(9)]
        public string UserName { get; set; }

        [Display(Name = "קוד מלגה")]
        public int SpId { get; set; }

        [Display(Name = "סוג מלגה")]
        public string SpType { get; set; }

        [Display(Name = "תאריך נעילה")]
        public DateTime LockDate { get; set; }
    }
}