// Person.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; // Upewnij się, że jest

namespace GasStation.Models
{
    [Table("People")] // Dodajemy atrybut Table dla jasności w TPT
    public class Person
    {
        [Key]
        public string Pesel { get; set; } 

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Surname { get; set; }

        
        public bool ValidatePesel()
        {
            return System.Text.RegularExpressions.Regex.IsMatch(Pesel, @"^\d{11}$");
        }
    }
}