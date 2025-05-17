using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GasStation.DTO
{
        public class EmployeeDTO
    {
                public string Pesel { get; set; } 
        public string Name { get; set; }

        public string Surname { get; set; }

                public string Login { get; set; }

        public string FullName => $"{Name} {Surname}";

            }

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

        public string FullName => $"{Name} {Surname}";

                [Required(ErrorMessage = "Password is required.")]
        [StringLength(50, ErrorMessage = "Password cannot exceed 50 characters.")]         public string Password { get; set; }
    }

        public class EmployeeLoginDTO
    {
        [Required(ErrorMessage = "Login is required.")]
        [StringLength(50, ErrorMessage = "Login cannot exceed 50 characters.")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(50, ErrorMessage = "Password cannot exceed 50 characters.")]
        [DataType(DataType.Password)]         public string Password { get; set; }
    }
}