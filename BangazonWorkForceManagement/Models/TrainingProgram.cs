﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BangazonWorkForceManagement.Models
{

    public class TrainingProgram
    {

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public int MaxAttendees { get; set; }

        public List<Employee> Attendees { get; set; }
    }
}
