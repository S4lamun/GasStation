using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.RegularExpressions; 
namespace GasStation.Models
{
    [Table("Customers")]         public class Customer
    {
        [Key]         public string Nip { get; set; }

        [Required]
        [StringLength(50)]
        public string CompanyName { get; set; }

        

                public virtual ICollection<Order> Orders { get; set; }

       
    }
}