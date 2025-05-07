using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GasStation.Models
{
    [Table("RefuelingEntry")]
    public class RefuelingEntry
	{
		[Key]
		public int RefuelingEntryId { get; set; } 

        [Required]
        public decimal Amount { get; set; } 

        [Required] 
        public decimal PriceAtSale { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        [ForeignKey("Fuel")]
        public int FuelId { get; set; }
        public virtual Fuel Fuel { get; set; }

        public virtual ICollection<OrderSpecification> OrderSpecifications { get; set; }

    }
}