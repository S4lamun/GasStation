using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GasStation.DTO
{
    public class EmployeeDTO : PersonDTO
    {
        public string Login { get; set; }

        // Password is typically NOT included in display DTOs for security
       
    }

    // DTO for creating a new Employee
    public class CreateEmployeeDTO
    {
        [Required(ErrorMessage = "Pesel is required.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Pesel must be 11 digits.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Pesel must consist of 11 digits.")]
        public string Pesel { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Surname is required.")]
        [StringLength(50, ErrorMessage = "Surname cannot exceed 50 characters.")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Login is required.")]
        [StringLength(50, ErrorMessage = "Login cannot exceed 50 characters.")]
        public string Login { get; set; }

        // Password is required for creation
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(50, ErrorMessage = "Password cannot exceed 50 characters.")] // Consider stronger validation/regex
        public string Password { get; set; }
    }

    // DTO for Employee Login input
    public class EmployeeLoginDTO
    {
        [Required(ErrorMessage = "Login is required.")]
        [StringLength(50, ErrorMessage = "Login cannot exceed 50 characters.")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(50, ErrorMessage = "Password cannot exceed 50 characters.")]
        [DataType(DataType.Password)] // Hint for UI
        public string Password { get; set; }
    }
}