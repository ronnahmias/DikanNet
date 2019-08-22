using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataEntities
{
    public class Users : IdentityUser
    {
        [NotMapped]
        [Display(Name = "שם פרטי")]
        public string FirstName { get; set; }

        [NotMapped]
        [Display(Name = "שם משפחה")]
        public string LastName { get; set; }

        [NotMapped]
        [Display(Name = "סיסמא"),
        Column("Passwordd"),
        DataType(DataType.Password),
        MinLength(8, ErrorMessage = "יש להזין לפחות 8 תווים")]
        public string Password { get; set; }


        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<Users> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}
