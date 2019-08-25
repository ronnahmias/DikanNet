using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataEntities;
using DataEntities.DB;

namespace DikanNetProject.Controllers
{
    [Authorize]
    public class DikanController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        #region ScholarShip Section

        [HttpGet]
        public ActionResult CreateEditSp(int? id)
        {
            ViewBag.Header = "עדכון מלגה";
            SpDefinition TempSp;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                TempSp = ctx.SpDef.Where(s => s.ScholarshipID == id).FirstOrDefault();
            }
            if (TempSp != null)
                return View(TempSp);
            else
            {
                ViewBag.Header = "הוספת מלגה";
                return View();
            }
                
        }

        [HttpPost]
        public ActionResult CreateEditSp(SpDefinition ClientSp)
        {
            string res = string.Empty;
            if(ModelState.IsValid)
            {
                using(DikanDbContext ctx = new DikanDbContext())
                {
                    SpDefinition Temp = ctx.SpDef.Where(s => s.ScholarshipID == ClientSp.ScholarshipID).FirstOrDefault();
                    if (Temp == null)
                    {
                        res = "מלגה נוספה בהצלחה";
                        ctx.SpDef.Add(ClientSp);
                    }
                    else
                    {
                        res = "מלגה עודכנה בהצלחה";
                        ctx.Entry(Temp).CurrentValues.SetValues(ClientSp);
                    }
                    ctx.SaveChanges();
                }
            }
            return RedirectToAction("SpList",new { response = res });
        }

        
        [HttpGet]
        [Authorize(Roles ="Student")]
        public ActionResult SpList(string response = "")
        {
            if (!(User.Identity.IsAuthenticated))
                return RedirectToAction("Login", "Login");
            ViewBag.response = response;
            List<SpDefinition> SpList;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                SpList = ctx.SpDef.ToList();
            }
            return View(SpList);
        }
        #endregion

        #region Volunter Places
        [HttpGet]
        public ActionResult VolunteerList(string response = "")
        {
            ViewBag.response = response;
            List<VolunteerPlaces> VoluList;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                VoluList = ctx.VolunteerPlaces.ToList();
            }
            return View(VoluList);
        }

        [HttpGet]
        public ActionResult CreateEditVol(int? id)
        {
            ViewBag.Header = "עדכון מקום התנדבות";
            VolunteerPlaces TempVol;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                TempVol = ctx.VolunteerPlaces.Where(s => s.Id == id).FirstOrDefault();
            }
            if (TempVol != null)
                return View(TempVol);
            else
            {
                ViewBag.Header = "הוספת מקום התנדבות";
                return View();
            }

        }

        [HttpPost]
        public ActionResult CreateEditVol(VolunteerPlaces ClientVol)
        {
            string res = string.Empty;
            if (ModelState.IsValid)
            {
                using (DikanDbContext ctx = new DikanDbContext())
                {
                    VolunteerPlaces Temp = ctx.VolunteerPlaces.Where(s => s.Id == ClientVol.Id).FirstOrDefault();
                    if (Temp == null)
                    {
                        res = "מקום התנדבות נוסף בהצלחה";
                        ctx.VolunteerPlaces.Add(ClientVol);
                    }
                    else
                    {
                        res = "מקום התנדבות עודכן בהצלחה";
                        ctx.Entry(Temp).CurrentValues.SetValues(ClientVol);
                    }
                    ctx.SaveChanges();
                }
            }
            return RedirectToAction("VolunteerList", new { response = res });
        }

        [HttpGet]
        public ActionResult DeleteVol(int VolId)
        {
            string res = string.Empty;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                VolunteerPlaces temp = ctx.VolunteerPlaces.Where(s => s.Id == VolId).FirstOrDefault();
                if (temp != null)
                {
                    ctx.VolunteerPlaces.Remove(temp);
                    ctx.SaveChanges();
                    res = "מקום התנדבות נמחק בהצלחה";
                }
            }
            return RedirectToAction("VolunteerList", new { response = res });
        }

        #endregion
    }
}