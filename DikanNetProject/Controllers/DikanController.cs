using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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

        [HttpPost]
        public ActionResult UpdateSpStatus(int ScholarId = -1, string StudId = "", string status = "") // update sp status of student
        {
            if(ScholarId == -1 || StudId == "" || status == "") // not have parameters 
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            using(DikanDbContext ctx = new DikanDbContext())
            {
                var sp = ctx.SpDef.Where(s => s.ScholarshipID == ScholarId).FirstOrDefault();
                if(sp == null)
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);

                switch(Enum.Parse(typeof(Enums.SpType), sp.Type)) // find the sp according to the type
                {
                    case Enums.SpType.סוציואקונומית:
                        var studsocio = ctx.Socio.Where(s => s.ScholarshipId == ScholarId && s.StudentId == StudId).FirstOrDefault();
                        if (studsocio != null) // if found update status
                            studsocio.Statuss = status;
                        break;

                    case Enums.SpType.הלכה:
                        var studhalacha = ctx.Halacha.Where(s => s.ScholarshipId == ScholarId && s.StudentId == StudId).FirstOrDefault();
                        if (studhalacha != null) // if found update status
                            studhalacha.Statuss = status;
                        break;

                    case Enums.SpType.מצוינות:
                        var studEx = ctx.Halacha.Where(s => s.ScholarshipId == ScholarId && s.StudentId == StudId).FirstOrDefault();
                        if (studEx != null) // if found update status
                            studEx.Statuss = status;
                        break;

                    default: return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
                SendUpdateMail(StudId, status, sp); // send update mail to student according to status
                ctx.SaveChanges();
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpGet]
        public ActionResult SubmitedSp(string res = "", int spId = -1, string spType = "") // redirect sp list of submited sp
        {
            ViewBag.res = res;
            if (spType == "") // no type parameters return to index
                return RedirectToAction("Index");

            var EspType = Enum.Parse(typeof(Enums.SpType), spType); // parse string type to enum
            if(EspType == null) // if no type -> return to index
                return RedirectToAction("Index");

            using (DikanDbContext ctx = new DikanDbContext())
            {
                if (spId == -1) // if there is no spid then get the list of the latest sp of the type
                    spId = ctx.SpDef.Where(s => s.Type == EspType.ToString()).ToList().OrderByDescending(x => x.DateDeadLine).FirstOrDefault().ScholarshipID; // get the latest spid

                switch (EspType)
                {
                    case Enums.SpType.סוציואקונומית:
                         List<SpSocio> sociolist = ctx.Socio.Where(s => s.ScholarshipId == spId).ToList();
                        return View("ListSocio", sociolist); // return view with this list
                        
                    case Enums.SpType.הלכה:
                         List<SpHalacha> halachalist = ctx.Halacha.Include("Student").Include("VolunteerPlacess").Include("ScholarshipDefinition").Where(s => s.ScholarshipId == spId).ToList();
                        return View("ListHalacha", halachalist); // return view with this list   

                    case Enums.SpType.מצוינות:
                         List<SpExcellence> excellentlist = ctx.Excellence.Where(s => s.ScholarshipId == spId).ToList();
                        return View("ListExcellence", excellentlist); // return view with this list

                    default: return RedirectToAction("Index"); // error return to index

                }
            }
        }

        [HttpGet]
        public ActionResult StudentSp(int spId = -1, string spType = "", string StudId = "") // redirect student details about sp against type of sp
        {
            if (StudId == "" || spType == "" || spId == -1) // no t parameters return to index
                return RedirectToAction("Index");

            var EspType = Enum.Parse(typeof(Enums.SpType), spType); // parse string type to enum
            if(EspType == null) // if no type -> return to index
                return RedirectToAction("Index");

            using (DikanDbContext ctx = new DikanDbContext())
            {
                switch (EspType)
                {
                    case Enums.SpType.סוציואקונומית:
                         SpSocio Studsocio = ctx.Socio.Where(s => s.ScholarshipId == spId && s.StudentId == StudId).FirstOrDefault();
                        return View("StudSocio", Studsocio); // return view with the object

                    case Enums.SpType.הלכה:
                         SpHalacha Studhalacha = ctx.Halacha
                                                    .Include("Student")
                                                    .Include("VolunteerPlacess")
                                                    .Include("ScholarshipDefinition")
                                                    .Where(s => s.ScholarshipId == spId && s.StudentId == StudId)
                                                    .FirstOrDefault();
                        return View("StudHalacha", Studhalacha); // return view with the object  

                    case Enums.SpType.מצוינות:
                         SpExcellence Studexcellent = ctx.Excellence.Where(s => s.ScholarshipId == spId && s.StudentId == StudId).FirstOrDefault();
                        return View("StudExcellence", Studexcellent); // return view with the object

                    default: return RedirectToAction("Index"); // error return to index
                }
            }
        }

        #region Manage Sp Halacha


        [HttpGet]
        public ActionResult StudHalacha(int ScholarId = -1, string StudId = "") // show halacha student full details
        {
            SpHalacha StudentHalacha = null;
            if (ScholarId == -1 || StudId == "") // not have parameters
                return RedirectToAction("ListHalacha", new { res = "שגיאה" });
            using (DikanDbContext ctx = new DikanDbContext())
            {
                StudentHalacha = ctx.Halacha.Include("Student").Include("VolunteerPlacess").Include("ScholarshipDefinition")
                    .Where(s=>s.ScholarshipId == ScholarId && s.StudentId == StudId).FirstOrDefault(); // find student in halacha sp
            }
            if(StudentHalacha == null) // not fount student
                return RedirectToAction("ListHalacha", new { res = "שגיאה" });
            return View(StudentHalacha); // display student parameters
        }

        #endregion

        #region Manage Head Major + Majors

        [HttpGet]
        public ActionResult MajorsList(string response = "")
        {
            ViewBag.response = response; // add response message if needed
            List<Major> MajorsList;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                MajorsList = ctx.Majors.Include("HeadMajor").ToList(); // get list of all majors and head majors
            }
            return View(MajorsList);
        }

        [HttpGet]
        public ActionResult CreateMajor(int? id)
        {
            // get major
            ViewBag.Title = "עדכון מגמה";
            Major TempMj;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                //ctx.Configuration.LazyLoadingEnabled = false;
                TempMj = ctx.Majors.Include("HeadMajor").Where(s => s.MajorId == id).FirstOrDefault(); // try to find by id
            }
            if (TempMj != null) // if the tempmj not null then get edit major
                return View(TempMj);
            else
            {
                ViewBag.Title = "הוספת מגמה"; // add new major
                return View();
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMajor(Major ClientMj)
        {
            string res = string.Empty; // send response message to majorlist 
            ClientMj.HeadMajorId = ClientMj.HeadMajor.HeadMajorId;
            ModelState.Remove("MajorId");
            if (ModelState.IsValid)
            {
                using (DikanDbContext ctx = new DikanDbContext())
                {
                    Major Temp = null;
                    Temp = ctx.Majors.Where(s => s.MajorId == ClientMj.MajorId).FirstOrDefault();
                    if (Temp == null) // if the object null then he add new major
                    {
                        ctx.HeadMajor.Add(ClientMj.HeadMajor);
                        ctx.Majors.Add(ClientMj);
                        res = "מגמה נוספה בהצלחה";
                    }
                    else // update existing major
                    {
                        res = "מגמה עודכנה בהצלחה";
                        var Hmajor = ctx.HeadMajor.Where(s => s.HeadMajorId == Temp.HeadMajorId).FirstOrDefault(); // get the head major to update
                        ctx.Entry(Hmajor).CurrentValues.SetValues(ClientMj.HeadMajor); // update head major
                        ctx.Entry(Temp).CurrentValues.SetValues(ClientMj); // update major
                    }
                    ctx.SaveChanges();
                }
                return RedirectToAction("MajorsList", new { response = res }); // return to majors list with response message

            }
            return View(ClientMj);
        }

        #endregion

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
            ModelState.Remove("ScholarshipID"); // if the scholarship is 0
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
            VolunteerPlaces tempdb = null;
            bool hasid = false;
            int res = -1;
            if (Id != null) // if came id we neet to edit the row
                hasid = int.TryParse(Id,out res);
            using (DikanDbContext ctx = new DikanDbContext())
            {
                ctx.Configuration.LazyLoadingEnabled = false;
                if (hasid) // check if we got id num of vol for edit
                    tempdb = ctx.VolunteerPlaces.Where(s => s.Id == res).FirstOrDefault();
                if (tempdb == null)
                    ctx.VolunteerPlaces.Add(new VolunteerPlaces { Name=Name, Desc = Desc, Active = true }); // add new vol place
                else
                {
                    tempdb.Name = Name;
                    tempdb.Desc = Desc;
                }
                    //ctx.Entry(Temp).CurrentValues.SetValues(new VolunteerPlaces { Name = Name, Desc = Desc, Active = true });
                ctx.SaveChanges();
            }
            return Json(GetActiveVolList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteVol(string VolId) // update active to false 
        {
            bool parsevalid = int.TryParse(VolId, out int res);
            if(!parsevalid)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            using (DikanDbContext ctx = new DikanDbContext())
            {
                VolunteerPlaces temp = ctx.VolunteerPlaces.Where(s => s.Id == res).FirstOrDefault();
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
            ViewBag.StudentsList = new SelectList(GetStudentList(), "Uniquee", "StudentRow"); // to show students list in drop down
            return View(Users);
        }

        #region Student Users Manage Section

        [HttpGet]
        public ActionResult FindStudent(string studentId) // find user student in db - ajax
        {
            Users user = null;
            if(studentId == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            user = UserManager.FindByName(studentId);
            if(user == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (user != null && UserManager.IsInRole(user.Id,"Student")) // only if the user is student
                return Json(user.Email,JsonRequestBehavior.AllowGet); //return user email
            else
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }

        [HttpPost]
        public ActionResult EditEmail(string studentId, string newemail) // edit email of student - ajax
        {
            Student dbstudent,tempstudent;
            if(studentId == null || newemail == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var user = UserManager.FindByName(studentId); // find the user by id
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

        #region Manage Exception Requests

        [HttpGet]
        public ActionResult ExceptionStudentList(string res = "")// all exceptions list
        {
            ViewBag.Res = res;
            List<ViewExceptionUsers> ExList = new List<ViewExceptionUsers>();
            ViewExceptionUsers temp;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                foreach(var Ex in ctx.SpExceptions.ToList())
                {
                    temp = new ViewExceptionUsers
                    {
                        UserId = Ex.UserId,
                        LockDate = Ex.LockDate,
                        Name = UserManager.FindById(Ex.UserId).FirstName + " " + UserManager.FindById(Ex.UserId).LastName,
                        SpId = Ex.SpId,
                        SpType = ctx.SpDef.Where(a => a.ScholarshipID == Ex.SpId).FirstOrDefault().Type,
                        UserName = UserManager.FindById(Ex.UserId).UserName
                    };
                    ExList.Add(temp);
                }
            }
            return View(ExList);
        }

        [HttpGet]
        public ActionResult CreateEditExStudent(string userid = "", int spid = -1) // create or edit ex
        {
            SpException tempEx = new SpException();
            if(userid != "" && spid !=-1) // is edit ex
            {
                using(DikanDbContext ctx = new DikanDbContext())
                {
                    tempEx = ctx.SpExceptions.Where(s => s.UserId == userid && s.SpId == spid).FirstOrDefault(); // find ex to edit
                }
            }
            ViewBag.StudentsList = new SelectList(GetStudentList(), "Uniquee", "StudentRow"); // to show students list in drop down
            ViewBag.SpList = new SelectList(GetSpList(), "ScholarshipID", "SpRow"); // to show sp list in drop down
            return View(tempEx);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEditExStudent(SpException ExUser)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindById(ExUser.UserId);
                if (user == null)
                    return View(ExUser); // error not found user
                using (DikanDbContext ctx = new DikanDbContext())
                {
                    var Ex = ctx.SpExceptions.Where(s => s.UserId == ExUser.UserId && s.SpId == ExUser.SpId).FirstOrDefault(); // checks if the student has already exception open for this sp
                    if(Ex==null) // if there is no ex open for this student with this sp -> add new
                        ctx.SpExceptions.Add(ExUser);
                    else
                        Ex.LockDate = ExUser.LockDate; // have already ex open so update lockdate
                    ctx.SaveChanges();
                }
                
                // Send an email with the link to continue fill the sp
                var body = "מלגת " + GetSptype(ExUser.SpId) + " נפתחה להמשך מילוי עד לתאריך: " + ExUser.LockDate.ToString("dd-MM-yyyy") + "<br/>" +"לחץ על התמונה להמשך מילוי" + "<br/>" + "<strong>שים לב, ניתן להיכנס למלגה זו אך ורק דרך מייל זה<strong>";
                var username = user.FirstName + " " + user.LastName;
                var callbackUrl = Url.Action("SpExRequest", "Student", new { Id = user.Id, SpId = ExUser.SpId }, protocol: Request.Url.Scheme);
                body = SendMail.CreateBodyEmail(username, callbackUrl, body);
                UserManager.SendEmail(user.Id, "המשך מילוי מלגת" + " " + GetSptype(ExUser.SpId), body);
                return RedirectToAction("ExceptionStudentList", "Dikan", new { res = "פתיחת המלגה נוספה בהצלחה - מייל עם קישור נשלח לסטודנט" });
            }
            ViewBag.StudentsList = new SelectList(GetStudentList(), "Uniquee", "StudentRow"); // to show students list in drop down
            ViewBag.SpList = new SelectList(GetSpList(), "ScholarshipID", "SpRow"); // to show sp list in drop down
            return View(ExUser); // error
        }

        [HttpGet]
        public ActionResult DeleteExStudent(string userid = "", int spid = -1) // remove ex 
        {
            if (userid != "" && spid != -1)
            {
                using(DikanDbContext ctx = new DikanDbContext())
                {
                    var ex = ctx.SpExceptions.Where(s => s.UserId == userid && s.SpId == spid).FirstOrDefault();
                    if(ex != null)
                    {
                        ctx.SpExceptions.Remove(ex);
                        ctx.SaveChanges();
                        return RedirectToAction("ExceptionStudentList", new { res = "נמחק בהצלחה" });
                    }
                }
            }
            return RedirectToAction("ExceptionStudentList",new { res = "אירעה שגיאה בעת המחיקה" });
        }

        #endregion

        #region Displine committee

        [HttpGet]
        public ActionResult DisciplineList(string res = "")// all discipline list
        {
            ViewBag.Res = res;
            List<DisciplineCommittee> disList = new List<DisciplineCommittee>();
            using (DikanDbContext ctx = new DikanDbContext())
            {
                disList = ctx.DisCommite.OrderByDescending(s=>s.CommitteeDate).ToList();
            }
            return View(disList);
        }

        [HttpGet]
        public ActionResult CreateEditDiscipline(int id = -1)// create or edit discipline
        {
            ViewBag.Title = "הוספת ועדה";
            ViewBag.btn = "הוסף";
            DisciplineCommittee temp = null;
            using (DikanDbContext ctx = new DikanDbContext())
            {  
                if (id != -1)
                {
             
                    temp = ctx.DisCommite.Where(s => s.CommitteeId == id).FirstOrDefault();
                    if (temp != null)
                    {
                        ViewBag.Title = "עריכת ועדה";
                        ViewBag.btn = "עדכן";
                    }
                
                }
            ViewBag.MajorList = new SelectList(ctx.Majors.ToList(), "HeadMajorId", "MajorName"); // to show students list in drop down
            }
            return View(temp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateEditDiscipline(DisciplineCommittee tempDis)
        {
            DisciplineCommittee dbdis;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                ModelState.Remove("CommitteeId");
                if (ModelState.IsValid)
                {
                
                    var respond = string.Empty;
                    dbdis = ctx.DisCommite.Where(s => s.CommitteeId == tempDis.CommitteeId).FirstOrDefault();
                    if(dbdis == null) // no committe exist
                    {
                        ctx.DisCommite.Add(tempDis);
                        respond = "ועדת משמעת נוספה בהצלחה";
                    }
                    else
                    {
                        ctx.Entry(dbdis).CurrentValues.SetValues(tempDis);// update comittee
                        respond = "ועדת משמעת עודכנה בהצלחה";
                    }
                    ctx.SaveChanges();
                    await SendDisNotificationEmailAsync(dis: tempDis, NewDis: true); // send mail to student and head major
                    return RedirectToAction("DisciplineList", new { res = respond});
                
                }
                ViewBag.MajorList = new SelectList(ctx.Majors.ToList(), "HeadMajorId", "MajorName"); // to show students list in drop down
            }
            return View(tempDis); // return view -> error
        }

        [HttpGet]
        public ActionResult DeleteDiscipline(int id = -1)// delete discipline
        {
            if(id == -1)
                return RedirectToAction("DisciplineList", new { res = "שגיאה" });
            using(DikanDbContext ctx = new DikanDbContext())
            {
                ctx.DisCommite.Remove(ctx.DisCommite.Where(s => s.CommitteeId == id).FirstOrDefault()); // remove discipline
                ctx.SaveChanges();
            }
            return RedirectToAction("DisciplineList", new { res = "הועדה נמחקה בהצלחה" });
        }

        public async Task SendDisNotificationEmailAsync(DisciplineCommittee dis, bool NewDis) // function that send mail to headmajor and student 
        {
            HeadMajor hm = null;
            // Send an email for call to discipline to student
            var body = "הנך מוזמן לועדת משמעת בתאריך: " + dis.CommitteeDate.ToString("dd/MM/yyyy") + " בשעה: " + dis.CommitteeTime.ToString("hh\\:mm");
            var username = dis.StudentFirstName + " " + dis.StudentLastName;
            var callbackUrl = Url.Action("Login", "Login", null, protocol: Request.Url.Scheme);
            body = SendMail.CreateBodyEmail(username, callbackUrl, body);
            await SendMail.configSendGridasync(new IdentityMessage { Body = body, Destination = dis.StudentMail, Subject = "זימון לועדת משמעת" });

            // Send an email for call to discipline to headmajor
            using (DikanDbContext ctx = new DikanDbContext())
            {
                hm = ctx.HeadMajor.Where(s => s.HeadMajorId == dis.HeadMajorId).FirstOrDefault();
            }
            if(hm != null)
            {
                body = "רצינו לעדכן אותך שנקבעה ועדת משמעת לסטודנט" + dis.StudentFirstName + " " + dis.StudentLastName + "<br/>" + "בתאריך: " + dis.CommitteeDate.ToString("dd/MM/yyyy") + " בשעה: " + dis.CommitteeTime.ToString("hh\\:mm");
                callbackUrl = Url.Action("Login", "Login", null, protocol: Request.Url.Scheme);
                body = SendMail.CreateBodyEmail(hm.HeadMajorName, callbackUrl, body);
                await SendMail.configSendGridasync(new IdentityMessage { Body = body, Destination = dis.StudentMail, Subject = "זימון לועדת משמעת" });
            }
        }

        #endregion

        #region Non Actions

        [NonAction]
        private void SendUpdateMail(string studId, string status, SpDefinition sp)// send email to student about update status
        {
            if (studId == "" || status == "") // if no parameters return
                return;

            var enumstatus = Enum.Parse(typeof(Enums.Status),status); // parse string to enum
            if (enumstatus == null)
                return;

            var body = string.Empty;
            Student tempstudent = null;

            using(DikanDbContext ctx = new DikanDbContext())
            {
                tempstudent = ctx.Students.Where(s => s.StudentId == studId).FirstOrDefault(); // find student accroding to studid
            }
            if (tempstudent == null) // if student null (not found) return
                return;
            var username = tempstudent.FirstName + " " + tempstudent.LastName;
            var callbackUrl = Url.Action("Login", "Login", null, protocol: Request.Url.Scheme);

            switch (enumstatus)
            {
                case Enums.Status.מאושר:
                    body = "ברכות!" + "בקשתך למלגת " + sp.Type + " " + " אושרה. " + "נא לפנות למשרד הדיקאן להמשך";
                    break;
                case Enums.Status.נדחה:
                    body = "ברצוננו לעדכן אותך כי " + "בקשתך למלגת " + sp.Type + " נדחתה.";
                    break;

                case Enums.Status.בטיפול: // no mail need to send return
                default:
                    return;
            }
            // Send an email
            body = SendMail.CreateBodyEmail(username, callbackUrl, body);
            _ = SendMail.configSendGridasync(new IdentityMessage { Body = body, Destination = tempstudent.Email, Subject = "עדכון מלגת " + sp.Type });
        }

        [NonAction]
        public List<Major> GetMajorList() // add to student id and full name to studentrow field
        {
            List<Major> majors = new List<Major>();
            using (DikanDbContext ctx = new DikanDbContext())
            {
                majors = ctx.Majors.ToList(); // all students in the system
                foreach (var major in majors.ToList()) // sets student id and drop down list
                {
                    
                }
            }
            return majors;
        }

        [NonAction]
        public List<Student> GetStudentList() // add to student id and full name to studentrow field
        {
            List<Student> students = new List<Student>();
            using (DikanDbContext ctx = new DikanDbContext())
            {
                students = ctx.Students.ToList(); // all students in the system
                foreach (var student in students.ToList()) // sets student id and drop down list
                {
                    student.StudentRow = student.StudentId + " - " + student.FirstName + " " + student.LastName;
                }
            }
            return students;
        }

        [NonAction]
        public List<SpDefinition> GetSpList() // add to sprow field scholrshipid + name + type
        {
            List<SpDefinition> spDef = new List<SpDefinition>();
            using (DikanDbContext ctx = new DikanDbContext())
            {
                spDef = ctx.SpDef.Where(s=> s.DateDeadLine < DateTime.Now).OrderByDescending(s=>s.DateDeadLine).ToList(); // show only sp that has over and order it by the last one
                foreach (var def in spDef.ToList()) // sets row for drop down list
                {
                    def.SpRow = def.ScholarshipID + " - " + def.ScholarshipName + " - " + def.Type;
                }
            }
            return spDef;
        }

        [NonAction]
        public string GetSptype(int spid) // get sp type according to spid
        {
            using (DikanDbContext ctx = new DikanDbContext())
            {
                return ctx.SpDef.Where(s => s.ScholarshipID == spid).FirstOrDefault().Type;
            }
        }
        #endregion
    }
}