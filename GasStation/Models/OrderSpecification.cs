using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GasStation.Models
{
    [Table("OrderSpecification")]
    public class OrderSpecification
	{
		[Key]
		public int OrderSpecificationId { get; set; } 

        [ForeignKey("Product")]
        public int? ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Required] 
        public decimal PriceAtSale { get; set; }

        public int Quantity { get; set; }


        [ForeignKey("RefuelingEntry")]
        public int? RefuelingEntryId { get; set; } 

        public virtual RefuelingEntry RefuelingEntry { get; set; } 

        [ForeignKey("Order")]
        public int? OrderId { get; set; } 
        public virtual Order Order{ get; set; } 

    }
}