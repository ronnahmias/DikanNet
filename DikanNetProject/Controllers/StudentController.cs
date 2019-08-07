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
        string sStudentId;

        #region OnExecuting Function
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.HttpContext.Session["Student"] == null || Session == null) // if there is no session -> no access to student interface
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Login", controller = "Login" }));
            }
            else
                sStudentId = ((Users)Session["Student"]).UserId;
        }
        #endregion

        #region Index
        public ActionResult Index()
        {
            StudentMain studentMain = new StudentMain();
            using (DikanDbContext ctx = new DikanDbContext())
            {
                 studentMain.ScholarshipDefinitions = ctx.SpDef.Where(x => DbFunctions.TruncateTime(x.DateDeadLine) > DbFunctions.TruncateTime(DateTime.Now) && DbFunctions.TruncateTime(x.DateOpenScholarship) < DbFunctions.TruncateTime(DateTime.Now)).ToList();
                 foreach(var scholarship in studentMain.ScholarshipDefinitions.ToList()) // dont show scholarship that already is submited
                {
                    switch(scholarship.Type)
                    {
                        case 1: if(ctx.Socio.Any(s=>s.ScholarshipId == scholarship.ScholarshipID && s.StudentId == sStudentId))
                                    studentMain.ScholarshipDefinitions.Remove(scholarship);
                            break;

                        case 2:
                            if (ctx.Ecellence.Any(s => s.ScholarshipId == scholarship.ScholarshipID && s.StudentId == sStudentId))
                                studentMain.ScholarshipDefinitions.Remove(scholarship);
                            break;

                        case 3:
                            if (ctx.Halacha.Any(s => s.ScholarshipId == scholarship.ScholarshipID && s.StudentId == sStudentId))
                                studentMain.ScholarshipDefinitions.Remove(scholarship);
                            break;
                    }

                }
                 // send only scholarship that belongs to student id
                 studentMain.InPracticeList = ctx.Halacha.Include(s=>s.ScholarshipDefinition).Where(s => s.StudentId == sStudentId).ToList(); // send to view inpractice list of student
                 studentMain.ExcelList = ctx.Ecellence.Include(s=>s.ScholarshipDefinition).Where(s => s.StudentId == sStudentId).ToList(); // send to view excellence list of student
                 studentMain.SocioList = ctx.Socio.Include(s=>s.ScholarshipDefinition).Where(s => s.StudentId == sStudentId).ToList(); // send to view socio list of student
            }
            
            return View(studentMain);
        }
        #endregion

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
                    
                    student = ctx.Students.Where(z => z.StudentId == sStudentId).FirstOrDefault();
                    if (student == null) // didnt found in student table -> first login -> update basic info
                    {
                        student = new Student
                        {
                            StudentId = sStudentId,
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
            Users tempuser = null;
            UpdateStudent.StudentId = sStudentId;
            if (ModelState.IsValid)
            {
                using (DikanDbContext ctx = new DikanDbContext())
                {
                    Student dbStudent = ctx.Students.Find(UpdateStudent.StudentId);
                    if (dbStudent != null) 
                    {
                        tempuser = ctx.Users.Find(UpdateStudent.StudentId); // get the student user
                        if (tempuser != null) // if the student changed the name update in users list also
                        {
                            tempuser.FirstName = UpdateStudent.FirstName;
                            tempuser.LastName = UpdateStudent.LastName;
                            Session["Student"] = tempuser; // update the session with the name
                        }
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

                    if(UpdateStudent.FileId != null)
                        UpdateStudent.PathId = Files.SaveFileInServer(UpdateStudent.FileId, "Id", UpdateStudent.StudentId,(dbStudent == null) ? null:dbStudent.PathId);

                    if (dbStudent == null) // after first login fill more info
                        ctx.Students.Add(UpdateStudent); // add student to data base
                    else
                        ctx.Entry(dbStudent).CurrentValues.SetValues(UpdateStudent);// update student
                    ctx.SaveChanges();

                    if (tempuser != null && tempuser.IsEmailVerified == false) // if the student change the email disconnect from system
                        return RedirectToAction("Disconnect", "Login");

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

        #region Redirect To Action
        public ActionResult RedirectToScholarship(int scholarshipid)
        {
            int type = -1;
            using(DikanDbContext ctx = new DikanDbContext())
            {
                SpDefinition temp = ctx.SpDef.Find(scholarshipid);
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
        #endregion

        #region Halacha Lemaase Scholarship

        [HttpGet]
        public ActionResult Halacha(int scholarshipid)
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
            }else
            {
                if (temphalacha.DateSubmitScholarship != null) // if already has entered this milga
                    return RedirectToAction("Index");
            }
            return View(temphalacha);
        }
        

        [HttpPost]
        public ActionResult Halacha(SpHalacha temphalacha, string uploadmethod) // submit  new halacha scholarship
        {
            temphalacha.StudentId = sStudentId;
            SpHalacha Studentinpractice;
            ViewBag.VolunteerPlacesList = new SelectList(SetsvolunteerPlaces(), "Id", "Name_desc"); // to show volunteer places list in drop down
            using (DikanDbContext ctx = new DikanDbContext())
            {
                Studentinpractice = ctx.Halacha.Where(s => s.StudentId == temphalacha.StudentId && s.ScholarshipId == temphalacha.ScholarshipId).SingleOrDefault(); // find if he insert already draft     
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
                    ctx.Halacha.Add(temphalacha); // if it first time insert new
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
            SpExcellence tempmetsuyanut;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                tempmetsuyanut = ctx.Ecellence.Where(s => s.StudentId == sStudentId && s.ScholarshipId == scholarshipid).SingleOrDefault();
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
                if (tempmetsuyanut.DateSubmitScholarship != null) // if already has entered this milga
                    return RedirectToAction("Index");
            }
            return View(tempmetsuyanut);
        }


        [HttpPost]
        public ActionResult Excellent(SpExcellence tempmetmesuyanut, string uploadmethod) // submit  new metsuyanut scholarship
        {
            tempmetmesuyanut.StudentId = sStudentId;
            SpExcellence StudentMetsuyanut;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                StudentMetsuyanut = ctx.Ecellence.Where(s => s.StudentId == tempmetmesuyanut.StudentId && s.ScholarshipId == tempmetmesuyanut.ScholarshipId).SingleOrDefault(); // find if he insert already draft     
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
                    ctx.Ecellence.Add(tempmetmesuyanut); // if it first time insert new
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
            ViewBag.YearsList = new SelectList(YearsSelectList(), null, "Text"); // to show years list in drop down
            SocioAdd socio = new SocioAdd // new socio add model
            {
                SocioMod = new SpSocio(),
                ListCarStudent = new List<CarStudent>(),
                ListFundings = new List<Funding>(),
                ListStudentFinances = new List<StudentFinance>()
            };
            using (DikanDbContext ctx = new DikanDbContext())
            {
                foreach (var car in ctx.CarStudents.Where(s=>s.StudentId == sStudentId).ToList()) // get all cars of student from db to list
                    socio.ListCarStudent.Add(car);
                foreach (var fund in ctx.Fundings.Where(s => s.StudentId == sStudentId).ToList()) // get all fundings of student from db to list
                    socio.ListFundings.Add(fund);
                foreach (var StudFin in ctx.StudentFinances.Where(s => s.StudentId == sStudentId && s.SpId == scholarshipid).ToList()) // get all fundings of student from db to list
                    socio.ListStudentFinances.Add(StudFin);
            }
            if(socio.ListStudentFinances.Count() < 3) // if the student dont have 3 finance rows
            {
                do
                {
                    socio.ListStudentFinances.Add(new StudentFinance { FinNo = socio.ListStudentFinances.Count() }); // add finance row to list
                } while (socio.ListStudentFinances.Count < 3);
            }
            socio.ListStudentFinances.OrderByDescending(s => s.FinNo);
            socio.SocioMod.ScholarshipId = scholarshipid; // insert scholarship id in socio model
            return View(socio);
        }

        [HttpPost]
        public ActionResult Socio(SocioAdd socio, string uploadmethod) // submit  new socio scholarship
         {
            socio.SocioMod.StudentId = sStudentId; // bind student id to socio model
            List<CarStudent> dbCars;
            List<Funding> dbFunding;
            List<StudentFinance> dbStuFinance;
            CarStudent tempDbCar;
            Funding tempDbFund;
            StudentFinance tempStuFin;
            StudentFinance tempDbStuFin;
            if (socio.ListCarStudent == null) socio.ListCarStudent = new List<CarStudent>(); // if there is no rows in car student list
            if (socio.ListFundings == null) socio.ListFundings = new List<Funding>(); // if there is no rows in fundings list
            if (ModelState.IsValid)
            {
                
                //save car detailes
                using (DikanDbContext ctx = new DikanDbContext())
                {
                    // list of cars by database
                    dbCars = ctx.CarStudents.Where(s => s.StudentId == sStudentId).ToList();
                    // list of funding by database
                    dbFunding = ctx.Fundings.Where(s => s.StudentId == sStudentId).ToList();
                    // list of finance by database
                    dbStuFinance = ctx.StudentFinances.Where(s => s.StudentId == sStudentId && s.SpId == socio.SocioMod.ScholarshipId).ToList();

                    #region Save Cars Detailes
                    // update each car that posted from client
                    foreach (var car in socio.ListCarStudent)
                    {
                        car.StudentId = sStudentId;// insert ths id of student to every car
                        //find the car in the list by carNumber
                        tempDbCar = dbCars.Where(s => s.CarNumber == car.CarNumber).FirstOrDefault();
                        //if it was change at carNumber reset the file path
                        if (tempDbCar == null)
                            car.PathCarLicense = null;
                        
                        // if there is a file upload and update the file path
                        if (car.FileCarLicense != null)
                            car.PathCarLicense = Files.SaveFileInServer(car.FileCarLicense, "Car" + car.CarNumber, sStudentId, car.PathCarLicense);

                        if (tempDbCar != null)
                            ctx.Entry(tempDbCar).CurrentValues.SetValues(car);// update car exists
                        else
                            ctx.CarStudents.Add(car); // add new car to database    
  
                        ctx.SaveChanges();
                    }

                    // אם נמצא בשרת ולא נמצא עם הרשימה שחזרה מהקליינט אז תמחק את הרשומה
                    foreach (var car in dbCars)
                    {
                        tempDbCar = socio.ListCarStudent.Where(s => s.CarNumber == car.CarNumber).FirstOrDefault();
                        if (tempDbCar == null)
                            DeleteCar(car.CarNumber);
                    }

                    #endregion

                    #region Save Funding
                    // update each fund that posted from client
                    foreach (var fund in socio.ListFundings)
                    {
                        fund.StudentId = sStudentId;// insert ths id of student to every fund
                        //find the fund in the list by funding id and the student id
                        tempDbFund = dbFunding.Where(s => s.FundingId == fund.FundingId && s.StudentId == sStudentId).FirstOrDefault();

                        if (tempDbFund != null)
                            ctx.Entry(tempDbFund).CurrentValues.SetValues(fund);// update fund exists
                        else
                            ctx.Fundings.Add(fund); // add new fund to database    

                        ctx.SaveChanges();
                        
                        // if there is a file upload and update the file path
                        if (fund.FileFunding != null)
                        {
                            fund.PathFunding = Files.SaveFileInServer(fund.FileFunding, "Fund" + fund.FundingId, sStudentId, fund.PathFunding);
                            tempDbFund.PathFunding = fund.PathFunding;
                        }
                        ctx.SaveChanges();
                    }
                    #endregion

                    #region Save Student Finance
                    int i = 0;
                    /*
                    foreach(var fin in dbStuFinance)
                    {
                        tempStuFin = socio.ListStudentFinances.Where(s => s.FinNo == fin.FinNo).FirstOrDefault();
                        if (tempStuFin != null)
                        {
                            tempStuFin.PathExpense = fin.PathExpense;
                            tempStuFin.PathSalary = fin.PathSalary;
                            ctx.StudentFinances.Remove(fin);
                            ctx.SaveChanges();

                        }
                        tempStuFin.StudentId = sStudentId;
                        tempStuFin.SpId = socio.SocioMod.ScholarshipId;
                        // if there is a expense file upload and update the file path
                        if (tempStuFin.FileExpense != null)
                            tempStuFin.PathExpense = Files.SaveFileInServer(fin.FileExpense, "Expense" + i, sStudentId, fin.PathExpense);

                        // if there is a salary file upload and update the file path
                        if (tempStuFin.FileSalary != null)
                            tempStuFin.PathSalary = Files.SaveFileInServer(fin.FileSalary, "Salary" + i, sStudentId, fin.PathSalary);
                        ctx.StudentFinances.Add(tempStuFin);
                        ctx.SaveChanges();
                        i++;
                    }
                    if(i < 3)
                    { 
                        foreach (var fin in socio.ListStudentFinances)
                        {
                            fin.StudentId = sStudentId;
                            fin.SpId = socio.SocioMod.ScholarshipId;

                            tempStuFin = dbStuFinance.Where(s => s.FinNo == i).FirstOrDefault();
                            if (tempStuFin != null) continue;

                            // if there is a expense file upload and update the file path
                            if (fin.FileExpense != null)
                                fin.PathExpense = Files.SaveFileInServer(fin.FileExpense, "Expense" + i, sStudentId, fin.PathExpense);

                            // if there is a salary file upload and update the file path
                            if (fin.FileSalary != null)
                                fin.PathSalary = Files.SaveFileInServer(fin.FileSalary, "Salary" + i, sStudentId, fin.PathSalary);

                            fin.FinNo = i;
                            ctx.StudentFinances.Add(fin);
                            ctx.SaveChanges();
                            i++;
                        }
                    }
                    */

                    foreach (var fin in socio.ListStudentFinances)
                    {
                        fin.StudentId = sStudentId;
                        fin.SpId = socio.SocioMod.ScholarshipId;

                        tempDbStuFin = dbStuFinance.Where(s => s.FinNo == fin.FinNo).FirstOrDefault();
                        if (tempDbStuFin != null)
                        {
                            fin.PathExpense = tempDbStuFin.PathExpense;
                            fin.PathSalary = tempDbStuFin.PathSalary;
                            ctx.StudentFinances.Remove(tempDbStuFin);
                            ctx.SaveChanges();
                        }

                        // if there is a expense file upload and update the file path
                        if (fin.FileExpense != null)
                            fin.PathExpense = Files.SaveFileInServer(fin.FileExpense, "Expense" +fin.FinNo, sStudentId, fin.PathExpense);

                        // if there is a salary file upload and update the file path
                        if (fin.FileSalary != null)
                            fin.PathSalary = Files.SaveFileInServer(fin.FileSalary, "Salary" + fin.FinNo, sStudentId, fin.PathSalary);

                        //fin.FinNo = i;
                        ctx.StudentFinances.Add(fin);
                        ctx.SaveChanges();
                        i++;
                    }
                    
                    #endregion
                }


            }
            ViewBag.YearsList = new SelectList(YearsSelectList(), null, "Text"); // to show years list in drop down
            return View(socio);
        }

        #region Partial Views
        public PartialViewResult CarsView()
        {
            ViewBag.YearsList = new SelectList(YearsSelectList(), null, "Text"); // to show years list in drop down
            return PartialView("CarsView", new CarStudent());
        }

        public PartialViewResult FundView()
        {
            ViewBag.YearsList = new SelectList(YearsSelectList(), null, "Text"); // to show years list in drop down
            return PartialView("FundView", new Funding());
        }

        public PartialViewResult StudFinView()
        {
            ViewBag.YearsList = new SelectList(YearsSelectList(), null, "Text"); // to show years list in drop down
            return PartialView("StudFinView", new StudentFinance());
        }
        #endregion

        #region Delete Rows Functions
        public ActionResult DeleteCar(string CarNum)
        {
            CarStudent tempcar;
            using(DikanDbContext ctx = new DikanDbContext())
            {
                tempcar = ctx.CarStudents.Where(s => s.CarNumber == CarNum && s.StudentId == sStudentId).FirstOrDefault();
                if (tempcar == null)
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound,"Not Found In DataBase");
                //if file exists delete it
                if (!string.IsNullOrEmpty(tempcar.PathCarLicense))
                    Files.Delete(tempcar.PathCarLicense, sStudentId);
                ctx.CarStudents.Remove(tempcar);
                ctx.SaveChanges();
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public ActionResult DeleteFund(string FundId)
        {
            Funding tempfund;
            int pFundId = int.Parse(FundId);
            using (DikanDbContext ctx = new DikanDbContext())
            {
                tempfund = ctx.Fundings.Where(s=>s.FundingId == pFundId && s.StudentId == sStudentId).FirstOrDefault();
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
        #endregion

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
            path = Files.SaveFileInServer(filee, fileName, sStudentId, oldFile);
            Response.StatusCode = 200;
            return Content(path, "text/plain");
        }

        #endregion

        #region NonActions

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

        [HttpPost]
        public ActionResult SaveSignature(string pDataUri)
        {
            var ok = Files.signatureSave(pDataUri, sStudentId);
            return new HttpStatusCodeResult (ok ? HttpStatusCode.OK : HttpStatusCode.BadRequest) ;
            //Files.SaveFileInServer(, "signature", sStudentId, null);
        }


        #endregion

    }
}