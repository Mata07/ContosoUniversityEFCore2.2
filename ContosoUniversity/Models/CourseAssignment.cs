namespace ContosoUniversity.Models
{
    public class CourseAssignment
    {
        // Join Table for Instructor-to-Courses many-to-many relationship

        public int InstructorID { get; set; }
        public int CourseID { get; set; }

        public Instructor Instructor { get; set; }
        public Course Course { get; set; }
    }
}