using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataEntities.DB;

namespace DikanNetProject
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.CreatePerOwinContext(DikanDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
        }
    }
}