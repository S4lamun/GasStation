// CustomerDTO.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GasStation.DTO
{
    // Usunięto dziedziczenie po PersonDTO
    public class CustomerDTO
    {
        // Dodano pola z PersonDTO
        public string Pesel { get; set; } // DTO zazwyczaj nie wymaga [Key]

        public string Name { get; set; }

        public string Surname { get; set; }

        // Pola specyficzne dla Customer
        public string CardNumber { get; set; }

        public string Company { get; set; }
    }

    // Ta klasa CreateCustomerDTO pozostaje bez zmian, ponieważ już miała wszystkie potrzebne pola
    public class CreateCustomerDTO
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

        [StringLength(20, ErrorMessage = "Card number cannot exceed 20 characters.")]
        public string CardNumber { get; set; }

        [StringLength(50, ErrorMessage = "Company name cannot exceed 50 characters.")]
        public string Company { get; set; }
    }
}