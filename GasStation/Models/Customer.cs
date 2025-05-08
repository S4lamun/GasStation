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
        public string Nip { get; set; }

        [Required]
        [StringLength(50)]
        public string CompanyName { get; set; }

        

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; }

       
    }
}