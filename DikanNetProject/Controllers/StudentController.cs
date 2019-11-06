using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DikanNetProject.Models;
using Common;
using DataEntities;
using DataEntities.DB;
using System.Web.Routing;
using System.IO;
using System.Data.Entity;
using System.Net;
using Newtonsoft.Json;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Z.EntityFramework.Plus;

namespace DikanNetProject.Controllers
{
    [RequireHttps]
    [Authorize(Roles ="Student")] // only student can access to controller
    public class StudentController : Controller
    {
        string sStudentId;

        #region Constructor and more
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            sStudentId = HttpContext.User.Identity.Name; // get id of user into global var
        }

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public StudentController()
        {
        }

        public StudentController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        #region Index
        [HttpGet]
        [Authorize(Roles = "Student")]
        public ActionResult Index(string response = "")
        {
            ViewBag.res = response;
            StudentMain studentMain = new StudentMain();
            using (DikanDbContext ctx = new DikanDbContext())
            {
                var student = ctx.Students.Where(s => s.StudentId == User.Identity.Name).FirstOrDefault();
                if(student == null) // if there is no student record return to update student page to fill details
                    return RedirectToAction("UpdateStudent", "Student");

                studentMain.ScholarshipDefinitions = ctx.SpDef.Where(x => DbFunctions.TruncateTime(x.DateDeadLine) >= DbFunctions.TruncateTime(DateTime.Now) && DbFunctions.TruncateTime(x.DateOpenScholarship) <= DbFunctions.TruncateTime(DateTime.Now)).ToList();
                foreach (var scholarship in studentMain.ScholarshipDefinitions.ToList()) // dont show scholarship that already is submited
                {
                    switch (Enum.Parse(typeof(Enums.SpType),scholarship.Type))
                    {
                        case Enums.SpType.סוציואקונומית:
                            if (ctx.Socio.Any(s => s.ScholarshipId == scholarship.ScholarshipID && s.StudentId == sStudentId))
                                studentMain.ScholarshipDefinitions.Remove(scholarship);
                            break;

                        case Enums.SpType.מצוינות:
                            if (ctx.Excellence.Any(s => s.ScholarshipId == scholarship.ScholarshipID && s.StudentId == sStudentId))
                                studentMain.ScholarshipDefinitions.Remove(scholarship);
                            break;

                        case Enums.SpType.הלכה:
                            if (ctx.Halacha.Any(s => s.ScholarshipId == scholarship.ScholarshipID && s.StudentId == sStudentId))
                                studentMain.ScholarshipDefinitions.Remove(scholarship);
                            break;
                    }

                }
                // send only scholarship that belongs to student id
                studentMain.InPracticeList = ctx.Halacha.Include(s => s.ScholarshipDefinition).Where(s => s.StudentId == sStudentId).ToList(); // send to view inpractice list of student
                studentMain.ExcelList = ctx.Excellence.Include(s => s.ScholarshipDefinition).Where(s => s.StudentId == sStudentId).ToList(); // send to view excellence list of student
                studentMain.SocioList = ctx.Socio.Include(s => s.ScholarshipDefinition).Where(s => s.StudentId == sStudentId).ToList(); // send to view socio list of student
                var user = UserManager.FindByName(sStudentId);
                ViewBag.StudentName = user.FirstName + " " + user.LastName;
            }
            return View(studentMain);
        }
        #endregion

        #region Update Password

        [HttpGet]
        [Authorize(Roles = "Student")]
        public ActionResult ChangePass()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePass(ChangePassword changePassword)
        {
            if(ModelState.IsValid) // check valid inputs
            {
                var user = UserManager.FindByName(User.Identity.Name); // find user by id number
                if (user == null)
                    return RedirectToAction("Index", new { response = "אירעה שגיאה במהלך החלפת הסיסמא נסה שנית מאוחר יותר" }); // error user not found
                if (!UserManager.CheckPassword(user, changePassword.OldPassword)) // compare to old password match
                {
                    ModelState.AddModelError(string.Empty, "שגיאה אחד מהשדות או יותר אינם נכונים");
                    return View(changePassword); // error return to view
                }
                var result = UserManager.ChangePassword(user.Id, changePassword.OldPassword, changePassword.Password);
                if (result.Succeeded)
                    return RedirectToAction("Index", new { response = "סיסמא עודכנה בהצלחה!" });
            }
            return View(changePassword);
        }


        #endregion

        #region Update Info Student

        [HttpGet]
        [Authorize(Roles = "Student")]
        public ActionResult UpdateStudent()
        {
            Student student;
            Users user;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                ViewBag.MajorsList = new SelectList(ctx.Majors.OrderBy(s=>s.MajorName).ToList(), "MajorId", "MajorName"); // to show majors list in drop down
                ViewBag.CountriesList = new SelectList(ctx.Countries.OrderBy(s=>s.CountryName).ToList(), "CountryId", "CountryName"); // to show countries list in drop down
                ViewBag.CitiesList = new SelectList(ctx.Cities.OrderBy(s=>s.Name).ToList(), "Id", "Name"); // to show cities list in drop down

                student = ctx.Students.Where(z => z.StudentId == sStudentId).FirstOrDefault();
                user = UserManager.FindByName(User.Identity.Name);
                if (student == null) // didnt found in student table -> first login -> update basic info
                {
                    student = new Student
                    {
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    };
                }
            }
            return View(student);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateStudent(Student UpdateStudent)
        {
            UpdateStudent.StudentId = sStudentId; // bind student id to model
            var tempuser = await UserManager.FindByNameAsync(UpdateStudent.StudentId); // get the student user
            if (tempuser == null)
                return View(UpdateStudent);
            UpdateStudent.Uniquee = tempuser.Id; // update unique id to student table
            if (UpdateStudent.PathId == null && UpdateStudent.FileId == null) // there is no file saved and no upload so add error
                ModelState.AddModelError("FileId", "חובה לצרף קובץ תעודת זהות");
            if (ModelState.IsValid)
            {
                using (DikanDbContext ctx = new DikanDbContext())
                {
                    Student dbStudent = ctx.Students.Find(UpdateStudent.StudentId);
                    if (tempuser != null) // if the student changed the name update in users list also
                    {
                        tempuser.FirstName = UpdateStudent.FirstName;
                        tempuser.LastName = UpdateStudent.LastName;
                    }
                    if (UpdateStudent.Email != tempuser.Email) // check if the student has change the email address
                    {
                        tempuser.Email = UpdateStudent.Email; // update email address in user list
                        tempuser.EmailConfirmed = false;
                        var body = "כתובת האימייל שונתה בחשבון<br/>לחץ על התמונה לאימות החשבון";
                        var username = tempuser.FirstName + " " + tempuser.LastName;
                        string code = await UserManager.GenerateEmailConfirmationTokenAsync(tempuser.Id);
                        var callbackUrl = Url.Action("VerifyAccount", "Login", new { userId = tempuser.Id, code = code }, protocol: Request.Url.Scheme);
                        body = SendMail.CreateBodyEmail(username, callbackUrl, body);
                        await UserManager.SendEmailAsync(tempuser.Id, "עדכון אימייל בחשבון", body);
                    }
                    var result = await UserManager.UpdateAsync(tempuser); // update the detail in users table

                    if (UpdateStudent.FileId != null)
                       UpdateStudent.PathId = Files.SaveFileInServer(UpdateStudent.FileId, "Id", UpdateStudent.StudentId, (dbStudent == null) ? null : dbStudent.PathId);

                    if (dbStudent == null) // after first login fill more info
                        ctx.Students.Add(UpdateStudent); // add student to data base
                    else
                        ctx.Entry(dbStudent).CurrentValues.SetValues(UpdateStudent);// update student
                    ctx.SaveChanges();

                    if (tempuser != null && tempuser.EmailConfirmed == false) // if the student change the email disconnect from system
                        return RedirectToAction("Disconnect", "Login");

                    return RedirectToAction("Index", new { response = "המשתמש עודכן בהצלחה" });
                }
            }
            using (DikanDbContext ctx = new DikanDbContext())
            {
                ViewBag.MajorsList = new SelectList(ctx.Majors.OrderBy(s => s.MajorName).ToList(), "MajorId", "MajorName"); // to show majors list in drop down
                ViewBag.CountriesList = new SelectList(ctx.Countries.OrderBy(s => s.CountryName).ToList(), "CountryId", "CountryName"); // to show countries list in drop down
                ViewBag.CitiesList = new SelectList(ctx.Cities.OrderBy(s => s.Name).ToList(), "Id", "Name"); // to show cities list in drop down
            }
            return View(UpdateStudent);
        }

        #endregion

        #region Redirect To Scholarship
        [HttpGet]
        [Authorize(Roles = "Student")]
        public ActionResult RedirectToScholarship(int scholarshipid)
        {
            SpDefinition temp;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                temp = ctx.SpDef.Find(scholarshipid);
            }
            switch (Enum.Parse(typeof(Enums.SpType), temp.Type))
            {
                case Enums.SpType.סוציואקונומית:
                    return RedirectToAction("Socio", new { scholarshipid }); // type 1 is socio scholarship

                case Enums.SpType.מצוינות:
                    return RedirectToAction("Excellent", new { scholarshipid }); // type 2 is metuyanut scholarship

                case Enums.SpType.הלכה:
                    return RedirectToAction("Halacha", new { scholarshipid }); // type 3 is halacha scholarship

                default: return RedirectToAction("Index"); // index of student if not found type
            }
        }
        #endregion

        #region Halacha Lemaase Scholarship

        [HttpGet]
        [Authorize(Roles = "Student")]
        public ActionResult Halacha(int scholarshipid, bool open = false)
        {
            SpHalacha temphalacha;
            ViewBag.VolunteerPlacesList = new SelectList(SetsvolunteerPlaces(), "Id", "Name_desc"); // to show volunteer places list in drop down
            using (DikanDbContext ctx = new DikanDbContext())
            {
                temphalacha = ctx.Halacha.Where(s => s.StudentId == sStudentId && s.ScholarshipId == scholarshipid).SingleOrDefault();
            }
            if (temphalacha == null) // checks if is it first time sign to scholarship or has save draft
            {
                temphalacha = new SpHalacha
                {
                    ScholarshipId = scholarshipid,
                    StudentId = sStudentId
                };
            }
            else
            {
                if (temphalacha.DateSubmitScholarship != null && !open) // if already has entered this milga and not exception open
                    return RedirectToAction("Index");
            }
            return View(temphalacha);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public ActionResult Halacha(SpHalacha temphalacha, string uploadmethod) // submit  new halacha scholarship
        {
            ViewBag.ResOk = false;
            ViewBag.Response = "טיוטא נשמרה בהצלחה";
            temphalacha.StudentId = sStudentId;
            SpHalacha Studentinpractice;
            ViewBag.VolunteerPlacesList = new SelectList(SetsvolunteerPlaces(), "Id", "Name_desc"); // to show volunteer places list in drop down
            using (DikanDbContext ctx = new DikanDbContext())
            {
                Studentinpractice = ctx.Halacha.Where(s => s.StudentId == temphalacha.StudentId && s.ScholarshipId == temphalacha.ScholarshipId).SingleOrDefault(); // find if he insert already draft     
                if (uploadmethod.Equals("submit"))
                {// submit scholarship
                    if (temphalacha.Volunteer1Id == null) // checks that volunteer place must select
                        ModelState.AddModelError("Volunteer1Id", "חובה למלא לפחות העדפת מקום התנדבות אחת");
                    if (ModelState.IsValid)
                    {
                        temphalacha.Statuss = Enums.Status.בטיפול.ToString(); // insert status betipul
                        temphalacha.StatusUpdateDate = temphalacha.DateSubmitScholarship = DateTime.Now; // insert date submit + update status
                        ViewBag.Response = "המלגה הוגשה בהצלחה!";
                        ViewBag.ResOk = true;
                    }
                    else // if not all required fields are fill
                    {
                        ViewBag.ResOk = "Error"; // if error open error modal
                        return View(temphalacha);
                    }
                }
                if (Studentinpractice == null)
                {
                    ctx.Halacha.Add(temphalacha); // if it first time insert new
                }
                else
                    ctx.Entry(Studentinpractice).CurrentValues.SetValues(temphalacha); // update
                ctx.Configuration.ValidateOnSaveEnabled = false;
                ctx.SaveChanges();
            }
            ViewBag.ResOk = true;
            return View(temphalacha);
        }
        #endregion

        #region Excellent Scholarship

        [HttpGet]
        [Authorize(Roles = "Student")]
        public ActionResult Excellent(int scholarshipid, bool open = false)
        {
            SpExcellence tempmetsuyanut;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                tempmetsuyanut = ctx.Excellence.Where(s => s.StudentId == sStudentId && s.ScholarshipId == scholarshipid).SingleOrDefault();
            }
            if (tempmetsuyanut == null) // checks if is it first time sign to scholarship or has save draft
            {
                tempmetsuyanut = new SpExcellence
                {
                    ScholarshipId = scholarshipid,
                    StudentId = sStudentId
                };
            }
            else
            {
                if (tempmetsuyanut.DateSubmitScholarship != null && !open) // if already has entered this milga and not exception open
                    return RedirectToAction("Index");
            }
            return View(tempmetsuyanut);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public ActionResult Excellent(SpExcellence tempmetmesuyanut, string uploadmethod) // submit  new metsuyanut scholarship
        {
            ViewBag.ResOk = false;
            ViewBag.Response = "טיוטא נשמרה בהצלחה";
            tempmetmesuyanut.StudentId = sStudentId;
            SpExcellence StudentMetsuyanut;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                StudentMetsuyanut = ctx.Excellence.Where(s => s.StudentId == tempmetmesuyanut.StudentId && s.ScholarshipId == tempmetmesuyanut.ScholarshipId).SingleOrDefault(); // find if he insert already draft     
                if (uploadmethod.Equals("submit"))
                {// submit scholarship
                    if (ModelState.IsValid)
                    {
                        tempmetmesuyanut.Statuss = Enums.Status.בטיפול.ToString(); // insert status betipul
                        tempmetmesuyanut.StatusUpdateDate = tempmetmesuyanut.DateSubmitScholarship = DateTime.Now; // insert date submit + update status
                        ViewBag.Response = "המלגה הוגשה בהצלחה!";
                        ViewBag.ResOk = true;
                    }
                    else
                    {
                        ViewBag.ResOk = "Error"; // if error open error modal
                        return View(tempmetmesuyanut);
                    }
                }
                if (StudentMetsuyanut == null)
                {
                    ctx.Excellence.Add(tempmetmesuyanut); // if it first time insert new
                }
                else
                    ctx.Entry(StudentMetsuyanut).CurrentValues.SetValues(tempmetmesuyanut); // update
                ctx.Configuration.ValidateOnSaveEnabled = false;
                ctx.SaveChanges();
            }
            ViewBag.ResOk = true;
            return View(tempmetmesuyanut);
        }
        #endregion

        #region SocioEconomic Scholarship


        #region SocioEconomic GET

        [HttpGet]
        [Authorize(Roles = "Student")]
        public ActionResult Socio(int scholarshipid, bool open = false)
        {
            SocioAdd socio;
            int numofFamMem = 0; // says how many row of family member with finance needed
            ViewBag.YearsList = new SelectList(YearsSelectList(), null, "Text"); // to show years list in drop down
            ViewBag.MonthsList = new SelectList(MonthsSelectList(), null, "Text"); // to show months list in drop down
            using (DikanDbContext ctx = new DikanDbContext())
            {
                socio = new SocioAdd() // new socio add model get the matrial status in construstor
                {
                    SocioMod = new SpSocio(),
                    ListCarStudent = new List<CarStudent>(),
                    ListFundings = new List<Funding>(),
                    ListStudentFinances = new List<StudentFinance>(),
                    ListFamMemFin = new List<FamilyMember>(), // family with finance
                    ListFamMem = new List<FamilyMember>(),
                    MatrialStatus = ctx.Students.Where(s => s.StudentId == sStudentId).FirstOrDefault().MaritalStatus
                };

                #region Socio Model
                // socio model get
                socio.SocioMod = ctx.Socio.Where(s => s.StudentId == sStudentId && s.ScholarshipId == scholarshipid).SingleOrDefault(); // get socio model of student from db
                if (socio.SocioMod == null) socio.SocioMod = new SpSocio { WorkSt = Enums.WorkingStatus.שכיר.ToString() }; // init workst radio button
                socio.SocioMod.ScholarshipId = scholarshipid; // insert scholarship id in socio model
                if(socio.SocioMod.DateSubmitScholarship != null && !open) // if already has entered this milga and not exception open
                    return RedirectToAction("Index");
                #endregion

                #region Car Student + Fundings
                foreach (var car in ctx.CarStudents.Where(s => s.StudentId == sStudentId && s.SpId == scholarshipid).ToList()) // get all cars of student from db to list
                    socio.ListCarStudent.Add(car);
                foreach (var fund in ctx.Fundings.Where(s => s.StudentId == sStudentId && s.SpId == scholarshipid).ToList()) // get all fundings of student from db to list
                    socio.ListFundings.Add(fund);
                #endregion

                #region Student Finance

                foreach (var StudFin in ctx.StudentFinances.Where(s => s.StudentId == sStudentId && s.SpId == scholarshipid).ToList()) // get all fundings of student from db to list
                    socio.ListStudentFinances.Add(StudFin);

                if (socio.ListStudentFinances.Count() < 3) // if the student dont have 3 finance rows
                {
                    do
                    {
                        socio.ListStudentFinances.Add(new StudentFinance { FinNo = socio.ListStudentFinances.Count() }); // add finance row to list
                    } while (socio.ListStudentFinances.Count < 3);
                }
                socio.ListStudentFinances = socio.ListStudentFinances.OrderBy(s => s.FinNo).ToList();
                #endregion

                #region Family Member + Finance
                /* family member + finance
                 1. calculate how many rows need depend on matrial status
                 2. get from db family member record, include finances that is dad/mom/wife/husband and from that student id and add to socio list
                 3. check if he has enough rows if not create new rows
                 4. on each family member checks the rows of finance - filter the finance of this spid and create new rows if needed
                 */

                //1. checks how much of family member row need depend on matrial status
                if(socio.MatrialStatus != null)
                    switch (Enum.Parse(typeof(Enums.MatrialStatus), socio.MatrialStatus))
                    {
                        // need 2 parents
                        case Enums.MatrialStatus.רווק:
                        case Enums.MatrialStatus.בודד_בארץ:
                            numofFamMem = 2;
                            break;
                        // need only for husband or wife
                        case Enums.MatrialStatus.נשוי:
                            numofFamMem = 1;
                            break;
                        // no need
                        case Enums.MatrialStatus.גרוש:
                        case Enums.MatrialStatus.אלמן:
                        case Enums.MatrialStatus.יתום:
                            numofFamMem = 0;
                            break;
                        default:
                            numofFamMem = 2;
                            break;
                    }
                else { numofFamMem = 2; }
                    

                if (numofFamMem > 0) // if no needed row for family member with finance skip on it
                {
                    //2. include on each family member that is dad/mom/wife/husband their finance
                    foreach (var FamMem in ctx.FamilyMembers.Include(s => s.FamilyStudentFinances)
                        .Where(s => s.StudentId == sStudentId)
                        .Where(s => s.Realationship == Enums.Realationship.אב.ToString() ||
                        s.Realationship == Enums.Realationship.אם.ToString() ||
                        s.Realationship == Enums.Realationship.בעל.ToString() ||
                        s.Realationship == Enums.Realationship.אישה.ToString()).ToList()) // filter only dad mom and wife/husband
                        socio.ListFamMemFin.Add(FamMem);

                    //3. if family member rows smaller than what he need to fill
                    if (socio.ListFamMemFin.Count() < numofFamMem)
                    {
                        do
                        {
                            IList<FamilyStudentFinance> familyStudentFinances = new List<FamilyStudentFinance>(); // init new list of finance to each family member
                            socio.ListFamMemFin.Add(new FamilyMember { FamilyStudentFinances = familyStudentFinances, WorkSt = Enums.WorkingStatus.שכיר.ToString() }); // add family member row to list and init finance and work st
                        } while (socio.ListFamMemFin.Count < numofFamMem);
                    }

                    //4. on each family member checks if he has 3 row of finance if not add
                    foreach (var member in socio.ListFamMemFin)
                    {
                        // on each member filter their only finance of the currect scholarship id
                        member.FamilyStudentFinances = member.FamilyStudentFinances.Where(s => s.SpId == scholarshipid).ToList();
                        if (member.FamilyStudentFinances.Count() < 3) // complete to 3 row of finance on each member
                        {
                            do
                            {
                                member.FamilyStudentFinances.Add(new FamilyStudentFinance { FinNo = member.FamilyStudentFinances.Count() }); // add finance row to list
                            } while (member.FamilyStudentFinances.Count < 3);
                        }
                    }
                }
                #endregion

                #region Family Members
                // get all family member that is not dad/mom/wife/husband
                foreach (var Fam in ctx.FamilyMembers
                        .Where(s => s.StudentId == sStudentId)
                        .Where(s => s.Realationship == Enums.Realationship.אח.ToString() ||
                        s.Realationship == Enums.Realationship.אחות.ToString() ||
                        s.Realationship == Enums.Realationship.בן.ToString() ||
                        s.Realationship == Enums.Realationship.בת.ToString()).ToList()) // filter only sons and brothers and sisters
                    socio.ListFamMem.Add(Fam);
                #endregion

            }
            return View(socio);
        }
        #endregion

        #region SocioEconomic POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public ActionResult Socio(SocioAdd socio, string uploadmethod) // submit  new socio scholarship
        {
            ViewBag.ResOk = "False";
            if (socio.ListCarStudent == null) socio.ListCarStudent = new List<CarStudent>(); // if there is no rows in car student list
            if (socio.ListFundings == null) socio.ListFundings = new List<Funding>(); // if there is no rows in fundings list
            if (socio.ListFamMemFin == null) socio.ListFamMemFin = new List<FamilyMember>(); // if there is no rows in family member finance list
            if (socio.ListStudentFinances == null) socio.ListStudentFinances = new List<StudentFinance>(); // if there is no rows in finance list
            if (socio.ListFamMem == null) socio.ListFamMem = new List<FamilyMember>(); // if there is no rows in family list

            ViewBag.YearsList = new SelectList(YearsSelectList(), null, "Text"); // to show years list in drop down
            ViewBag.MonthsList = new SelectList(MonthsSelectList(), null, "Text"); // to show months list in drop down

            if (uploadmethod.Equals("submit")) // if submit socio
            {
                if (!socioIsValid(socio)) // validation to all socio model
                {
                    if (!ModelState.IsValid)
                    {
                        ViewBag.ResOk = "Error";
                        return View(socio);
                    }
                }
            }
            else
                socio = SocioDraftValid(socio); // checks errors to save draft
            
            if (socio.SocioMod.CarOwner && socio.ListCarStudent != null)
                SaveSocioCars(socio.ListCarStudent, socio.SocioMod.ScholarshipId); // if there is cars -> Save Cars Detailes

            if (socio.ListFundings != null)
                SaveFundings(socio.ListFundings, socio.SocioMod.ScholarshipId);  //  if there is fundings -> Save Funding

            SaveStudentFinance(socio.ListStudentFinances, socio.SocioMod.ScholarshipId); // Save Student Finance

            SaveFamilyMemberFinance(socio.ListFamMemFin, socio.SocioMod.ScholarshipId);  // Save Family Member + Finance

            if (socio.ListFamMem != null)
                SaveFamilyMember(socio.ListFamMem);// if there is family members -> Save Family Members

            socio.SocioMod = SaveSocioModel(socio.SocioMod); // Save Socio model
            ViewBag.ResOk = "True";
            ViewBag.Response = "הטיוטא נשמרה בהצלחה!";

            if (uploadmethod.Equals("submit")) // if there is submit the sp and not a draft
            {
                using (DikanDbContext ctx = new DikanDbContext())
                {
                    socio.SocioMod.Statuss = Enums.Status.בטיפול.ToString(); // insert status betipul
                    socio.SocioMod.StatusUpdateDate = socio.SocioMod.DateSubmitScholarship = DateTime.Now; // insert date submit + update status 
                    SpSocio Dbsocio = ctx.Socio.Where(s => s.StudentId == socio.SocioMod.StudentId && s.ScholarshipId == socio.SocioMod.ScholarshipId).SingleOrDefault();
                    ctx.Entry(Dbsocio).CurrentValues.SetValues(socio.SocioMod); // update socio model
                    ctx.SaveChanges();
                    ViewBag.ResOk = "True";
                    ViewBag.Response = "המלגה הוגשה בהצלחה!";
                }
            }
            return View(socio);
        }

        #region Post Socio Methods

        private SocioAdd SocioDraftValid(SocioAdd socio)
        {
            #region socio model
            if (socio.SocioMod.Newcomer && socio.SocioMod.DateImmigration == null)
            {
                socio.SocioMod.Newcomer = false;
                ModelState.Remove("SocioMod.Newcomer");
            }
            #endregion

            #region Fundings
            List<Funding> tempfund = new List<Funding>();
            foreach(var fund in socio.ListFundings) // remove funds that dont have all fields
            {
                if (string.IsNullOrEmpty(fund.FinancingInstitution) ||
                    fund.FinancingHeight == null ||
                    fund.YearFinancing == null)
                        tempfund.Add(fund);
            }

            if(tempfund != null)
            {
                foreach (var delfund in tempfund) // remove all funds from the temp table
                    socio.ListFundings.Remove(delfund);
            }
            #endregion

            #region cars
            List<CarStudent> tempcar = new List<CarStudent>();
            foreach (var car in socio.ListCarStudent) // remove funds that dont have all fields
            {
                if (car.CarNumber == null ||
                    string.IsNullOrEmpty(car.CarCompany) ||
                    string.IsNullOrEmpty(car.CarModel) ||
                    car.CarYear == 0)
                        tempcar.Add(car);
            }

            if (tempcar != null)
            {
                foreach (var delcar in tempcar) // remove all cars from the temp table
                    socio.ListCarStudent.Remove(delcar);
            }
            #endregion

            #region student finance
            List<StudentFinance> studfintemp = new List<StudentFinance>();
            foreach (var fin in socio.ListStudentFinances) // remove student finance rows that dont have filled
            {
                if (fin.Year == 0 ||
                    fin.Month == 0)
                        studfintemp.Add(fin);
            }

            if (studfintemp != null)
            {
                foreach (var delfin in studfintemp) // remove all student finance from the temp table
                    socio.ListStudentFinances.Remove(delfin);
            }
            #endregion

            #region family members
            List<FamilyMember> fammemtemp = new List<FamilyMember>();
            foreach (var fammem in socio.ListFamMem) // if there is no familymember id delete row
            {
                if (string.IsNullOrEmpty(fammem.FamilyMemberId)|| !IdValidtion(fammem.FamilyMemberId))
                    fammemtemp.Add(fammem);
            }

            if (fammemtemp != null)
            {
                foreach (var familymem in fammemtemp) // remove all family members from the temp table
                    socio.ListFamMem.Remove(familymem);
            }
            #endregion

            #region family members with finance
            fammemtemp = new List<FamilyMember>();
            List<FamilyStudentFinance> familyfintemp;
            foreach (var famfin in socio.ListFamMemFin) // if there is no familymember id delete row
            {
                if (string.IsNullOrEmpty(famfin.FamilyMemberId) || !IdValidtion(famfin.FamilyMemberId)) // if not correct add to delete list
                {
                    fammemtemp.Add(famfin);
                    continue;
                }
                familyfintemp = new List<FamilyStudentFinance>();
                if (famfin.FamilyStudentFinances != null) // check finance rows
                {
                    foreach (var fin in famfin.FamilyStudentFinances)
                    {
                        if (fin.Year == 0 ||
                            fin.Month == 0)
                            familyfintemp.Add(fin); // add row if not fill month or year to delete list
                    }
                }
                if(familyfintemp != null) // if delete list of finance is not empty delete rows 
                {
                    foreach(var delfin in familyfintemp)
                        famfin.FamilyStudentFinances.Remove(delfin);
                }
            }

            if (fammemtemp != null)
            {
                foreach (var familymem in fammemtemp) // remove all family members finance   from the temp table
                {
                    socio.ListFamMemFin.Remove(familymem);
                }
            }
            #endregion

            return socio;
        } // check all socio to save draft

        private bool socioIsValid(SocioAdd socio)
        {
            var ok = true;
            #region BasicValidtion
            if (string.IsNullOrEmpty(socio.SocioMod.SchoolYear))
            {
                ModelState.AddModelError("SchoolYear", "חובה לציין שנת לימוד");
                ok = false;
            }
            if (socio.SocioMod.Apartment)
            {
                if (socio.SocioMod.FileApartmentLease == null && socio.SocioMod.PathApartmentLease == null)
                {
                    ModelState.AddModelError("SocioMod.FileApartmentLease", "חובה לצרף קובץ");
                    ok = false;
                }
            }
            if (socio.SocioMod.Newcomer)
            {
                if (socio.SocioMod.FileNewcomer == null && socio.SocioMod.PathNewcomer == null)
                {
                    ModelState.AddModelError("SocioMod.FileNewcomer", "חובה לצרף קובץ");
                    ok = false;
                }
            }
            if (socio.SocioMod.SingleParent)
            {
                if (socio.SocioMod.FileSingleParent == null && socio.SocioMod.PathSingleParent == null)
                {
                    ModelState.AddModelError("SocioMod.FileSingleParent", "חובה לצרף קובץ");
                    ok = false;
                }
            }
            if (socio.SocioMod.BereavedFam)
            {
                if (socio.SocioMod.FileBereavedFam == null && socio.SocioMod.PathBereavedFam == null)
                {
                    ModelState.AddModelError("SocioMod.FileBereavedFam", "חובה לצרף קובץ");
                    ok = false;
                }
            }
            if (string.IsNullOrEmpty(socio.SocioMod.MilitaryService))
            {
                ModelState.AddModelError("SocioMod.MilitaryService", "חובה לציין סוג שירות");
                ok = false;
            }
            else // check file of miltary service
            {
                if (socio.SocioMod.FileMilitaryService == null && socio.SocioMod.PathMilitaryService == null)
                {
                    ModelState.AddModelError("SocioMod.FileMilitaryService", "חובה לצרף קובץ");
                    ok = false;
                }
            }
            if (socio.SocioMod.ReserveMilitaryService)
            {
                if (socio.SocioMod.FileReserveMilitaryService == null && socio.SocioMod.PathReserveMilitaryService == null)
                {
                    ModelState.AddModelError("SocioMod.FileReserveMilitaryService", "חובה לצרף קובץ");
                    ok = false;
                }
            }
            if (!string.IsNullOrEmpty(socio.SocioMod.DisabilityType))
            {
                if (socio.SocioMod.FileDisabilityType == null && socio.SocioMod.PathDisabilityType == null)
                {
                    ModelState.AddModelError("SocioMod.FileDisabilityType", "חובה לצרף קובץ");
                    ok = false;
                }
            }

            if (socio.SocioMod.BankStatus == null)
            {
                ModelState.AddModelError("SocioMod.BankStatus", "חובה להזין מצב חשבון");
                ok = false;
            }

            if (socio.SocioMod.FileBankAccount == null && socio.SocioMod.PathBankAccount == null)
            {
                ModelState.AddModelError("SocioMod.FileBankAccount", "חובה לצרף קובץ");
                ok = false;
            }
            #endregion

            #region StudentFinanceValid
            /* בתחילה אני בודק שמצב העבודה הוא לא ריק אם ריק יש לחזור ולתקן
             * לאחר מכן אני בודק את מצב העבודה ועל פי זה מוחק שורות לא רלוונטיות
             * לדוגמא אם הוא עצמאי הוא צריך להגיש טופס אחד ולכן נמחק את 2 השורות האחרונות
             * לאחר מכן נעשת בדיקה על כל שדה ושדה לראות שהשדות תקינות
             */

            if (string.IsNullOrEmpty(socio.SocioMod.WorkSt))
            {
                ModelState.AddModelError("WorkSt", "חובה לציין מצב עבודה");
                ok = false;
            }
            else
            {
                switch (Enum.Parse(typeof(Enums.WorkingStatus), socio.SocioMod.WorkSt))
                {
                    case Enums.WorkingStatus.עצמאי:
                    case Enums.WorkingStatus.חבר_קיבוץ:
                    case Enums.WorkingStatus.לא_עובד:
                    case Enums.WorkingStatus.נכה:
                    case Enums.WorkingStatus.אחר:
                        if (socio.ListStudentFinances.Count() > 1)
                        {
                            do
                            {
                                socio.ListStudentFinances.RemoveAt(1);
                            } while (socio.ListStudentFinances.Count() != 1);
                        }
                        break;
                    case Enums.WorkingStatus.פנסיונר:
                        if (socio.ListStudentFinances.Count() > 2)
                        {
                            do
                            {
                                socio.ListStudentFinances.RemoveAt(1);
                            } while (socio.ListStudentFinances.Count() != 1);
                        }
                        break;
                    default:
                        break;
                }

                foreach (var fin in socio.ListStudentFinances)
                {
                    //until last year
                    if (fin.Year < DateTime.Now.Year - 1 || fin.Year == 0)
                    {
                        ModelState.AddModelError("Year", "שנה לא תקינה");
                        ok = false;
                    }
                    if (fin.Month > 12 && fin.Month < 1)
                    {
                        ModelState.AddModelError("Month", "חודש לא תקין");
                        ok = false;
                    }
                    if (fin.Salary < 0)
                    {
                        ModelState.AddModelError("Salary", "הכנסה לא תקינה");
                        ok = false; 
                    }
                    if (fin.FileSalary == null && fin.PathSalary == null)
                    {
                        ModelState.AddModelError("FileSalary", "חובה לצרף קובץ");
                        ok = false;
                    }
                }
            }
            #endregion

            #region FamilyFinanceValid
            /* בתחילה אני בודק שמצב העבודה הוא לא ריק אם ריק יש לחזור ולתקן
             * לאחר מכן אני בודק את מצב העבודה ועל פי זה מוחק שורות לא רלוונטיות
             * לדוגמא אם הוא עצמאי הוא צריך להגיש טופס אחד ולכן נמחק את 2 השורות האחרונות
             * לאחר מכן נעשת בדיקה על כל שדה ושדה לראות שהשדות תקינות
             */
            foreach (var family in socio.ListFamMemFin)
            {
                bool die = false;

                if (string.IsNullOrEmpty(family.WorkSt))
                {
                    ModelState.AddModelError("WorkSt", "חובה לציין מצב עבודה");
                    ok = false;
                }

                switch (Enum.Parse(typeof(Enums.WorkingStatus), family.WorkSt))
                {
                    case Enums.WorkingStatus.עצמאי:
                    case Enums.WorkingStatus.חבר_קיבוץ:
                    case Enums.WorkingStatus.לא_עובד:
                    case Enums.WorkingStatus.נכה:
                    case Enums.WorkingStatus.אחר:
                        if (family.FamilyStudentFinances.Count() > 1)
                        {
                            do
                            {
                                family.FamilyStudentFinances.RemoveAt(1);
                            } while (family.FamilyStudentFinances.Count() != 1);
                        }
                        break;
                    case Enums.WorkingStatus.נפטר:
                        if (family.FamilyStudentFinances.Count() > 1)
                        {
                            do
                            {
                                family.FamilyStudentFinances.RemoveAt(1);
                            } while (family.FamilyStudentFinances.Count() != 1);
                        }
                        die = true;
                        break;
                    case Enums.WorkingStatus.פנסיונר:
                        if (family.FamilyStudentFinances.Count() > 2)
                        {
                            do
                            {
                                family.FamilyStudentFinances.RemoveAt(1);
                            } while (family.FamilyStudentFinances.Count() != 1);
                        }
                        break;
                    default:
                        break;
                }

                if (family.FileFamId == null && family.PathFmId == null)
                {
                    ModelState.AddModelError("FileFamId", "חובה לצרף קובץ");
                    ok = false;
                }

                /* כעת אני לוקח את השנים והחודשים שם אותם במערך דו ממדי
                 * ובודק אם הם כפולים גם השנה וגם החודש אם כן זאת שגיאה ולכן חוזר
                 */
                /*
               int[,] matYM = new int[2,3];
               for (int i = 0; i < family.FamilyStudentFinances.Count; i++)
               {
                   matYM[0, i] = family.FamilyStudentFinances[i].Year;
                   matYM[1, i] = family.FamilyStudentFinances[i].Month;
               }

               for (int i = 0; i < family.FamilyStudentFinances.Count - 1; i++)
               {
                   if (matYM[0, i] == matYM[0, i + 1] && matYM[1, i] == matYM[1, i + 1])
                       return false;
               }
               */

                foreach (var fin in family.FamilyStudentFinances)
                {
                    //until last year
                    if ((fin.Year < DateTime.Now.Year - 1 || fin.Year == 0) && !die)
                    {
                        ModelState.AddModelError("Year", "שנה לא תקינה");
                        ok = false;
                    }
                    if (fin.Month > 12 && fin.Month < 1)
                    {
                        ModelState.AddModelError("Month", "חודש לא תקין");
                        ok = false;
                    }
                    if (fin.Salary < 0)
                    {
                        ModelState.AddModelError("Salary", "הכנסה לא תקינה");
                        ok = false; 
                    }
                    if (fin.FileSalary == null && fin.PathSalary == null)
                    {
                        ModelState.AddModelError("FileSalary", "חובה לצרף קובץ");
                        ok = false;
                    }
                }
            }
            #endregion

            #region Check Funding Valid
            // check funding valid
            if (socio.SocioMod.HasFunding)
            {
                foreach(var fund in socio.ListFundings)
                {
                    if(string.IsNullOrEmpty(fund.FinancingInstitution))
                    {
                        ModelState.AddModelError("FinancingInstitution", "חובה לציין גוף ממן");
                        ok = false;
                    }
                    if (fund.FinancingHeight <= 0)
                    {
                        ModelState.AddModelError("FinancingHeight", "חובה לציין גובה מימון");
                        ok = false;
                    }
                    if (fund.YearFinancing == null)
                    {
                        ModelState.AddModelError("YearFinancing", "חובה לציין שנת מימון");
                        ok = false;
                    }
                }
            }
            #endregion

            #region CarStudentValid
            if (socio.SocioMod.CarOwner)
            {
                foreach (var car in socio.ListCarStudent)
                {
                    if(car.CarNumber.Length < 6 && car.CarNumber.Length > 8)
                    {
                        ModelState.AddModelError("CarNumber", "מספר רכב לא תקין");
                        ok = false;
                    }
                    if(string.IsNullOrEmpty(car.CarCompany))
                    {
                        ModelState.AddModelError("CarCompany", "חובה לציין יצרן רכב");
                        ok = false;
                    }
                    if (string.IsNullOrEmpty(car.CarModel))
                    {
                        ModelState.AddModelError("CarModel", "חובה לציין מודל רכב");
                        ok = false;
                    }
                    if (car.CarYear < 0 || car.CarYear == 0)
                    {
                        ModelState.AddModelError("CarYear", "חובה לציין שנת רכב");
                        ok = false;
                    }
                    if(car.FileCarLicense == null && car.PathCarLicense == null)
                    {
                        ModelState.AddModelError("FileCarLicense", "חובה לצרף קובץ");
                        ok = false;
                    }
                }
            }

            #endregion

            #region FamilyMemeberValid
            foreach(var family in socio.ListFamMem)
            {
                if(!IdValidtion(family.FamilyMemberId))
                {
                    ModelState.AddModelError("FamilyMemberId", "תז לא תקין");
                    ok = false;
                }

                if(string.IsNullOrEmpty(family.Name))
                {
                    ModelState.AddModelError("Name", "חובה לציין שם");
                    ok = false;
                }
                if (string.IsNullOrEmpty(family.Realationship))
                {
                    ModelState.AddModelError("Realationship", "חובה לציין קשר משפחתי");
                    ok = false;
                }
                if (string.IsNullOrEmpty(family.Gender))
                {
                    ModelState.AddModelError("Gender", "חובה לציין מגדר");
                    ok = false;
                }
                if(family.BirthDay == null)
                {
                    ModelState.AddModelError("BirthDay", "חובה להזין תאריך לידה");
                    ok = false;
                }
                if (family.FileFamId == null && family.PathFmId == null)
                {
                    ModelState.AddModelError("FileFamId", "חובה לצרף קובץ");
                    ok = false;
                }
            }

            #endregion

            return ok;
        }

        private bool IdValidtion(string strID)
        {
            /* עדיין לא נבדק*/
            int[] id_12_digits = { 1, 2, 1, 2, 1, 2, 1, 2, 1 };
            int count = 0;

            if (strID == null)
                return false;

            strID = strID.PadLeft(9, '0');

            for (int i = 0; i < 9; i++)
            {
                int num = Int32.Parse(strID.Substring(i, 1)) * id_12_digits[i];

                if (num > 9)
                    num = (num / 10) + (num % 10);

                count += num;
            }
            return (count % 10 == 0);
        }

        #region Save socio models
        [NonAction]
        public void SaveSocioCars(List<CarStudent> ClientList , int SpId)
        {
            List<CarStudent> DbList;
            CarStudent TempDbCar;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                // list of cars from database
                DbList = ctx.CarStudents.Where(s => s.StudentId == sStudentId && s.SpId == SpId).ToList();
                // update each car that posted from client
                foreach (var car in ClientList)
                {
                    car.StudentId = sStudentId;// insert the id of student to every car
                    car.SpId = SpId;           // insert spid to every car     
                                               //find the car in the list by carNumber
                    TempDbCar = DbList.Where(s => s.CarNumber == car.CarNumber).FirstOrDefault();
                    //if it was change at carNumber reset the file path
                    if (TempDbCar == null)
                        car.PathCarLicense = null;

                    // if there is a file upload and update the file path
                    if (car.FileCarLicense != null)
                        car.PathCarLicense = Files.SaveFileInServer(car.FileCarLicense, "Car" + car.CarNumber, sStudentId, car.PathCarLicense);

                    if (TempDbCar != null)
                        ctx.Entry(TempDbCar).CurrentValues.SetValues(car);// update car exists
                    else
                        ctx.CarStudents.Add(car); // add new car to database    

                    ctx.SaveChanges();
                }
            }

            // אם נמצא בשרת ולא נמצא עם הרשימה שחזרה מהקליינט אז תמחק את הרשומה
            foreach (var car in DbList)
            {
                TempDbCar = ClientList.Where(s => s.CarNumber == car.CarNumber).FirstOrDefault();
                if (TempDbCar == null)
                    DeleteCar(car.CarNumber);
            }
        }

        [NonAction]
        public void SaveFundings(List<Funding> ClientList, int SpId)
        {
            List<Funding> DbList;
            Funding TempDbFund;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                // list of funding by database
                DbList = ctx.Fundings.Where(s => s.StudentId == sStudentId && s.SpId == SpId).ToList();
                // update each fund that posted from client
                foreach (var fund in ClientList)
                {
                    fund.StudentId = sStudentId;// insert ths id of student to every fund
                    fund.SpId = SpId; // insert spid to every fund
                                      //find the fund in the list by funding id and the student id
                    TempDbFund = DbList.Where(s => s.FundingId == fund.FundingId && s.StudentId == sStudentId).FirstOrDefault();

                    if (TempDbFund != null)
                        ctx.Entry(TempDbFund).CurrentValues.SetValues(fund);// update fund exists
                    else
                        ctx.Fundings.Add(fund); // add new fund to database    

                    ctx.SaveChanges();

                    // if there is a file upload and update the file path
                    if (fund.FileFunding != null)
                    {
                        fund.PathFunding = Files.SaveFileInServer(fund.FileFunding, "Fund" + fund.FundingId, sStudentId, fund.PathFunding);
                        if(TempDbFund != null)
                            TempDbFund.PathFunding = fund.PathFunding;
                    }
                    ctx.SaveChanges();
                }
            }
        }

        [NonAction]
        public void SaveStudentFinance(List<StudentFinance> ClientList, int SpId)
        {
            /* בתחילה אני מסיר את כל הנתונים מהשרת תוך כדי שמירה על קבצים
                     * לאחר מכן רץ על הרשימה שהתקבלה מהלקוח
                     * משתיל את פרטי הסטודנט וקוד מלגה
                     * סוחב את הנתונים מהשרת לתוך רשימה חדשה
                     * ושולף את השורה שבה השנה והוחודש שווים לאלמנט שנמצא עכשיו
                     * אם קיים יש כפל ולכן אי אפשר לשמור את המידע הזה ומדלגים עליו
                     * הבעיה שנוצרה היא הקובץ קיים בשרת ולא נמחק אך מאבדים את מיקומו
                     * פתרון אפשרי: בעת המציאה למחוק אותו באמצעות פונקציה קיימת
                     */
            List<StudentFinance> DbList;
            StudentFinance TempDb;
            string[] PathFinance = new string[3]; // holding path salary;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                // list of student finance by database
                DbList = ctx.StudentFinances.Where(s => s.StudentId == sStudentId && s.SpId == SpId).ToList();

                foreach (var finDb in DbList)
                {
                    if (finDb.FinNo < 0 || finDb.FinNo > 2) continue;
                    PathFinance[finDb.FinNo] = finDb.PathSalary;
                    ctx.StudentFinances.Remove(finDb);
                }
                ctx.SaveChanges();
                foreach (var fin in ClientList)
                {
                    fin.StudentId = sStudentId;
                    fin.SpId = SpId;

                    // list of finance by database
                    DbList = ctx.StudentFinances.Where(s => s.StudentId == sStudentId && s.SpId == SpId).ToList();
                    TempDb = DbList.Where(s => s.Month == fin.Month && s.Year == fin.Year).FirstOrDefault();
                    if (TempDb != null) continue;

                    if (fin.FinNo >= 0 && fin.FinNo <= 2)
                        fin.PathSalary = PathFinance[fin.FinNo];

                    // if there is a salary file upload and update the file path
                    if (fin.FileSalary != null)
                        fin.PathSalary = Files.SaveFileInServer(fin.FileSalary, "Salary" + fin.FinNo, sStudentId, fin.PathSalary);

                    ctx.StudentFinances.Add(fin);
                    ctx.SaveChanges();
                }
            }
        }

        [NonAction]
        public void SaveFamilyMemberFinance(List<FamilyMember> ClientList, int SpId)
        {
            List<FamilyStudentFinance> dbFamFinance;
            FamilyStudentFinance tempDbFamFin;
            List<FamilyMember> dbFamMem; // hold db family member only with finance
            FamilyMember tempDbFamMem;
            string[] PathFinance = new string[3]; // holding path salary;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                dbFamMem = ctx.FamilyMembers.Where(s => s.StudentId == sStudentId).Where(s => s.Realationship == Enums.Realationship.אב.ToString() ||
                             s.Realationship == Enums.Realationship.אם.ToString() ||
                             s.Realationship == Enums.Realationship.בעל.ToString() ||
                             s.Realationship == Enums.Realationship.אישה.ToString()).ToList(); // filter only dad mom and wife/husband

                foreach (var mem in ClientList)
                {
                    if (mem.FamilyMemberId == null) // if there is no id dont save even if draft
                        continue;
                    /*Start finance*/
                    dbFamFinance = ctx.FamilyStudentFinances.Where(s => s.FamilyMemberId == mem.FamilyMemberId && s.SpId == SpId).ToList();
                    foreach (var finDb in dbFamFinance)
                    {
                        if (finDb.FinNo < 0 || finDb.FinNo > 2) continue;
                        PathFinance[finDb.FinNo] = finDb.PathSalary;
                        ctx.FamilyStudentFinances.Remove(finDb);
                    }
                    ctx.SaveChanges();

                    // save or update finance of the family member
                    foreach (var fin in mem.FamilyStudentFinances)
                    {
                        if (fin.Year == 0 || fin.Month == 0) continue;

                        fin.FamilyMemberId = mem.FamilyMemberId;
                        fin.SpId = SpId;

                        // list of finance by database
                        dbFamFinance = ctx.FamilyStudentFinances.Where(s => s.FamilyMemberId == mem.FamilyMemberId && s.SpId == SpId).ToList();
                        tempDbFamFin = dbFamFinance.Where(s => s.Month == fin.Month && s.Year == fin.Year).FirstOrDefault();
                        if (tempDbFamFin != null) continue;

                        if (fin.FinNo >= 0 && fin.FinNo <= 2)
                        {
                            fin.PathSalary = PathFinance[fin.FinNo];
                        }

                        // if there is a salary file upload and update the file path
                        if (fin.FileSalary != null)
                            fin.PathSalary = Files.SaveFileInServer(fin.FileSalary, "Salary-" + mem.FamilyMemberId + "-" + fin.FinNo, sStudentId, fin.PathSalary);

                        ctx.FamilyStudentFinances.Add(fin);
                    }
                    /*end finance*/

                    mem.StudentId = sStudentId;
                    tempDbFamMem = dbFamMem.Where(s => s.FamilyMemberId == mem.FamilyMemberId).FirstOrDefault(); // find if this family member has exist in db

                    // save or update family member
                    if (mem.FileFamId != null) // there is new file to save on server
                        mem.PathFmId = Files.SaveFileInServer(mem.FileFamId, "Id" + mem.FamilyMemberId, mem.StudentId, mem.PathFmId); // save id family member
                    if (tempDbFamMem != null)
                    {
                        dbFamMem.Remove(tempDbFamMem); // remove from list of db
                        ctx.Entry(tempDbFamMem).CurrentValues.SetValues(mem);// update family member exists
                        ctx.SaveChanges();
                    }
                    else
                        ctx.FamilyMembers.Add(mem);
                    ctx.SaveChanges();

                    foreach (var DBmem in dbFamMem) // after what has been remain in this table needs to remove
                    {
                        DeleteFamMem(DBmem.FamilyMemberId);
                    }
                }
            }
        }

        [NonAction]
        public void SaveFamilyMember(List<FamilyMember> ClientList)
        {
            List<FamilyMember> dbFamMem;
            FamilyMember tempFam;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                // get from db all family members without finance
                dbFamMem = ctx.FamilyMembers.Where(s => s.StudentId == sStudentId)
                    .Where(s => s.Realationship == Enums.Realationship.אח.ToString() ||
                    s.Realationship == Enums.Realationship.אחות.ToString() ||
                    s.Realationship == Enums.Realationship.בן.ToString() ||
                    s.Realationship == Enums.Realationship.בת.ToString()).ToList(); // filter only sons and brothers and sisters

                foreach (var mem in ClientList)
                {
                    mem.StudentId = sStudentId;
                    if (mem.FileFamId != null) // if there is file to upload
                        mem.PathFmId = Files.SaveFileInServer(mem.FileFamId, "Family" + mem.FamilyMemberId, sStudentId, mem.PathFmId);

                    tempFam = dbFamMem.Where(s => s.FamilyMemberId == mem.FamilyMemberId).FirstOrDefault(); // find if it is in db already
                    if (tempFam != null)
                    {
                        dbFamMem.Remove(tempFam); // remove from list of db
                        ctx.Entry(tempFam).CurrentValues.SetValues(mem);// update family member exists
                    }
                    else
                        ctx.FamilyMembers.Add(mem);

                    ctx.SaveChanges();
                }

                foreach (var DBmem in dbFamMem) // after what has been remain in this table needs to remove
                {
                    DeleteFamMem(DBmem.FamilyMemberId);
                }
            }
        }

        [NonAction]
        public SpSocio SaveSocioModel(SpSocio socio)
        {
            SpSocio Dbsocio;
            socio.StudentId = sStudentId; // bind student id to socio model

            if (socio.FileApartmentLease != null) // save file if not null
                socio.PathApartmentLease = Files.SaveFileInServer(socio.FileApartmentLease, "ApartmentLease", sStudentId, socio.PathApartmentLease);

            if (socio.FileBereavedFam != null) // save file if not null
                socio.PathBereavedFam = Files.SaveFileInServer(socio.FileBereavedFam, "BereavedFam", sStudentId, socio.PathBereavedFam);

            if (socio.FileDisabilityType != null) // save file if not null
                socio.PathDisabilityType = Files.SaveFileInServer(socio.FileDisabilityType, "DisabilityType", sStudentId, socio.PathDisabilityType);

            if (socio.FileMilitaryService != null) // save file if not null
                socio.PathMilitaryService = Files.SaveFileInServer(socio.FileMilitaryService, "MilitaryService", sStudentId, socio.PathMilitaryService);

            if (socio.FileNewcomer != null) // save file if not null
                socio.PathNewcomer = Files.SaveFileInServer(socio.FileNewcomer, "Newcomer", sStudentId, socio.PathNewcomer);

            if (socio.FileReserveMilitaryService != null) // save file if not null
                socio.PathReserveMilitaryService = Files.SaveFileInServer(socio.FileReserveMilitaryService, "ReserveMilitaryService", sStudentId, socio.PathReserveMilitaryService);

            if (socio.FileSingleParent != null) // save file if not null
                socio.PathSingleParent = Files.SaveFileInServer(socio.FileSingleParent, "SingleParent", sStudentId, socio.PathSingleParent);

            if (socio.FileBankAccount != null) // save file if not null
                socio.PathBankAccount = Files.SaveFileInServer(socio.FileBankAccount, "BankAccount", sStudentId, socio.PathBankAccount);


            using (DikanDbContext ctx = new DikanDbContext())
            {
                Dbsocio = ctx.Socio.Where(s => s.StudentId == socio.StudentId && s.ScholarshipId == socio.ScholarshipId).SingleOrDefault(); // find if he insert already draft     
                if (Dbsocio != null)
                    ctx.Socio.Remove(Dbsocio); // remove previous socio
                ctx.SaveChanges();
                ctx.Socio.Add(socio); // add new sp socio

                ctx.SaveChanges();
            }
            return socio;
        }
        #endregion

        #endregion

        #endregion

        #region Partial Views
        [Authorize(Roles ="Student")]
        public PartialViewResult CarsView()
        {
            ViewBag.YearsList = new SelectList(YearsSelectList(), null, "Text"); // to show years list in drop down
            return PartialView("CarsView", new CarStudent());
        }

        [Authorize(Roles = "Student")]
        public PartialViewResult FundView()
        {
            ViewBag.YearsList = new SelectList(YearsSelectList(), null, "Text"); // to show years list in drop down
            return PartialView("FundView", new Funding());
        }

        [Authorize(Roles = "Student")]
        [ChildActionOnly]
        public PartialViewResult StudFinView()
        {
            ViewBag.YearsList = new SelectList(YearsSelectList(), null, "Text"); // to show years list in drop down
            return PartialView("StudFinView", new StudentFinance());
        }

        [Authorize(Roles = "Student")]
        [ChildActionOnly]
        public PartialViewResult FamMemView() // family member with finance
        {
            ViewBag.YearsList = new SelectList(YearsSelectList(), null, "Text"); // to show years list in drop down
            return PartialView("FamMemView", new FamilyMember());
        }

        [Authorize(Roles = "Student")]
        [ChildActionOnly]
        public PartialViewResult FamilyFinView(string containerPrefix) // finance of family member
        {
            ViewData["ContainerPrefix"] = containerPrefix;
            return PartialView("FamilyFinView", new FamilyStudentFinance());
        }

        [Authorize(Roles = "Student")]
        public PartialViewResult FamilyView() // family member
        {
            return PartialView("FamilyView", new FamilyMember());
        }

        #endregion

        #region Delete Rows Functions

        [Authorize(Roles = "Student")]
        public ActionResult DeleteCar(string CarNum)
        {
            CarStudent tempcar;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                tempcar = ctx.CarStudents.Where(s => s.CarNumber == CarNum && s.StudentId == sStudentId).FirstOrDefault();
                if (tempcar == null)
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Not Found In DataBase");
                //if file exists delete it
                if (!string.IsNullOrEmpty(tempcar.PathCarLicense))
                    Files.Delete(tempcar.PathCarLicense, sStudentId);
                ctx.CarStudents.Remove(tempcar);
                ctx.SaveChanges();
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [Authorize(Roles = "Student")]
        public ActionResult DeleteFund(string FundId)
        {
            Funding tempfund;
            int pFundId = int.Parse(FundId);
            using (DikanDbContext ctx = new DikanDbContext())
            {
                tempfund = ctx.Fundings.Where(s => s.FundingId == pFundId && s.StudentId == sStudentId).FirstOrDefault();
                if (tempfund == null)
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Not Found In DataBase");
                //if file exists delete it
                if (!string.IsNullOrEmpty(tempfund.PathFunding))
                    Files.Delete(tempfund.PathFunding, sStudentId);
                ctx.Fundings.Remove(tempfund);
                ctx.SaveChanges();
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [Authorize(Roles = "Student")]
        public ActionResult DeleteFamMem(string FamilyId) // delete row family member
        {
            FamilyMember tempfamily;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                tempfamily = ctx.FamilyMembers.Where(s => s.FamilyMemberId == FamilyId && s.StudentId == sStudentId).FirstOrDefault();
                if (tempfamily == null)
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Not Found In DataBase");
                //if file exists delete it
                if (!string.IsNullOrEmpty(tempfamily.PathFmId))
                    Files.Delete(tempfamily.PathFmId, sStudentId);
                ctx.FamilyMembers.Remove(tempfamily);
                ctx.SaveChanges();
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
        #endregion

        #endregion

        #region Scholarship Exception Request

        [HttpGet]
        [Authorize(Roles ="Student")]
        public ActionResult SpExRequest(string Id, string SpId) // open link for edit sp after deadline date end
        {
            if (Id == null || SpId == null) // error no parameters
                return RedirectToAction("Login", "Login", null);
            bool ok = int.TryParse(SpId, out int iSpId); // try to parse spid to int
            if(!ok)
                return RedirectToAction("Login", "Login", null);
            using (DikanDbContext ctx = new DikanDbContext())
            {
                var Ex = ctx.SpExceptions.Where(s => s.UserId == Id && s.SpId == iSpId).FirstOrDefault(); // find if the exception is gueniue
                if(Ex == null || Ex.LockDate < DateTime.Now) // no row found or the date has over -> return to login page
                    return RedirectToAction("Login", "Login", null);
                var sptype = Enum.Parse(typeof(Enums.SpType),ctx.SpDef.Where(s => s.ScholarshipID == Ex.SpId).FirstOrDefault().Type); // get the type of spdef that match to spid
                var user = UserManager.FindById(Id);
                if(user == null)
                    return RedirectToAction("Login", "Login", null);
                switch (sptype) // redirect to sp according to type
                {
                    case Enums.SpType.הלכה:
                        var halacha = ctx.Halacha.Where(s => s.ScholarshipId == Ex.SpId && s.StudentId == user.UserName).FirstOrDefault();
                        if (halacha != null)
                            return RedirectToAction("Halacha", new { scholarshipid = iSpId, open = true });
                        break;
                    case Enums.SpType.מצוינות:
                        var metsuyanut = ctx.Excellence.Where(s => s.ScholarshipId == Ex.SpId && s.StudentId == user.UserName).FirstOrDefault();
                        if (metsuyanut != null)
                            return RedirectToAction("Excellent", new { scholarshipid = iSpId, open = true });
                        break;
                    case Enums.SpType.סוציואקונומית:
                        var socio = ctx.Socio.Where(s => s.ScholarshipId == Ex.SpId && s.StudentId == user.UserName).FirstOrDefault();
                        if (socio != null)
                            return RedirectToAction("Socio", new { scholarshipid = iSpId, open = true });
                        break;
                    default:
                        break;
                }
            }
            return RedirectToAction("Login", "Login", null);
        }

        #endregion

        #region Save Signature
        [HttpPost]
        [Authorize(Roles = "Student")]
        public ActionResult SaveSignature(string pDataUri, string pName)
        {
            var ok = Files.signatureSave(pDataUri, pName, sStudentId);
            return new HttpStatusCodeResult(ok ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
        }
        #endregion

        #region NonActions

        [NonAction]
        [Authorize(Roles = "Student")]
        public List<VolunteerPlaces> SetsvolunteerPlaces() // add to name and desc to name_desc field
        {
            List<VolunteerPlaces> places = new List<VolunteerPlaces>();
            using (DikanDbContext ctx = new DikanDbContext())
            {
                places = ctx.VolunteerPlaces.Where(s=>s.Active == true).ToList(); // only active places
                foreach (var place in places.ToList()) // sets name+desc field to show in drop down list
                {
                    place.Name_desc = place.Name + " - " + place.Desc;
                }
            }
            return places;
        }

        [NonAction]
        [Authorize(Roles = "Student")]
        public List<SelectListItem> YearsSelectList()
        {
            List<SelectListItem> years = new List<SelectListItem>();
            for (int year = DateTime.Now.Year; year >= 1980; year--)
            {
                years.Add(new SelectListItem { Text = year.ToString(), Value = year.ToString() });
            }
            return years;
        }

        [NonAction]
        [Authorize(Roles = "Student")]
        public List<SelectListItem> MonthsSelectList()
        {
            List<SelectListItem> months = new List<SelectListItem>();
            for (int month = 1; month <= 12; month++)
            {
                months.Add(new SelectListItem { Text = month.ToString(), Value = month.ToString() });
            }
            return months;
        }

        #endregion

        #region Upload File Ajax

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadFile(HttpPostedFileBase filee, string fileName, string oldFile)
        {
            if (filee == null || string.IsNullOrEmpty(fileName))
            {
                Response.StatusCode = 400;
                return Content("Not send file or name", "text/plain");
            }
            var file = Request.Files[0];
            string path;
            path = Files.SaveFileInServer(filee, fileName, sStudentId, oldFile);
            Response.StatusCode = 200;
            return Content(path, "text/plain");
        }

        #endregion

    }
}