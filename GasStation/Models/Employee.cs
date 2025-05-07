// Employee.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.RegularExpressions; // Dodaj to using

namespace GasStation.Models
{
    [Table("Employees")] // Tabela dla Employee
    // Usunięto dziedziczenie po Person
    public class Employee
    {
        [Key] // Pesel staje się kluczem głównym dla Employee
        public string Pesel { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Surname { get; set; }

        [Required]
        [StringLength(50)]
        public string Login { get; set; }

        [Required]
        [StringLength(50)]
        public string Password { get; set; }

        // Navigation properties
        public virtual ICollection<FuelPriceHistory> HistoriaCenPaliwZmiany { get; set; }
        public virtual ICollection<Order> Orders { get; set; }

        // Metoda przeniesiona z Person
        public bool ValidatePesel()
        {
            return Regex.IsMatch(Pesel, @"^\d{11}$");
        }

        public bool ValidatePassword()
        {
            return (Regex.IsMatch(Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$"));
        }
    }
}