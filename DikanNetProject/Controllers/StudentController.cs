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

namespace DikanNetProject.Controllers
{
    [RequireHttps]
    public class StudentController : Controller
    {
        string StudentId;
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.HttpContext.Session["Student"] == null || Session == null) // if there is no session -> no access to student interface
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Login", controller = "Login" }));
            }
            else
                StudentId = ((Users)Session["Student"]).UserId;
        }

        public ActionResult Index()
        {
            StudentMain studentMain = new StudentMain();
            using (DikanDbContext ctx = new DikanDbContext())
            {
                 studentMain.ScholarshipDefinitions = ctx.ScholarshipDefinitions.Where(x => DbFunctions.TruncateTime(x.DateDeadLine) > DbFunctions.TruncateTime(DateTime.Now) && DbFunctions.TruncateTime(x.DateOpenScholarship) < DbFunctions.TruncateTime(DateTime.Now)).ToList();
                 foreach(var scholarship in studentMain.ScholarshipDefinitions.ToList()) // dont show scholarship that already is submited
                {
                    switch(scholarship.Type)
                    {
                        case 1: if(ctx.Socioeconomics.Any(s=>s.ScholarshipId == scholarship.ScholarshipID && s.StudentId == StudentId))
                                    studentMain.ScholarshipDefinitions.Remove(scholarship);
                            break;

                        case 2:
                            if (ctx.ExcellenceStudent.Any(s => s.ScholarshipId == scholarship.ScholarshipID && s.StudentId == StudentId))
                                studentMain.ScholarshipDefinitions.Remove(scholarship);
                            break;

                        case 3:
                            if (ctx.InPractice.Any(s => s.ScholarshipId == scholarship.ScholarshipID && s.StudentId == StudentId))
                                studentMain.ScholarshipDefinitions.Remove(scholarship);
                            break;
                    }

                }
                 // send only scholarship that belongs to student id
                 studentMain.InPracticeList = ctx.InPractice.Include(s=>s.ScholarshipDefinition).Where(s => s.StudentId == StudentId).ToList(); // send to view inpractice list of student
                 studentMain.ExcelList = ctx.ExcellenceStudent.Include(s=>s.ScholarshipDefinition).Where(s => s.StudentId == StudentId).ToList(); // send to view excellence list of student
                 studentMain.SocioList = ctx.Socioeconomics.Include(s=>s.ScholarshipDefinition).Where(s => s.StudentId == StudentId).ToList(); // send to view socio list of student
            }
            
            return View(studentMain);
        }

        #region Update Info Student

        [HttpGet]
        public ActionResult UpdateStudent()
        {
                Student student;
                using (DikanDbContext ctx = new DikanDbContext())
                {
                    ViewBag.MajorsList = new SelectList(ctx.Majors.ToList(), "MajorId", "MajorName"); // to show majors list in drop down
                    ViewBag.CountriesList = new SelectList(ctx.Countries.ToList(), "CountryId", "CountryName"); // to show countries list in drop down
                    ViewBag.CitiesList = new SelectList(ctx.Cities.ToList(), "Id", "Name"); // to show cities list in drop down
                    
                    student = ctx.Students.Where(z => z.StudentId == StudentId).FirstOrDefault();
                    if (student == null) // didnt found in student table -> first login -> update basic info
                    {
                        student = new Student
                        {
                            StudentId = StudentId,
                            Email = ((Users)Session["Student"]).Email,
                            FirstName = ((Users)Session["Student"]).FirstName,
                            LastName = ((Users)Session["Student"]).LastName
                        };
                    }
                    else
                    {
                    }
                }
                return View(student);
        }

        [HttpPost]
        public ActionResult UpdateStudent(Student UpdateStudent)
        {
            UpdateStudent.StudentId = StudentId;
            if (ModelState.IsValid)
            {
                using (DikanDbContext ctx = new DikanDbContext())
                {
                    Student dbStudent = ctx.Students.Find(UpdateStudent.StudentId);
                    if (dbStudent != null) 
                    {
                        Users tempuser = ctx.Users.Find(UpdateStudent.StudentId); // get the student user
                        if (UpdateStudent.Email != tempuser.Email) // check if the student has change the email address
                        {
                            tempuser.Email = UpdateStudent.Email; // update email address in user list
                            tempuser.ActivationCode = Guid.NewGuid(); // generate new guid for activate the new email address
                            tempuser.IsEmailVerified = false; // the email need to be verified
                            // send activation email to user
                            var body = "לחץ על התמונה לאימות החשבון";
                            var username = tempuser.FirstName + " " + tempuser.LastName;
                            var verifyUrl = "/Login/" + "VerifyAccount/" + tempuser.ActivationCode.ToString();
                            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
                            body = SendMail.CreateBodyEmail(username, link, body);
                            SendMail.SendEmailLink(tempuser.Email, body, "אימות חשבון - דיקאנט");
                        }
                        
                    }

                    if(UpdateStudent.IdFile != null)
                        UpdateStudent.FileId = SaveFile.SaveFileInServer(UpdateStudent.IdFile, "Id", UpdateStudent.StudentId,(dbStudent == null) ? null:dbStudent.FileId);

                    if (dbStudent == null) // after first login fill more info
                        ctx.Students.Add(UpdateStudent); // add student to data base
                    else
                        ctx.Entry(dbStudent).CurrentValues.SetValues(UpdateStudent);// update student
                    ctx.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
            using (DikanDbContext ctx = new DikanDbContext())
            {
                ViewBag.MajorsList = new SelectList(ctx.Majors.ToList(), "MajorId", "MajorName"); // to show majors list in drop down
                ViewBag.CountriesList = new SelectList(ctx.Countries.ToList(), "CountryId", "CountryName"); // to show countries list in drop down
                ViewBag.CitiesList = new SelectList(ctx.Cities.ToList(), "Id", "Name"); // to show cities list in drop down
            }
            return View(UpdateStudent);
        }

        #endregion

        public ActionResult RedirectToScholarship(int scholarshipid)
        {
            int type = -1;
            using(DikanDbContext ctx = new DikanDbContext())
            {
                ScholarshipDefinition temp = ctx.ScholarshipDefinitions.Find(scholarshipid);
                type = temp.Type;
            }
            switch (type)
            {
                case 1: return RedirectToAction("Socio",new { scholarshipid }); // type 1 is socio scholarship

                case 2:
                    return RedirectToAction("Excellent", new { scholarshipid }); // type 2 is metuyanut scholarship

                case 3:
                    return RedirectToAction("Halacha", new { scholarshipid }); // type 3 is halacha scholarship

                default: return RedirectToAction("Index"); // index of srudent if not found type
            }
        }

        #region Halacha Lemaase Scholarship

        [HttpGet]
        public ActionResult Halacha(int scholarshipid)
        {
            InPractice temphalacha;
            ViewBag.VolunteerPlacesList = new SelectList(SetsvolunteerPlaces(), "Id", "Name_desc"); // to show volunteer places list in drop down
            using (DikanDbContext ctx = new DikanDbContext())
            {
                temphalacha = ctx.InPractice.Where(s => s.StudentId == StudentId && s.ScholarshipId == scholarshipid).SingleOrDefault();
            }
            if (temphalacha == null) // checks if is it first time sign to scholarship or has save draft
            {
                temphalacha = new InPractice
                {
                    ScholarshipId = scholarshipid,
                    StudentId = StudentId
                };
            }else
            {
                if (temphalacha.DateSubmitScholarship != null) // if already has entered this milga
                    return RedirectToAction("Index");
            }
            return View(temphalacha);
        }
        

        [HttpPost]
        public ActionResult Halacha(InPractice temphalacha, string uploadmethod) // submit  new halacha scholarship
        {
            temphalacha.StudentId = StudentId;
            InPractice Studentinpractice;
            ViewBag.VolunteerPlacesList = new SelectList(SetsvolunteerPlaces(), "Id", "Name_desc"); // to show volunteer places list in drop down
            using (DikanDbContext ctx = new DikanDbContext())
            {
                Studentinpractice = ctx.InPractice.Where(s => s.StudentId == temphalacha.StudentId && s.ScholarshipId == temphalacha.ScholarshipId).SingleOrDefault(); // find if he insert already draft     
                if (uploadmethod.Equals("הגש מלגה"))
                {// submit scholarship
                    if (ModelState.IsValid)
                    {
                        temphalacha.Statuss = Enums.Status.בטיפול.ToString(); // insert status betipul
                        temphalacha.StatusUpdateDate = temphalacha.DateSubmitScholarship = DateTime.Now; // insert date submit + update status
                    }
                    else // if not all required fields are fill
                        return View(temphalacha);
                }
                if (Studentinpractice == null)
                {
                    ctx.InPractice.Add(temphalacha); // if it first time insert new
                }
                else
                    ctx.Entry(Studentinpractice).CurrentValues.SetValues(temphalacha); // update
                ctx.Configuration.ValidateOnSaveEnabled = false;
                ctx.SaveChanges();
            }
            return View(temphalacha);
        }
        #endregion

        #region Excellent Scholarship

        [HttpGet]
        public ActionResult Excellent(int scholarshipid)
        {
            ExcellenceStudent tempmetsuyanut;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                tempmetsuyanut = ctx.ExcellenceStudent.Where(s => s.StudentId == StudentId && s.ScholarshipId == scholarshipid).SingleOrDefault();
            }
            if (tempmetsuyanut == null) // checks if is it first time sign to scholarship or has save draft
            {
                tempmetsuyanut = new ExcellenceStudent
                {
                    ScholarshipId = scholarshipid,
                    StudentId = StudentId
                };
            }
            else
            {
                if (tempmetsuyanut.DateSubmitScholarship != null) // if already has entered this milga
                    return RedirectToAction("Index");
            }
            return View(tempmetsuyanut);
        }


        [HttpPost]
        public ActionResult Excellent(ExcellenceStudent tempmetmesuyanut, string uploadmethod) // submit  new metsuyanut scholarship
        {
            tempmetmesuyanut.StudentId = StudentId;
            ExcellenceStudent StudentMetsuyanut;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                StudentMetsuyanut = ctx.ExcellenceStudent.Where(s => s.StudentId == tempmetmesuyanut.StudentId && s.ScholarshipId == tempmetmesuyanut.ScholarshipId).SingleOrDefault(); // find if he insert already draft     
                if (uploadmethod.Equals("הגש מלגה"))
                {// submit scholarship
                    if (ModelState.IsValid)
                    {
                        tempmetmesuyanut.Statuss = Enums.Status.בטיפול.ToString(); // insert status betipul
                        tempmetmesuyanut.StatusUpdateDate = tempmetmesuyanut.DateSubmitScholarship = DateTime.Now; // insert date submit + update status
                    }
                    else
                        return View(tempmetmesuyanut);
                }
                if (StudentMetsuyanut == null)
                {
                    ctx.ExcellenceStudent.Add(tempmetmesuyanut); // if it first time insert new
                }
                else
                    ctx.Entry(StudentMetsuyanut).CurrentValues.SetValues(tempmetmesuyanut); // update
                ctx.Configuration.ValidateOnSaveEnabled = false;
                ctx.SaveChanges();
            }
            return View(tempmetmesuyanut);
        }
        #endregion

        #region SocioEconomic Scholarship

        [HttpGet]
        public ActionResult Socio(int scholarshipid)
        {
            ViewBag.YearsList = new SelectList(YearsSelectList(), null, "Text"); // to show volunteer places list in drop down
            SocioAdd socio = new SocioAdd
            {
                SocioMod = new Socioeconomic(),
                ListCarStudent = new List<CarStudent>()
            };
            socio.SocioMod.ScholarshipId = scholarshipid;
            socio.SocioMod.StudentId = StudentId;
            using(DikanDbContext ctx = new DikanDbContext())
            {
                foreach (var car in ctx.CarStudents.Where(s=>s.StudentId == StudentId).ToList()) // get all cars of student from db to list
                {
                    car.StudentId = StudentId;
                    socio.ListCarStudent.Add(car);
                }
                //ctx.Configuration.LazyLoadingEnabled = false;
            }            
            return View(socio);
        }

        [HttpPost]
        public ActionResult Socio(SocioAdd socioeconomic) // submit  new socio scholarship
         {
            socioeconomic.SocioMod.StudentId = StudentId;
            if (ModelState.IsValid)
            {
            }
            ViewBag.YearsList = new SelectList(YearsSelectList(), null, "Text"); // to show volunteer places list in drop down
            return View(socioeconomic);
          
        }

        public PartialViewResult CarsView()
        {
            ViewBag.YearsList = new SelectList(YearsSelectList(), null, "Text"); // to show volunteer places list in drop down
            return PartialView("CarsView", new CarStudent());
        }

        public ActionResult DeleteCar(string CarNum)
        {
            CarStudent tempcar;
            
            using(DikanDbContext ctx = new DikanDbContext())
            {
                tempcar = ctx.CarStudents.Find(int.Parse(CarNum));
                if (tempcar == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                ctx.CarStudents.Remove(tempcar);
                ctx.SaveChanges();
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
        #endregion

        #region Upload File Ajax

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadFile(HttpPostedFileBase filee,string fileName, string oldFile)
        {
            if (filee == null || string.IsNullOrEmpty(fileName))
            {
                Response.StatusCode = 400;
                return Content("Not send file or name", "text/plain");
            }
            var file = Request.Files[0];
            string path;
            path = SaveFile.SaveFileInServer(filee, fileName, StudentId, oldFile);
            Response.StatusCode = 200;
            return Content(path, "text/plain");
        }

        #endregion


        [NonAction]
        public List<VolunteerPlaces> SetsvolunteerPlaces() // add to name and desc to name_desc field
        {
            List<VolunteerPlaces> places = new List<VolunteerPlaces>();
            using (DikanDbContext ctx = new DikanDbContext())
            {
                places = ctx.VolunteerPlaces.ToList();
                foreach (var place in places.ToList()) // sets name+desc field to show in drop down list
                {
                    place.Name_desc = place.Name + " - " + place.Desc;
                }
            }
            return places;
        }

        [NonAction]
        public List<SelectListItem> YearsSelectList()
        {
            List<SelectListItem> years = new List<SelectListItem>();
            for(int year= DateTime.Now.Year; year>=1980;year--)
            {
                years.Add(new SelectListItem { Text = year.ToString(), Value = year.ToString() });
            }
            return years;
        }
    
    }
}