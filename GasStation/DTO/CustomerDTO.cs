// CustomerDTO.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GasStation.DTO
{
        public class CustomerDTO
    {
        
        public string Nip { get; set; }

        public string CompanyName { get; set; }
    }

    // Ta klasa CreateCustomerDTO pozostaje bez zmian, ponieważ już miała wszystkie potrzebne pola
    public class CreateCustomerDTO
    {
        [Required(ErrorMessage = "NIP is required.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "NIP must be 10 digits.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Pesel must consist of 10 digits.")]
        public string Nip { get; set; }

        [StringLength(50, ErrorMessage = "Company name cannot exceed 50 characters.")]
        public string CompanyName { get; set; }
    }
}