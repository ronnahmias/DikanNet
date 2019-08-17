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
                if (uploadmethod.Equals("submit"))
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
                    ListFamMemFin = new List<FamilyMember>(),
                    ListFamMem = new List<FamilyMember>(),
                    MatrialStatus = ctx.Students.Where(s => s.StudentId == sStudentId).FirstOrDefault().MaritalStatus
                };

                #region Socio Model
                // socio model get
                socio.SocioMod = ctx.Socio.Where(s => s.StudentId == sStudentId && s.ScholarshipId == scholarshipid).SingleOrDefault(); // get socio model of student from db
                if (socio.SocioMod == null) socio.SocioMod = new SpSocio();
                socio.SocioMod.ScholarshipId = scholarshipid; // insert scholarship id in socio model
                #endregion

                #region Car Student + Fundings
                foreach (var car in ctx.CarStudents.Where(s=>s.StudentId == sStudentId).ToList()) // get all cars of student from db to list
                    socio.ListCarStudent.Add(car);
                foreach (var fund in ctx.Fundings.Where(s => s.StudentId == sStudentId).ToList()) // get all fundings of student from db to list
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

                if (numofFamMem > 0) // if no needed row for family member with finance skip on it
                {
                    //2. include on each family member that is dad/mom/wife/husband their finance
                    foreach (var FamMem in ctx.FamilyMembers.Include(s=>s.FamilyStudentFinances)
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
                            List<FamilyStudentFinance> familyStudentFinances = new List<FamilyStudentFinance>(); // init new list of finance to each family member
                            socio.ListFamMemFin.Add(new FamilyMember { FamilyStudentFinances = familyStudentFinances }); // add family member row to list
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
                                member.FamilyStudentFinances.Add(new FamilyStudentFinance { FinNo = socio.ListStudentFinances.Count() }); // add finance row to list
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
                        s.Realationship == Enums.Realationship.בת.ToString()).ToList()) // filter only dad mom and wife/husband
                    socio.ListFamMem.Add(Fam);
                #endregion

            }
            return View(socio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Socio(SocioAdd socio, string uploadmethod) // submit  new socio scholarship
         {
            #region Help Variables
            SpSocio spsocio;
            CarStudent tempDbCar;
            Funding tempDbFund;
            StudentFinance tempDbStuFin;
            FamilyMember tempDbFamMem;
            List<CarStudent> dbCars;
            List<Funding> dbFunding;
            List<StudentFinance> dbStuFinance;
            List<FamilyMember> dbFamMem; // hold db family member only with finance
            string[,] pathStudExSaFinance = new string[3,3]; // holding path expense and salary;
            #endregion

            #region init null lists of soicoAdd model
            if (socio.ListCarStudent == null) socio.ListCarStudent = new List<CarStudent>(); // if there is no rows in car student list
            if (socio.ListFundings == null) socio.ListFundings = new List<Funding>(); // if there is no rows in fundings list
            if (socio.ListFamMemFin == null) socio.ListFamMemFin = new List<FamilyMember>(); // if there is no rows in family member finance list
            if (socio.ListStudentFinances == null) socio.ListStudentFinances = new List<StudentFinance>(); // if there is no rows in finance list
            #endregion

            if (ModelState.IsValid)
            {
                //save detailes
                using (DikanDbContext ctx = new DikanDbContext())
                {
                    #region Save Cars Detailes
                    // list of cars by database
                    dbCars = ctx.CarStudents.Where(s => s.StudentId == sStudentId).ToList();
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
                    // list of funding by database
                    dbFunding = ctx.Fundings.Where(s => s.StudentId == sStudentId).ToList();
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
                    /* בתחילה אני מסיר את כל הנתונים מהשרת תוך כדי שמירה על קבצים
                     * לאחר מכן רץ על הרשימה שהתקבלה מהלקוח
                     * משתיל את פרטי הסטודנט וקוד מלגה
                     * סוחב את הנתונים מהשרת לתוך רשימה חדשה
                     * ושולף את השורה שבה השנה והוחודש שווים לאלמנט שנמצא עכשיו
                     * אם קיים יש כפל ולכן אי אפשר לשמור את המידע הזה ומדלגים עליו
                     * הבעיה שנוצרה היא הקובץ קיים בשרת ולא נמחק אך מאבדים את מיקומו
                     * פתרון אפשרי: בעת המציאה למחוק אותו באמצעות פונקציה קיימת
                     */

                    // list of student finance by database
                    dbStuFinance = ctx.StudentFinances.Where(s => s.StudentId == sStudentId && s.SpId == socio.SocioMod.ScholarshipId).ToList();

                    foreach (var finDb in dbStuFinance)
                    {
                        pathStudExSaFinance[0, finDb.FinNo] = finDb.PathExpense;
                        pathStudExSaFinance[0, finDb.FinNo] = finDb.PathSalary;
                        ctx.StudentFinances.Remove(finDb);
                    }
                    ctx.SaveChanges();
                    foreach (var fin in socio.ListStudentFinances)
                    {
                        fin.StudentId = sStudentId;
                        fin.SpId = socio.SocioMod.ScholarshipId;

                        // list of finance by database
                        dbStuFinance = ctx.StudentFinances.Where(s => s.StudentId == sStudentId && s.SpId == socio.SocioMod.ScholarshipId).ToList();
                        tempDbStuFin = dbStuFinance.Where(s => s.Month == fin.Month && s.Year == fin.Year).FirstOrDefault();
                        if (tempDbStuFin != null) continue;

                        fin.PathExpense = pathStudExSaFinance[0, fin.FinNo];
                        fin.PathSalary  = pathStudExSaFinance[0, fin.FinNo];

                        // if there is a expense file upload and update the file path
                        if (fin.FileExpense != null)
                            fin.PathExpense = Files.SaveFileInServer(fin.FileExpense, "Expense" +fin.FinNo, sStudentId, fin.PathExpense);

                        // if there is a salary file upload and update the file path
                        if (fin.FileSalary != null)
                            fin.PathSalary = Files.SaveFileInServer(fin.FileSalary, "Salary" + fin.FinNo, sStudentId, fin.PathSalary);

                        ctx.StudentFinances.Add(fin);
                        ctx.SaveChanges();
                    }

                    #endregion

                    #region Save Family Member + Finance

                    dbFamMem = ctx.FamilyMembers.Where(s => s.StudentId == sStudentId).Where(s => s.Realationship == Enums.Realationship.אב.ToString() ||
                        s.Realationship == Enums.Realationship.אם.ToString() ||
                        s.Realationship == Enums.Realationship.בעל.ToString() ||
                        s.Realationship == Enums.Realationship.אישה.ToString()).ToList(); // filter only dad mom and wife/husband

                    foreach(var FamMem in socio.ListFamMemFin)
                    {
                        FamMem.StudentId = sStudentId;
                        tempDbFamMem = dbFamMem.Where(s => s.FamilyMemberId == FamMem.FamilyMemberId).FirstOrDefault(); // find if this family member has exist in db

                        // save or update finance of the family member
                        IList<FamilyStudentFinance> tempfinlist = FamMem.FamilyStudentFinances;

                        foreach(var fin in tempfinlist)
                        {

                        }


                        // save or update family member
                        if(FamMem.FileFamId != null) // there is new file to save on server
                            FamMem.PathFmId = Files.SaveFileInServer(FamMem.FileFamId, "Family" + FamMem.FamilyMemberId, FamMem.StudentId, FamMem.PathFmId); // save id family member

                        if (tempDbFamMem == null) // new family member
                        {
                            ctx.FamilyMembers.Add(FamMem);
                        }
                        else
                        {
                            ctx.Entry(tempDbFamMem).CurrentValues.SetValues(FamMem);// update family member exists
                        }
                        ctx.SaveChanges();
                    }

                    #endregion

                    #region Save Family Members
                    #endregion

                    #region Save Socio model + Submit Sp
                    socio.SocioMod.StudentId = sStudentId; // bind student id to socio model

                    if (socio.SocioMod.FileApartmentLease != null) // save file if not null
                        socio.SocioMod.PathApartmentLease = Files.SaveFileInServer(socio.SocioMod.FileApartmentLease, "ApartmentLease", sStudentId, socio.SocioMod.PathApartmentLease);

                    if (socio.SocioMod.FileBereavedFam != null) // save file if not null
                        socio.SocioMod.PathBereavedFam = Files.SaveFileInServer(socio.SocioMod.FileBereavedFam, "BereavedFam", sStudentId, socio.SocioMod.PathBereavedFam);

                    if (socio.SocioMod.FileDisabilityType != null) // save file if not null
                        socio.SocioMod.PathDisabilityType = Files.SaveFileInServer(socio.SocioMod.FileDisabilityType, "DisabilityType", sStudentId, socio.SocioMod.PathDisabilityType);

                    if (socio.SocioMod.FileMilitaryService != null) // save file if not null
                        socio.SocioMod.PathMilitaryService = Files.SaveFileInServer(socio.SocioMod.FileMilitaryService, "MilitaryService", sStudentId, socio.SocioMod.PathMilitaryService);

                    if (socio.SocioMod.FileNewcomer != null) // save file if not null
                        socio.SocioMod.PathNewcomer = Files.SaveFileInServer(socio.SocioMod.FileNewcomer, "Newcomer", sStudentId, socio.SocioMod.PathNewcomer);

                    if (socio.SocioMod.FileReserveMilitaryService != null) // save file if not null
                        socio.SocioMod.PathReserveMilitaryService = Files.SaveFileInServer(socio.SocioMod.FileReserveMilitaryService, "ReserveMilitaryService", sStudentId, socio.SocioMod.PathReserveMilitaryService);

                    if (socio.SocioMod.FileSingleParent != null) // save file if not null
                        socio.SocioMod.PathSingleParent = Files.SaveFileInServer(socio.SocioMod.FileSingleParent, "SingleParent", sStudentId, socio.SocioMod.PathSingleParent);

                    spsocio = ctx.Socio.Where(s => s.StudentId == socio.SocioMod.StudentId && s.ScholarshipId == socio.SocioMod.ScholarshipId).SingleOrDefault(); // find if he insert already draft     
                    if (uploadmethod.Equals("הגש מלגה"))
                    {
                        socio.SocioMod.Statuss = Enums.Status.בטיפול.ToString(); // insert status betipul
                        socio.SocioMod.StatusUpdateDate = socio.SocioMod.DateSubmitScholarship = DateTime.Now; // insert date submit + update status 
                    }
                    if(spsocio == null)
                    {
                        ctx.Socio.Add(socio.SocioMod); // add new sp socio
                    }
                    else
                        ctx.Entry(spsocio).CurrentValues.SetValues(socio.SocioMod); // update

                    ctx.Configuration.ValidateOnSaveEnabled = false;
                    ctx.SaveChanges();
                    #endregion
                }
            }
            ViewBag.YearsList = new SelectList(YearsSelectList(), null, "Text"); // to show years list in drop down
            ViewBag.MonthsList = new SelectList(MonthsSelectList(), null, "Text"); // to show months list in drop down
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

        public PartialViewResult FamMemView() // family member with finance
        {
            ViewBag.YearsList = new SelectList(YearsSelectList(), null, "Text"); // to show years list in drop down
            return PartialView("FamMemView", new FamilyMember());
        }

        public PartialViewResult FamilyView() // family member
        {
            return PartialView("FamilyView", new FamilyMember());
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

        [HttpPost]
        public ActionResult SaveSignature(string pDataUri)
        {
            var ok = Files.signatureSave(pDataUri, sStudentId);
            return new HttpStatusCodeResult(ok ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            //Files.SaveFileInServer(, "signature", sStudentId, null);
        }


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

        [NonAction]
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