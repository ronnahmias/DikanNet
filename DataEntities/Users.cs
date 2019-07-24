using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DataEntities
{
    [Table("Users")]
    public class Users
    {
        [Key,
        Display(Name = "תעודת זהות"),
        Required(AllowEmptyStrings = false, ErrorMessage = "חובה להזין תעודת זהות"),
        RegularExpression("^[0-9]+$",ErrorMessage ="תעודת זהות לא תקינה"),
        MinLength(9,ErrorMessage ="תעודת זהות קצרה מידי"),
        MaxLength(9,ErrorMessage ="תעודת זהות ארוכה מידי")]
        public string UserId { get; set; }

        [Display(Name ="שם פרטי"), Required(AllowEmptyStrings =false,ErrorMessage ="חובה להזין שם פרטי")]
        public string FirstName { get; set; }

        [Display(Name = "שם משפחה"), Required(AllowEmptyStrings = false, ErrorMessage = "חובה להזין שם משפחה")]
        public string LastName { get; set; }

        [Display(Name = "אימייל"),Required(AllowEmptyStrings = false, ErrorMessage = "יש להזין אימייל"),
        EmailAddress(ErrorMessage = "אימייל לא תקין")]
        public string Email { get; set; }

        [Display(Name = "סיסמא"),
        Column("Passwordd"),
        Required(AllowEmptyStrings = false, ErrorMessage = "סיסמא לא תקינה"),
        DataType(DataType.Password),
        MinLength(8,ErrorMessage ="יש להזין לפחות 8 תווים")]
        public string Password { get; set; }

        [NotMapped,
        Display(Name = "אימות סיסמא"),
        Required(AllowEmptyStrings = false, ErrorMessage = "אימות סיסמא לא תקין"),
        DataType(DataType.Password),
        MinLength(8, ErrorMessage = "יש להזין לפחות 8 תווים"),
        Compare("Password", ErrorMessage = "יש להזין סיסמא תואמת באימות סיסמא")]
        public string ConfirmPassword { get; set; }

        [Column("Rolee")]
        public string Role { get; set; }

        public Guid? ResetPasswordCode { get; set; }

        public bool IsEmailVerified { get; set; }

        public Guid ActivationCode { get; set; }
    }
}