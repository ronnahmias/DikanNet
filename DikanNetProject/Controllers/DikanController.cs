using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Common;
using DataEntities;
using DataEntities.DB;
using DikanNetProject.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace DikanNetProject.Controllers
{
    public class DikanController : Controller
    {

        #region Variables
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public DikanController()
        {
        }

        public DikanController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        #endregion

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

        #region Volunter Places Section
        [HttpGet]
        public ActionResult VolunteerList(string response = "")
        {
            ViewBag.response = response;
            return View();
        }

        [HttpGet]
        public JsonResult GetVolList() // get list to client of volunteer list in ajax
        {
            return Json(GetActiveVolList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateEditVol(string Id, string Name, string Desc)
        {
            VolunteerPlaces Temp = null;
            var hasid = int.TryParse(Id,out int res);
            using (DikanDbContext ctx = new DikanDbContext())
            {
                ctx.Configuration.LazyLoadingEnabled = false;
                if (hasid) // check if we got id num of vol for edit
                    Temp = ctx.VolunteerPlaces.Where(s => s.Id == res).FirstOrDefault();
                if (Temp == null)
                    ctx.VolunteerPlaces.Add(new VolunteerPlaces { Name=Name, Desc = Desc, Active = true }); // add new vol place
                else
                    ctx.Entry(Temp).CurrentValues.SetValues(new VolunteerPlaces { Name = Name, Desc = Desc, Active = true });
                ctx.SaveChanges();
            }
            return Json(GetActiveVolList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPut]
        public ActionResult DeleteVol(string VolId) // update active to false 
        {
            int intvolid = -1;
            intvolid = int.Parse(VolId);
            if(intvolid == -1)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            using (DikanDbContext ctx = new DikanDbContext())
            {
                VolunteerPlaces temp = ctx.VolunteerPlaces.Where(s => s.Id == intvolid).FirstOrDefault();
                if (temp != null)
                {
                    ctx.VolunteerPlaces.Attach(temp).Active = false;
                    ctx.SaveChanges();
                }
            }
            return Json(GetActiveVolList(), JsonRequestBehavior.AllowGet);
        }

        [NonAction]
        public List<VolunteerPlaces> GetActiveVolList() // return list of all vol places that are active true
        {
            List<VolunteerPlaces> VoluList;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                VoluList = ctx.VolunteerPlaces.Where(s => s.Active == true).ToList(); // only active places
                ctx.Configuration.LazyLoadingEnabled = false;
            }
            return VoluList;
        }

        #endregion

        #region Manage Users Section

        [HttpGet]
        public ActionResult UsersList(string response = "")// get all users list view
        {
            ViewBag.response = response; // add response message if needed
            List<Users> UsersList;
            List<UsersView> Users = new List<UsersView>();
            UsersView tempu;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                UsersList = ctx.Users.ToList(); // get list of all users
            }
            foreach(var user in UsersList)
            {
                var rolesofuser = UserManager.GetRoles(user.Id)[0];
                tempu = new UsersView
                {
                    FirstName = user.FirstName,
                    Email = user.Email,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Role = rolesofuser,
                    Id = user.Id
                };
                Users.Add(tempu);
            }
            return View(Users);
        } 

        #region Student Users Manage Section
        [HttpPost]
        public ActionResult EditEmail(string Id, string newemail) // edit email of student - ajax
        {
            Student dbstudent,tempstudent;
            var user = UserManager.FindById(Id); // find the user by id
            if (user != null)
            {
                user.Email = newemail; // inserts the new email
                user.EmailConfirmed = false; // need the new email to be confirmed
                using(DikanDbContext ctx = new DikanDbContext())
                {// gets the student to update email address in student table
                    dbstudent = ctx.Students.Where(s => s.StudentId == user.UserName).FirstOrDefault(); 
                    tempstudent = dbstudent;
                    if (dbstudent != null)
                    {
                        tempstudent.Email = newemail;
                        ctx.Entry(dbstudent).CurrentValues.SetValues(tempstudent);// update student with new email
                    }
                }
                UserManager.Update(user); // update the new email
               // Send an email with this link
                var body = "כתובת המייל שונתה<br/>לחץ על התמונה לאימות החשבון";
                var username = user.FirstName + " " + user.LastName;
                string code = UserManager.GenerateEmailConfirmationToken(user.Id);
                var callbackUrl = Url.Action("VerifyAccount", "Login", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                body = SendMail.CreateBodyEmail(username, callbackUrl, body);
                UserManager.SendEmail(user.Id, "יצירת חשבון - דיקאנט", body);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        [HttpPost]
        public ActionResult SendResetPassword(string Id) // send reset password to student - ajax
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        #endregion

        #region Other Users Manage Section

        [HttpGet]
        public ActionResult CreateEditUser(string Id="") // create or edit other users
        {
            ViewBag.Title = "הוספת משתמש";
            CreateUser temp = null;
            if (Id != "") // route came with id -> find user with the id
            {
                var user = UserManager.FindById(Id);
                if (user != null) // there is user so get i to view for edit
                {
                    temp = new CreateUser
                    {
                        UserName = user.UserName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        ConfirmEmail = user.Email
                    };
                    ViewBag.Title = "עריכת משתמש";
                }
            }
            return View(temp);
        }

        [HttpPost]
        public ActionResult CreateEditUser(UsersView NewUser) // create or edit other users - post
        {
            // need to complete
            return View();
        }

        [HttpGet]
        public ActionResult DeleteUser(string Id = "")// delete other users
        {
            // need to complete
            return View();
        }
        #endregion
        #endregion
    }
}