using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace GasStation.Models
{
    [Table("Employees")] // Tabela dla Employee
    public class Employee : Person
    {
        // Klucz główny tabeli Employees, NAZWANY TAK SAMO JAK KLUCZ PERSON (Pesel)
        // i będzie jednocześnie kluczem obcym do tabeli People.
        [Key]
        public string Pesel { get; set; } 

        [Required]
        [StringLength(50)]
        public string Login { get; set; }

        [Required]
        [StringLength(50)]
        public string Password { get; set; }

        //Navigation properties
        public virtual ICollection<FuelPriceHistory> HistoriaCenPaliwZmiany { get; set; }
        public virtual ICollection<Order> Orders { get; set; }

        public bool ValidatePassword()
        {
            return (System.Text.RegularExpressions.Regex.IsMatch(Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$"));
        }
    }
}