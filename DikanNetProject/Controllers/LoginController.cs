using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DikanNetProject.Models;
using System.IO;
using Common;
using DataEntities;
using DataEntities.DB;
using System.Net;
using System.Web.Security;
using System.Web.Routing;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using System.Diagnostics;
using System.Data.Entity.Validation;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.EntityFramework;

namespace DikanNetProject.Controllers
{
    [RequireHttps] // only https requests
    public class LoginController : Controller
    {

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public LoginController()
        {
        }

        public LoginController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        #region Login
        
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(UserLogin loginuser)
        {
            var result = await SignInManager.PasswordSignInAsync(loginuser.UserName, loginuser.Password, loginuser.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToAction("Index","Dikan",null);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    //return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    break;
                    //return View(model);
            }
            /*
            ViewBag.Status = false;
            if (ModelState.IsValid)
            {
                using (DikanDbContext ctx = new DikanDbContext())
                {
                    var account = ctx.Users.Where(e => e.UserId == loginuser.UserId).FirstOrDefault();
                    if (account != null)
                    {
                        if (string.Compare(Crypto.Hash(loginuser.Password), account.Password) == 0)
                        {
                            int timeout = loginuser.RememberMe ? 525600 : 120; // one year or 1.5 hour
                            switch (account.Role) // checks the role of the account to direct to their controller
                            {
                                case "Student":
                                    if (account.IsEmailVerified) // checks if the student has verify with email
                                    {
                                        HttpContext.Session.Add("Student", account);
                                        Session.Timeout = timeout;

                                        //FormsAuthentication.SetAuthCookie(account.UserId,true);
                                        //HttpContext.User.IsInRole("Student");

                                        var student = ctx.Students.Where(s => s.StudentId == account.UserId).FirstOrDefault();
                                        if (student != null) // if the account found in student table
                                            return RedirectToAction("Index", "Student");
                                        else // not found in student table-> go to fill basic info
                                        {
                                            ViewBag.Status = true;
                                            return RedirectToAction("UpdateStudent","Student"); // complete register of student
                                        }
                                    }
                                    return View(); // need to add error to require a email verify

                                case "Dikan":
                                    //var dikan = ctx.dikan.where(s => s.dikanid == account.userid).firstordefault();
                                    //if (dikan != null)
                                    //{
                                    //    httpcontext.session.add("dikan", account);
                                    //    session.timeout = timeout;
                                    //    return redirecttoaction("index", "dikan");
                                    //}
                                    //else
                                    //{
                                    //    return redirecttoaction("completeregisterdikan"); // complete register of dikan
                                    //}
                                    break;

                                case "Admin":
                                    HttpContext.Session.Add("admin", account);
                                    Session.Timeout = timeout;
                                    return RedirectToAction("Index", "Admin");

                                // in future add to mazkira case

                                default: break;
                            }
                        }
                    }

                }
            }*/
            return View(loginuser);
        }

        #endregion

        #region Registration

        [HttpGet,AllowAnonymous]
        public ActionResult Registration()
        {
                return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Registration(RegisterUserModel RegisterUser)
        {
            ViewBag.Status = false;
            if (ModelState.IsValid)
            {
                var user = new Users { UserName = RegisterUser.UserName, Email = RegisterUser.Email };
                var result = await UserManager.CreateAsync(user, RegisterUser.Password);
                using(DikanDbContext ctx = new DikanDbContext())
                {
                    var roleStore = new RoleStore<IdentityRole>(ctx);
                    var roleManager = new RoleManager<IdentityRole>(roleStore);
                    var userStore = new UserStore<Users>(ctx);
                    var userManager = new UserManager<Users>(userStore);
                    userManager.AddToRole(user.Id, "Student");
                }
                if (result.Succeeded)
                {
                    // Send an email with this link
                    var body = "חשבונך נוצר בהצלחה<br/>לחץ על התמונה לאימות החשבון";
                    var username = RegisterUser.FirstName + " " + RegisterUser.LastName; string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("VerifyAccount", "Login", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    body = SendMail.CreateBodyEmail(username, callbackUrl, body);
                    await UserManager.SendEmailAsync(user.Id, "יצירת חשבון", body);
                    ViewBag.Status = true;
                    ViewBag.ModelTitle = "רישום";
                    ViewBag.ModelMessageBody = "הרישום הצליח! עלייך לאמת את החשבון דרך תיבת הדואר האלקטרוני לפני ההתחברות הראשונה";

                }
            }
            return View(RegisterUser);
        }
        
        #endregion
/*
        #region Forgot Pass

        [HttpGet]
        public ActionResult ForgotPass()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPass(Users1 user)
        {
            using (DikanDbContext ctx = new DikanDbContext())
            {
                var account = ctx.Users.Where(u => u.Email == user.Email).Where(u => u.UserId == user.UserId).FirstOrDefault();
                if (account != null)
                {
                    account.ResetPasswordCode = Guid.NewGuid();
                    ctx.Configuration.ValidateOnSaveEnabled = false;
                    ctx.SaveChanges();

                    // send reset code email to user
                    var body = "על מנת לאפס את הסיסמא עלייך ללחוץ על התמונה";
                    var username = account.FirstName + " " + account.LastName;
                    var verifyUrl = "/Login/" + "ResetPassword/" + account.ResetPasswordCode.ToString();
                    var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
                    body = SendMail.CreateBodyEmail(username, link, body);
                    SendMail.SendEmailLink(account.Email, body, "איפוס סיסמא - דיקאנט");
                    // return message of sending email
                }
            }
            return View(user); // add message that the email has been send 
        }

        #endregion

        #region Reset Password

        [HttpGet]
        public ActionResult ResetPassword(string id)
        {
            using (DikanDbContext ctx = new DikanDbContext())
            {
                var account = ctx.Users.Where(u => u.ResetPasswordCode == new Guid(id)).FirstOrDefault();
                if (account != null)
                {
                    ResetPasswordModel model = new ResetPasswordModel();
                    model.ResetPasswordCode = new Guid(id);
                    ctx.Configuration.ValidateOnSaveEnabled = false;
                    ctx.SaveChanges();
                    return View(model);
                }
                else
                    return HttpNotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                using (DikanDbContext ctx = new DikanDbContext())
                {
                    var user = ctx.Users.Where(u => u.ResetPasswordCode == model.ResetPasswordCode).FirstOrDefault();
                    if (user != null)
                    {
                        user.Password = Crypto.Hash(model.Password);
                        user.ResetPasswordCode = null;
                        ctx.Configuration.ValidateOnSaveEnabled = false;
                        ctx.SaveChanges();
                        // return feedback of change password with button to login
                    }
                }
            }
            return View(model);
        }

        #endregion
        */
        #region Check If User Exist
        [HttpPost]
        public ActionResult CheckIfUserExist(string UserId, string Email) // ajax call
        {
                //email exist
                if (IsEmailExist(Email) || IsIdExist(UserId))
                {
                    ViewBag.ModelMessageBody= "מייל או תעודת זהות קיימים במערכת";
                    ViewBag.ModelTitle = "שגיאה";
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
        #endregion
        
        #region Verify Account
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> VerifyAccount(string userId, string code)
        {
            ViewBag.Status = false;
            if (userId == null || code == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            if (result.Succeeded) ViewBag.Status = true;
            return View();
        }
        #endregion

        #region Disconnect
        public ActionResult Disconnect() // disconnect from user
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Login", null);
        }
        #endregion
        
        #region Non Action
        [NonAction]
        private bool IsIdExist(string userId)
        {
            using (DikanDbContext ctx = new DikanDbContext())
            {
                var tempuser = ctx.Users.Where(user => user.UserName == userId).FirstOrDefault();
                return tempuser == null ? false : true; // if the Id not found he return false
            }
        }

        [NonAction]
        private bool IsEmailExist(string Email) // checks if email exist already
        {
            using (DikanDbContext ctx = new DikanDbContext())
            {
                var tempuser = ctx.Users.Where(user => user.Email == Email).FirstOrDefault();
                return tempuser == null ? false : true; // if the email not found he return false
            }
           
        }
        #endregion
    }
}