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
                foreach (var scholarship in studentMain.ScholarshipDefinitions.ToList()) // dont show scholarship that already is submited
                {
                    switch (scholarship.Type)
                    {
                        case 1:
                            if (ctx.Socio.Any(s => s.ScholarshipId == scholarship.ScholarshipID && s.StudentId == sStudentId))
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
                studentMain.InPracticeList = ctx.Halacha.Include(s => s.ScholarshipDefinition).Where(s => s.StudentId == sStudentId).ToList(); // send to view inpractice list of student
                studentMain.ExcelList = ctx.Ecellence.Include(s => s.ScholarshipDefinition).Where(s => s.StudentId == sStudentId).ToList(); // send to view excellence list of student
                studentMain.SocioList = ctx.Socio.Include(s => s.ScholarshipDefinition).Where(s => s.StudentId == sStudentId).ToList(); // send to view socio list of student
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

                    if (UpdateStudent.FileId != null)
                        UpdateStudent.PathId = Files.SaveFileInServer(UpdateStudent.FileId, "Id", UpdateStudent.StudentId, (dbStudent == null) ? null : dbStudent.PathId);

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
            using (DikanDbContext ctx = new DikanDbContext())
            {
                SpDefinition temp = ctx.SpDef.Find(scholarshipid);
                type = temp.Type;
            }
            switch (type)
            {
                case 1: return RedirectToAction("Socio", new { scholarshipid }); // type 1 is socio scholarship

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
            }
            else
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
                    ListFamMemFin = new List<FamilyMember>(), // family with finance
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
                foreach (var car in ctx.CarStudents.Where(s => s.StudentId == sStudentId).ToList()) // get all cars of student from db to list
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Socio(SocioAdd socio, string uploadmethod) // submit  new socio scholarship
        {

            if (socio.ListCarStudent != null)
                SaveSocioCars(socio.ListCarStudent); //Save Cars Detailes

            if (socio.ListFundings != null)
                SaveFundings(socio.ListFundings);  // Save Funding

            SaveStudentFinance(socio.ListStudentFinances, socio.SocioMod.ScholarshipId); // Save Student Finance

            SaveFamilyMemberFinance(socio.ListFamMemFin, socio.SocioMod.ScholarshipId);  // Save Family Member + Finance

            if (socio.ListFamMem != null)
                SaveFamilyMember(socio.ListFamMem);// Save Family Members

            socio.SocioMod = SaveSocioModel(socio.SocioMod); // Save Socio model

            if (uploadmethod.Equals("הגש מלגה"))
            {
                using (DikanDbContext ctx = new DikanDbContext())
                {
                    socio.SocioMod.Statuss = Enums.Status.בטיפול.ToString(); // insert status betipul
                    socio.SocioMod.StatusUpdateDate = socio.SocioMod.DateSubmitScholarship = DateTime.Now; // insert date submit + update status 
                    SpSocio Dbsocio = ctx.Socio.Where(s => s.StudentId == socio.SocioMod.StudentId && s.ScholarshipId == socio.SocioMod.ScholarshipId).SingleOrDefault();
                    ctx.Entry(Dbsocio).CurrentValues.SetValues(socio.SocioMod); // update
                    ctx.SaveChanges();
                }
            }

            #region init null lists of soicoAdd model
            if (socio.ListCarStudent == null) socio.ListCarStudent = new List<CarStudent>(); // if there is no rows in car student list
            if (socio.ListFundings == null) socio.ListFundings = new List<Funding>(); // if there is no rows in fundings list
            if (socio.ListFamMemFin == null) socio.ListFamMemFin = new List<FamilyMember>(); // if there is no rows in family member finance list
            if (socio.ListStudentFinances == null) socio.ListStudentFinances = new List<StudentFinance>(); // if there is no rows in finance list

            ViewBag.YearsList = new SelectList(YearsSelectList(), null, "Text"); // to show years list in drop down
            ViewBag.MonthsList = new SelectList(MonthsSelectList(), null, "Text"); // to show months list in drop down
            #endregion

            return View(socio);
        }

        [NonAction]
        public void SaveSocioCars(List<CarStudent> ClientList)
        {
            List<CarStudent> DbList;
            CarStudent TempDbCar;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                // list of cars by database
                DbList = ctx.CarStudents.Where(s => s.StudentId == sStudentId).ToList();
                // update each car that posted from client
                foreach (var car in ClientList)
                {
                    car.StudentId = sStudentId;// insert ths id of student to every car
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
        public void SaveFundings(List<Funding> ClientList)
        {
            List<Funding> DbList;
            Funding TempDbFund;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                // list of funding by database
                DbList = ctx.Fundings.Where(s => s.StudentId == sStudentId).ToList();
                // update each fund that posted from client
                foreach (var fund in ClientList)
                {
                    fund.StudentId = sStudentId;// insert ths id of student to every fund
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
            string[,] PathFinance = new string[3, 3]; // holding path expense and salary;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                // list of student finance by database
                DbList = ctx.StudentFinances.Where(s => s.StudentId == sStudentId && s.SpId == SpId).ToList();

                foreach (var finDb in DbList)
                {
                    if (finDb.FinNo < 0 || finDb.FinNo > 2) continue;
                    PathFinance[0, finDb.FinNo] = finDb.PathExpense;
                    PathFinance[0, finDb.FinNo] = finDb.PathSalary;
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
                    {
                        fin.PathExpense = PathFinance[0, fin.FinNo];
                        fin.PathSalary = PathFinance[0, fin.FinNo];
                    }

                    // if there is a expense file upload and update the file path
                    if (fin.FileExpense != null)
                        fin.PathExpense = Files.SaveFileInServer(fin.FileExpense, "Expense" + fin.FinNo, sStudentId, fin.PathExpense);

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
            string[,] PathFinance = new string[3, 3]; // holding path expense and salary;
            using (DikanDbContext ctx = new DikanDbContext())
            {
                dbFamMem = ctx.FamilyMembers.Where(s => s.StudentId == sStudentId).Where(s => s.Realationship == Enums.Realationship.אב.ToString() ||
                             s.Realationship == Enums.Realationship.אם.ToString() ||
                             s.Realationship == Enums.Realationship.בעל.ToString() ||
                             s.Realationship == Enums.Realationship.אישה.ToString()).ToList(); // filter only dad mom and wife/husband

                foreach (var mem in ClientList)
                {
                    /*Start finance*/
                    dbFamFinance = ctx.FamilyStudentFinances.Where(s => s.FamilyMemberId == mem.FamilyMemberId && s.SpId == SpId).ToList();
                    foreach (var finDb in dbFamFinance)
                    {
                        if (finDb.FinNo < 0 || finDb.FinNo > 2) continue;
                        PathFinance[0, finDb.FinNo] = finDb.PathExpense;
                        PathFinance[0, finDb.FinNo] = finDb.PathSalary;
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
                            fin.PathExpense = PathFinance[0, fin.FinNo];
                            fin.PathSalary = PathFinance[0, fin.FinNo];
                        }

                        // if there is a expense file upload and update the file path
                        if (fin.FileExpense != null)
                            fin.PathExpense = Files.SaveFileInServer(fin.FileExpense, "Expense-" + mem.FamilyMemberId + "-" + fin.FinNo, sStudentId, fin.PathExpense);

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

            using (DikanDbContext ctx = new DikanDbContext())
            {
                Dbsocio = ctx.Socio.Where(s => s.StudentId == socio.StudentId && s.ScholarshipId == socio.ScholarshipId).SingleOrDefault(); // find if he insert already draft     
                if (Dbsocio == null)
                {
                    ctx.Socio.Add(socio); // add new sp socio
                }
                else
                    ctx.Entry(Dbsocio).CurrentValues.SetValues(socio); // update

                ctx.SaveChanges();
            }

            return socio;
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

        #region Save Signature
        [HttpPost]
        public ActionResult SaveSignature(string pDataUri, string pName)
        {
            var ok = Files.signatureSave(pDataUri, pName, sStudentId);
            return new HttpStatusCodeResult(ok ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            //Files.SaveFileInServer(, "signature", sStudentId, null);
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
            for (int year = DateTime.Now.Year; year >= 1980; year--)
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