using System.ComponentModel.DataAnnotations;

namespace BangazonWorkForceManagement.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        [StringLength(55, MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(55, MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Employee Name")]
        public string FullName
        {
            get { return $"{FirstName} {LastName}"; }
        }

        [Required]
        [Display(Name = "Supervisor?")]
        public bool IsSuperVisor { get; set; }

        [Required]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        [Display(Name = "Computer")]
        public int? CurrentComputerId { get; set; }
        public Computer Computer { get; set; }
    }
}
