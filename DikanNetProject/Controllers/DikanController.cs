﻿using System;
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
using Microsoft.AspNet.Identity.EntityFramework;
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
            ViewBag.Title = "עדכון מלגה";
            SpDefinition TempSp;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                TempSp = ctx.SpDef.Where(s => s.ScholarshipID == id).FirstOrDefault(); // try to find by id
            }
            if (TempSp != null) // if the tempsp not null then get edit scholarship
                return View(TempSp);
            else
            {
                ViewBag.Title = "הוספת מלגה"; // add new scholarship
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
                UsersList = ctx.Users.ToList(); // get list of users 
            }
            foreach(var user in UsersList)
            {
                var rolesofuser = UserManager.GetRoles(user.Id)[0];
                if (rolesofuser == "Mazkira" || rolesofuser == "Dikan" /*|| !(user.UserName == User.Identity.Name)*/) // dont show the current dikan user 
                {
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
            }
            return View(Users);
        }

        #region Student Users Manage Section

        [HttpGet]
        public ActionResult FindStudent(string param,string type) // find user student in db - ajax
        {
            Users user = null;
            if(type == "Name") // search user by name
            {
                var name = param.Split(' '); // split name to first and lst name
                using(DikanDbContext ctx = new DikanDbContext())
                {
                    user = ctx.Users.Where(s => s.FirstName == name[0] && s.LastName == name[1] ).FirstOrDefault(); // contains only student role
                }
            }
            if(type == "Id") // search by id
            {
                user = UserManager.FindByName(param);
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (user != null && UserManager.IsInRole(user.Id,"Student")) // only if thecuser is student
                return View(user); //return user
            else
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }

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
        #endregion

        #region Other Users Manage Section

        [HttpGet]
        public ActionResult CreateUser() // create other users
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser(CreateUser NewUser) // create other users - post
        {
            Users user;
            if (ModelState.IsValid)
            { 
                if (LoginController.IsEmailExist(NewUser.Email) || LoginController.IsIdExist(NewUser.UserName))
                {
                    ViewBag.Error = "תעודת זהות ו\\או אימייל נמצאים במערכת";
                    ModelState.AddModelError("Email", "יש להזין אימייל שונה");
                    ModelState.AddModelError("UserName", "יש להזין תעודת זהות שונה");
                    return View(NewUser);
                }
                    user = new Users
                    {
                        FirstName = NewUser.FirstName,
                        LastName = NewUser.LastName,
                        Email = NewUser.Email,
                        EmailConfirmed = true,
                        UserName = NewUser.UserName
                    };
                    UserManager.Create(user); // create role without password
                    user = UserManager.FindByName(user.UserName); // find the user after creation
                    UserManager.AddToRole(user.Id, NewUser.Role); // add user to role or dikan or mazkira
                   // Send an email with this link
                    var body = "לחץ על התמונה <br/> לאיפוס הסיסמא לחשבונך";
                    string code = UserManager.GeneratePasswordResetToken(user.Id);
                    var callbackUrl = Url.Action("ResetPassword", "Login", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    body = SendMail.CreateBodyEmail(user.FirstName + " " + user.LastName, callbackUrl, body);
                    UserManager.SendEmail(user.Id, "איפוס סיסמא - דיקאנט", body);
                    return RedirectToAction("UsersList", new { response = "משתמש נוצר בהצלחה - נשלח קישור לאיפוס הסיסמא" });
            }
            return View(NewUser);
        }

        [HttpGet]
        public ActionResult EditUser(string Id = "") // edit other users
        {
            CreateUser temp = null;
            if (Id != "") // route came with id -> find user with the id
            {
                var user = UserManager.FindById(Id);
                if (user != null) // there is user so get view for edit
                {
                    temp = new CreateUser
                    {
                        UserName = user.UserName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Role = UserManager.GetRoles(user.Id)[0]
                    };
                    return View(temp);
                }
            }
            return RedirectToAction("UsersList", new { response = "שגיאה" }); // error back to list
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(CreateUser NewUser) // edit other users - post
        {
            Users user;
            if (ModelState.IsValid)
            {
                user = UserManager.FindByName(NewUser.UserName); // find the user 
                if (LoginController.IsEmailExist(NewUser.Email) && NewUser.Email != user.Email)// checks email unique on edit user
                {
                    ViewBag.Error = "שגיאה - נא להזין אימייל אחר";
                    ModelState.AddModelError("Email", "יש להזין אימייל שונה");
                }
                else
                {
                    if (user != null) // insert new values to user
                    {
                        user.FirstName = NewUser.FirstName;
                        user.LastName = NewUser.LastName;
                        user.Email = NewUser.Email;
                        UserManager.Update(user); // update user in user table
                        return RedirectToAction("UsersList", new { response = "פרטי המשתמש עודכנו" }); // redirect to list with response
                    }
                }
            }
            return View(NewUser);
        }

        [HttpGet]
        public ActionResult DeleteUser(string Id = "")// delete other users
        {
            var user = UserManager.FindById(Id);
            if (user != null)
            {
                UserManager.Delete(user);
                return RedirectToAction("UsersList", new { response = "המשתמש נמחק בהצלחה" });
            }
            else
                return RedirectToAction("UsersList", new { response = "שגיאה המשתמש לא נמחק" });
        }
        #endregion

        #endregion
    }
}