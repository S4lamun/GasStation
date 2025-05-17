using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System;

namespace GasStation.Models
{
    [Table("Order")]
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentType { get; set; }

        public string CustomerPesel { get; set; }

        
        [ForeignKey("CustomerPesel")] 
        public virtual Customer Customer { get; set; }

       
        public string EmployeePesel { get; set; }
        [ForeignKey("EmployeePesel")] 
        public virtual Employee Employee { get; set; }


        [Required]
        public decimal TotalAmount { get; set; } 

                public virtual ICollection<RefuelingEntry> RefuelingEntries { get; set; }
        public virtual ICollection<OrderSpecification> OrderSpecifications { get; set; }
    }
}