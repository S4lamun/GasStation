// Customer.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace GasStation.Models
{
    [Table("Customers")] // Tabela dla Customer
    public class Customer : Person
    {
      
        [Key]
        public string Pesel { get; set; } 

        [StringLength(20)]
        public string CardNumber { get; set; }

        [StringLength(50)]
        public string Company { get; set; }

        //Navigation properties
        public virtual ICollection<Order> Orders { get; set; }
    }
}