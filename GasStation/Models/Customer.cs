// Customer.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.RegularExpressions; // Dodaj to using

namespace GasStation.Models
{
    [Table("Customers")] // Tabela dla Customer
    // Usunięto dziedziczenie po Person
    public class Customer
    {
        [Key] // Pesel staje się kluczem głównym dla Customer
        public string Pesel { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Surname { get; set; }

        [StringLength(20)]
        public string CardNumber { get; set; }

        [StringLength(50)]
        public string Company { get; set; }

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; }

        // Metoda przeniesiona z Person
        public bool ValidatePesel()
        {
            return Regex.IsMatch(Pesel, @"^\d{11}$");
        }
    }
}