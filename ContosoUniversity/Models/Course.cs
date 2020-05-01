﻿using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
    public class Course
    {
        //this attribute lets you enter the primary key for the course rather than having the database generate it.
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CourseID { get; set; }
        public string Title { get; set; }
        public int Credits { get; set; }

        //A Course entity can be related to any number of Enrollment entities.
        public ICollection<Enrollment> Enrollments { get; set; }

    }
}