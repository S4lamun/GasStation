using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GasStation.DTO
{
    // DTO for displaying, creating, or updating Product
    public class ProductDTO 
    {
        // ID is included for display/update, not needed for creation input normally
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(50, ErrorMessage = "Product name cannot exceed 50 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
    }
}