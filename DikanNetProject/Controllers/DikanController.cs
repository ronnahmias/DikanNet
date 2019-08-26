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

        #region ScholarShip Section

        [HttpGet]
        public ActionResult CreateEditSp(int? id)
        {
            // get scholarship 
            ViewBag.Header = "עדכון מלגה";
            SpDefinition TempSp;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                TempSp = ctx.SpDef.Where(s => s.ScholarshipID == id).FirstOrDefault(); // try to find by id
            }
            if (TempSp != null) // if the tempsp not null then get edit scholarship
                return View(TempSp);
            else
            {
                ViewBag.Header = "הוספת מלגה"; // add new scholarship
                return View();
            }
                
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEditSp(SpDefinition ClientSp)
        {
            string res = string.Empty; // send response message to spllist 
            if(ModelState.IsValid)
            {
                using(DikanDbContext ctx = new DikanDbContext())
                {
                    SpDefinition Temp = ctx.SpDef.Where(s => s.ScholarshipID == ClientSp.ScholarshipID).FirstOrDefault(); // try to find sp by id
                    if (Temp == null) // if the object null then he add new scholarship
                    {
                        res = "מלגה נוספה בהצלחה";
                        ctx.SpDef.Add(ClientSp);
                    }
                    else // update existing scholarship
                    {
                        res = "מלגה עודכנה בהצלחה";
                        ctx.Entry(Temp).CurrentValues.SetValues(ClientSp);
                    }
                    ctx.SaveChanges();
                }
            }
            return RedirectToAction("SpList",new { response = res }); // return to sp list with response message
        }

        
        [HttpGet]
        public ActionResult SpList(string response = "")
        {
            ViewBag.response = response; // add response message if needed
            List<SpDefinition> SpList;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                SpList = ctx.SpDef.ToList(); // get list of all scholarships
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