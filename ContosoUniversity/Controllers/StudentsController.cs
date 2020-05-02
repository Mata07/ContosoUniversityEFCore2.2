using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;

namespace ContosoUniversity.Controllers
{
    public class StudentsController : Controller
    {
        private readonly SchoolContext _context;

        public StudentsController(SchoolContext context)
        {
            _context = context;
        }

        //The async keyword tells the compiler to generate callbacks for parts of the method body
        //and to automatically create the Task<IActionResult> object that's returned.
        //The return type Task<IActionResult> represents ongoing work with a result of type IActionResult.

        //Only statements that cause queries or commands to be sent to the database are executed asynchronously.
        //That includes, for example, ToListAsync, SingleOrDefaultAsync, and SaveChangesAsync. 
        //It doesn't include, for example, statements that just change an IQueryable,
        //such as var students = context.Students.Where(s => s.LastName == "Davolio").

        // GET: Students
        public async Task<IActionResult> Index()
        {
            //The await keyword causes the compiler to split the method into two parts.
            //The first part ends with the operation that's started asynchronously.
            //The second part is put into a callback method that's called when the operation completes.

            //ToListAsync is the asynchronous version of the ToList extension method.
            return View(await _context.Students.ToListAsync());
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //The Include and ThenInclude methods cause the context to load
            //the Student.Enrollments navigation property, 
            //and within each enrollment the Enrollment.Course navigation property.
            var student = await _context.Students
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        // OVERPOSTING Methods
        //1. BIND - The Bind attribute that the scaffolded code includes on the Create method is one way to protect against overposting in create scenarios.
        //2. TRYUPDATEMODEL - You can prevent overposting in edit scenarios by reading the entity from the database first and then calling TryUpdateModel, 
        //      passing in an explicit allowed properties list.
        //      - primjer u Edit i u RazorPages tutorialu
        //3. VIEW MODELS
        //An alternative way to prevent overposting that's preferred by many developers
        //is to use view models rather than entity classes with model binding.
        //Include only the properties you want to update in the view model. 
        //Once the MVC model binder has finished, copy the view model properties to the entity instance,
        //optionally using a tool such as AutoMapper.
        //Use _context.Entry on the entity instance to set its state to Unchanged,
        //and then set Property("PropertyName").IsModified to true on each entity property that's included in the view model.
        //This method works in both edit and create scenarios.

        // 1. BIND method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EnrollmentDate,FirstMidName,LastName")] Student student)
        {
            //Bind bez ID jer se dodaje automatski, a ne od usera i dodan try catch blok
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(student); //This code adds the Student entity created by the ASP.NET Core MVC model binder
                    await _context.SaveChangesAsync(); // save changes
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists " +
                    "see your system administrator.");
            }
            return View(student);
        }

        // 2. TryUpdateModelAsync Method
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(Student student)
        //{
        //    if (await TryUpdateModelAsync<Student>(
        //        student,    // Uses the posted form values
        //        "student", // Prefix for form value.
        //        s => s.FirstMidName, s => s.LastName, s => s.EnrollmentDate)) //Updates only the properties listed
        //    {
        //        try //nisam siguran da li treba? dodano iz Edit metode
        //        {
        //            _context.Add(student); //This code adds the Student entity created by the ASP.NET Core MVC model binder
        //            await _context.SaveChangesAsync(); // save changes
        //            return RedirectToAction(nameof(Index));
        //        }
        //        catch (DbUpdateException /* ex */)
        //        {
        //            //Log the error (uncomment ex variable name and write a log.)
        //            ModelState.AddModelError("", "Unable to save changes. " +
        //                "Try again, and if the problem persists " +
        //                "see your system administrator.");
        //        }
        //    }
        //    return View(student);
        //}

        //3. View Model metoda - Mijenjamo i @model u View-u
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(StudentViewModel studentViewModel) //proslijeđujemo ViewModel
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var entry = _context.Add(new Student());
        //            //The SetValues method sets the values of this object by reading values from another PropertyValues object. 
        //            //SetValues uses property name matching. The view model type doesn't need to be related to the model type,
        //            //it just needs to have properties that match.
        //            entry.CurrentValues.SetValues(studentViewModel);
        //            await _context.SaveChangesAsync(); // save changes
        //            return RedirectToAction(nameof(Index));
        //        }
        //        catch (DbUpdateException /*ex*/)
        //        {
        //            //Log the error (uncomment ex variable name and write a log.)
        //            ModelState.AddModelError("", "Unable to save changes. " +
        //                "Try again, and if the problem persists " +
        //                "see your system administrator.");
        //        }
        //    }
        //    return View(studentViewModel);
        //}

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var studentToUpdate = await _context.Students.FirstOrDefaultAsync(s => s.ID == id);

            if (await TryUpdateModelAsync<Student>(
                studentToUpdate,
                "",
                s => s.FirstMidName, s => s.LastName, s => s.EnrollmentDate))
            {
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException /*ex*/)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
                return RedirectToAction(nameof(Index));
            }
            return View(studentToUpdate);
        }


        // 2. BIND - You can use this approach when the web page UI includes all of the fields in the entity and can update any of them.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("ID,LastName,FirstMidName,EnrollmentDate")] Student student)
        //{
        //    if (id != student.ID)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(student);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!StudentExists(student.ID))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(student);
        //}

        // GET: Students/Delete/5
        //This code accepts an optional parameter that indicates whether the method was called after a failure to save changes.
        //This parameter is false when the HttpGet Delete method is called without a previous failure. 
        //When it's called by the HttpPost Delete method in response to a database update error, the parameter is true and an error message is passed to the view.
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .AsNoTracking() //dodano
                .FirstOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return NotFound();
            }

            if (saveChangesError.GetValueOrDefault()) // dodana error poruka
            {
                ViewData["ErrorMessage"] =
                    "Delete failed. Try again, and if the problem persists " +
                    "see your system administrator.";
            }
            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)// provjera
            {
                return RedirectToAction(nameof(Index));
            }

            try // dodan try catch
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.ID == id);
        }
    }
}
