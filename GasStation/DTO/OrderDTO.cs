using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GasStation.DTO
{
	public class OrderDTO
	{
        public int OrderId { get; set; }


        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime OrderDate { get; set; }

        public string PaymentType { get; set; }


        // Related entity data for display
        public string CustomerNip { get; set; }
        public string CustomerCompanyName { get; set; } // Full name of the customer

        public string EmployeePesel { get; set; }
        public string EmployeeFullName { get; set; } // Full name of the employee

        // Include line items for display
        public List<OrderSpecificationDTO> OrderSpecifications { get; set; }

        // Calculated field for display
        public decimal TotalAmount { get; set; } // Sum of all item totals
    }

    // DTO for creating a new Order (input)
    public class CreateOrderDTO
    {
        public string EmployeePesel { get; set; }
        public string CustomerNip { get; set; }
        public string PaymentType { get; set; }
        public List<OrderItemDTO> Items { get; set; } = new List<OrderItemDTO>();
    }

    
    // DTO for a single item when creating an Order (used within CreateOrderDto)
    public class OrderItemDTO
    {
        [Required(ErrorMessage = "Item ID is required.")]
        public int ItemId { get; set; } // ID of Fuel OR Product

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be positive.")]
        public decimal Quantity { get; set; } // Amount for fuel (liters) OR count for product (pieces)

        [Required(ErrorMessage = "Item type is required.")]
        public bool IsFuel { get; set; } // True if it's fuel, False if it's a product
    }

  

}