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

        
        // GET: Students       
        public async Task<IActionResult> Index(string sortOrder, string searchString, string currentFilter, int? pageNumber)
        {
            //The first time the page is displayed, or if the user hasn't clicked a paging or sorting link, all the parameters will be null.
            //If a paging link is clicked, the page variable will contain the page number to display.

            //The ViewData element named CurrentSort provides the view with the current sort order, 
            //because this must be included in the paging links in order to keep the sort order the same while paging.
            ViewData["CurrentSort"] = sortOrder;
           
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            //If the search string is changed during paging, the page has to be reset to 1, because the new filter can result in different data to display.
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            //The ViewData element named CurrentFilter provides the view with the current filter string.
            //This value must be included in the paging links in order to maintain the filter settings during paging,
            //and it must be restored to the text box when the page is redisplayed.
            ViewData["CurrentFilter"] = searchString;  //dodajemo za search
            
            var students = from s in _context.Students
                           select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.LastName.Contains(searchString)
                                        || s.FirstMidName.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    students = students.OrderByDescending(s => s.LastName);
                    break;
                case "Date":
                    students = students.OrderBy(s => s.EnrollmentDate);
                    break;
                case "date_desc":
                    students = students.OrderByDescending(s => s.EnrollmentDate);
                    break;
                default:
                    students = students.OrderBy(s => s.LastName);
                    break;
            }
            int pageSize = 3;

            //At the end of the Index method, the PaginatedList.CreateAsync method converts the student query 
            //to a single page of students in a collection type that supports paging.
            //That single page of students is then passed to the view.
            //pageNumber ?? 1 - hasValue = pageNumber ili null = 1
            return View(await PaginatedList<Student>.CreateAsync(students.AsNoTracking(), pageNumber ?? 1, pageSize));
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

        

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //Koristimo FindAsync jer ne moramo Include neki drugi entity pa je nešto efikasniji
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
        
    }
}
