using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Models.SchoolViewModels;
using System.Runtime.CompilerServices;

namespace ContosoUniversity.Controllers
{
    public class InstructorsController : Controller
    {
        private readonly SchoolContext _context;

        public InstructorsController(SchoolContext context)
        {
            _context = context;
        }


        // GET: Instructors
        public async Task<IActionResult> Index(int? id, int? courseID)
        {
            // instance of a viewModel
            var viewModel = new InstructorIndexData();

            viewModel.Instructors = await _context.Instructors
                .Include(i => i.OfficeAssignment)           // load navigation properties
                .Include(i => i.CourseAssignments)          // load navigation properties
                    .ThenInclude(i => i.Course)             // load navigation properties to get related Courses, Erollments and Students
                        .ThenInclude(i => i.Enrollments)
                            .ThenInclude(i => i.Student)
                .Include(i => i.CourseAssignments)          // load navigation properties to get related Departments to Courses
                    .ThenInclude(i => i.Course)
                        .ThenInclude(i => i.Department)
                .AsNoTracking()
                .OrderBy(i => i.LastName)
                .ToListAsync();

            // The following code executes when an instructor was selected.
            if (id != null)
            {
                ViewData["InstructorID"] = id.Value;

                // The selected instructor is retrieved from the list of instructors in the view model.
                // The Where method returns a collection, but in this case the criteria passed to that method result in only a single Instructor entity being returned.
                // The Single method converts the collection into a single Instructor entity, which gives you access to that entity's CourseAssignments property.                
                Instructor instructor = viewModel.Instructors.Where(i => i.ID == id.Value).Single();

                // The view model's Courses property is then loaded with the Course entities from that instructor's CourseAssignments navigation property.
                // The CourseAssignments property contains CourseAssignment entities, from which you want only the related Course entities.
                viewModel.Courses = instructor.CourseAssignments.Select(s => s.Course);
            }

            // if a course was selected, the selected course is retrieved from the list of courses in the view model
            if (courseID != null)
            {
                ViewData["CourseID"] = courseID.Value;

                // Then the view model's Enrollments property is loaded with the Enrollment entities from that course's Enrollments navigation property.
                viewModel.Enrollments = viewModel.Courses.Where(x => x.CourseID == courseID).Single().Enrollments;
            }

            return View(viewModel);
        }

        // GET: Instructors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }




        // GET: Instructors/Create
        public IActionResult Create()
        {
            // provide empty collection for the foreach loop in the View(to avoid null reference exception)
            var instructor = new Instructor();
            instructor.CourseAssignments = new List<CourseAssignment>();
            PopulateAssignedCourseData(instructor);
            return View();
        }

        // POST: Instructors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstMidName,HireDate,LastName,OfficeAssignment")] Instructor instructor, string[] selectedCourses)
        {
            // The HttpPost Create method adds each selected course to the CourseAssignments navigation property
            // before it checks for validation errors and adds the new instructor to the database.
            // Courses are added even if there are model errors so that when there are model errors
            // (for an example, the user keyed an invalid date), and the page is redisplayed with an error message,
            // any course selections that were made are automatically restored.

            if (selectedCourses != null)
            {
                //  to be able to add courses to the CourseAssignments navigation property you have to initialize the property as an empty collection:
                // Alternative is to do it in the Instructor model by changing the property getter to automatically create the collection if it doesn't exist
                //private ICollection<CourseAssignment> _courseAssignments;
                //public ICollection<CourseAssignment> CourseAssignments
                //{ get {  return _courseAssignments ?? (_courseAssignments = new List<CourseAssignment>()); }
                //  set {  _courseAssignments = value;} }
                instructor.CourseAssignments = new List<CourseAssignment>();
                foreach (var course in selectedCourses)
                {
                    var courseToAdd = new CourseAssignment { InstructorID = instructor.ID, CourseID = int.Parse(course) };
                    instructor.CourseAssignments.Add(courseToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                _context.Add(instructor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateAssignedCourseData(instructor);
            return View(instructor);

        }


        // GET: Instructors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors
                             .Include(i => i.OfficeAssignment) // load OfficeAssignment
                             .Include(i => i.CourseAssignments) // eager load CourseAssignments
                                .ThenInclude(i => i.Course)
                             .AsNoTracking()
                             .FirstOrDefaultAsync(m => m.ID == id);

            if (instructor == null)
            {
                return NotFound();
            }
            // call the new PopulateAssignedCourseData method to provide information
            // for the check box array using the AssignedCourseData view model class.
            PopulateAssignedCourseData(instructor);
            return View(instructor);
        }




        // The code in the PopulateAssignedCourseData method reads through all Course entities
        // in order to load a list of courses using the view model class.
        private void PopulateAssignedCourseData(Instructor instructor)
        {
            // load all courses
            var allCourses = _context.Courses;

            // For each course, the code CHECKS whether the COURSE EXIST in the instructor's Courses navigation property. 
            // To create efficient lookup when checking whether a course is assigned to the instructor, the courses assigned to the instructor are put into a HashSet collection
            // HashSet<int> - collection with no duplicate items and not sorted
            var instructorCourses = new HashSet<int>(instructor.CourseAssignments.Select(c => c.CourseID));

            // Create list of viewModels
            var viewModel = new List<AssignedCourseData>();
            foreach (var course in allCourses)
            {
                viewModel.Add(new AssignedCourseData
                {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    // The Assigned (bool) property is set to true for courses the instructor is assigned to.
                    // The view will use this property to determine which check boxes must be displayed as selected. 
                    Assigned = instructorCourses.Contains(course.CourseID)
                });
            }
            // pass the list to View
            ViewData["Courses"] = viewModel;
        }




        // POST: Instructors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, string[] selectedCourses)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Gets the current Instructor entity from the database
            // using eager loading for the OfficeAssignment navigation property
            var instructorToUpdate = await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments).ThenInclude(i => i.Course)
                .FirstOrDefaultAsync(m => m.ID == id);

            // Updates the retrieved Instructor entity with values from the model binder.
            if (await TryUpdateModelAsync<Instructor>(
                instructorToUpdate,
                "",
                i => i.FirstMidName, i => i.LastName, i => i.HireDate, i => i.OfficeAssignment))
            {
                // If the office location is blank, sets the Instructor.OfficeAssignment property to null
                // so that the related row in the OfficeAssignment table will be deleted.
                if (String.IsNullOrWhiteSpace(instructorToUpdate.OfficeAssignment?.Location))
                {
                    instructorToUpdate.OfficeAssignment = null;
                }

                // Since the view doesn't have a collection of Course entities, the model binder can't automatically update the CourseAssignments navigation property.
                // Instead of using the model binder to update the CourseAssignments navigation property, you do that in the new UpdateInstructorCourses method
                UpdateInstructorCourses(selectedCourses, instructorToUpdate);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException /*ex*/)
                {
                    // Log the error
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
                return RedirectToAction(nameof(Index));
            }
            UpdateInstructorCourses(selectedCourses, instructorToUpdate);
            PopulateAssignedCourseData(instructorToUpdate);
            return View(instructorToUpdate);
        }





        private void UpdateInstructorCourses(string[] selectedCourses, Instructor instructorToUpdate)
        {

            // If no check boxes were selected, the code in UpdateInstructorCourses initializes the CourseAssignments navigation property with an empty collection and returns:
            if (selectedCourses == null)
            {
                instructorToUpdate.CourseAssignments = new List<CourseAssignment>();
                return;
            };

            var selectedCoursesHS = new HashSet<string>(selectedCourses);
            var instructorCourses = new HashSet<int>(instructorToUpdate.CourseAssignments.Select(c => c.Course.CourseID));

            foreach (var course in _context.Courses)
            {
                // If the check box for a course was selected but the course isn't in the Instructor.CourseAssignments navigation property, 
                // the course is added to the collection in the navigation property.
                if (selectedCoursesHS.Contains(course.CourseID.ToString()))
                {
                    if (!instructorCourses.Contains(course.CourseID))
                    {
                        instructorToUpdate.CourseAssignments.Add(new CourseAssignment
                        {
                            InstructorID = instructorToUpdate.ID,
                            CourseID = course.CourseID
                        });
                    }
                }
                else
                {
                    // If the check box for a course wasn't selected,
                    // but the course is in the Instructor.CourseAssignments navigation property,
                    if (instructorCourses.Contains(course.CourseID))
                    {
                        // the course is removed from the navigation property.
                        CourseAssignment courseToRemove = instructorToUpdate.CourseAssignments.FirstOrDefault(i => i.CourseID == course.CourseID);
                        _context.Remove(courseToRemove);
                    }
                }
            }
        }

        // GET: Instructors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        // 05-UpdateRelatedData - Change
        // POST: Instructors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Does eager loading for the CourseAssignments navigation property.
            // You have to include this or EF won't know about related CourseAssignment entities and won't delete them.
            // To avoid needing to read them here you could configure cascade delete in the database.
            Instructor instructor = await _context.Instructors
                .Include(i => i.CourseAssignments)
                .SingleAsync(i => i.ID == id);

            // If the instructor to be deleted is assigned as administrator of any departments,
            // removes the instructor assignment from those departments.
            var departments = await _context.Departments
                .Where(d => d.InstructorID == null)
                .ToListAsync();
            departments.ForEach(d => d.InstructorID = null);

            _context.Instructors.Remove(instructor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //private bool InstructorExists(int id)
        //{
        //    return _context.Instructors.Any(e => e.ID == id);
        //}


    }
}
