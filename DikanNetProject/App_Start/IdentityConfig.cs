using Microsoft.AspNet.Identity;
using System;
using DataEntities.DB;
using DataEntities;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.AspNet.Identity.EntityFramework;

namespace DikanNetProject
{
    public class IdentityConfig
    {
       
    }

    public class ApplicationUserStore : UserStore<ApplicationUser>
    {
        public ApplicationUserStore(DikanDbContext context) : base(context)
        {
            //context.Get<DikanDbContext>();
        }
    }

    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store) : base(store)
        {
        }
        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var store = new UserStore<ApplicationUser>(context.Get<DikanDbContext>());
            var manager = new ApplicationUserManager(store);
            return manager;
        }
    }
}