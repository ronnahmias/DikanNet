using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DikanNetProject.Models
{
    public class UserLogin
    {
        [Display(Name ="תעודת זהות")]
        [Required(AllowEmptyStrings =false, ErrorMessage ="אנא הזן תעודת זהות")]
        [MinLength(9)]
        [MaxLength(9)]
        public string UserId { get; set; }

        [Display(Name = "סיסמא")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "אנא הזן סיסמא")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name ="זכור אותי")]
        public bool RememberMe { get; set; }


    }
}