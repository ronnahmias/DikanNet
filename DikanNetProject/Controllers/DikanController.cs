using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataEntities;
using DataEntities.DB;

namespace DikanNetProject.Controllers
{
    public class DikanController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        #region Create ScholarShip Definition

        [HttpGet]
        public ActionResult CreateSp()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateSp(SpDefinition NewSp)
        {
            if(ModelState.IsValid)
            {
                using(DikanDbContext ctx = new DikanDbContext())
                {
                    ctx.SpDef.Add(NewSp);
                    ctx.SaveChanges();
                }
            }
            return View();
        }
        #endregion
    }
}