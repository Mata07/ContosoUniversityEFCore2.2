using System.ComponentModel.DataAnnotations;
using System.Security.Permissions;
using System.Threading;

namespace ContosoUniversity.Models
{
    public enum Grade
    {
        A, B, C, D, F
    }
    public class Enrollment
    {
        public int EnrollmentID { get; set; }
        public int CourseID { get; set; } //foreign key 
        public int StudentID { get; set; } //foreign key

        //A grade that's null is different from a zero grade 
        //null means a grade isn't known or hasn't been assigned yet.
        [DisplayFormat(NullDisplayText = "No grade")]
        public Grade? Grade { get; set; }

        //Navigation properties 
        //can have only 1 Student entity and only 1 Course entity
        //1 to many relationship (this is 1) 
        public Course Course { get; set; } //single course entity
        public Student Student { get; set; }

    }
}