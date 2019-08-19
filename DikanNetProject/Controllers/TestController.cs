using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DataEntities;
using DataEntities.DB;

namespace DikanNetProject.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index()
        {
            return View();
        }

        public async Task<string> AddUser()
        {
            ApplicationUser user;
            ApplicationUserStore Store = new ApplicationUserStore(new DikanDbContext());
            ApplicationUserManager userManager = new ApplicationUserManager(Store);
            user = new ApplicationUser
            {
                UserName = "TestUser",
                Email = "TestUser@test.com"
                
            };

            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return result.Errors.First();
            }
            return "User Added";
        }
    }
}