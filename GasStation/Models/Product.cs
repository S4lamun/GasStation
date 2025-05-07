using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GasStation.Models
{
	[Table("Product")]
    public class Product
	{
		[Key]
		public int ProductId { get; set; }

		[Required]
		public decimal Price { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        //Navigation property
        public virtual ICollection<OrderSpecification> OrderSpecifications { get; set; }


    }
}