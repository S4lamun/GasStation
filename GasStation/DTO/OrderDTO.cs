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
        public string CustomerPesel { get; set; }
        public string CustomerFullName { get; set; } // Full name of the customer

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
        // OrderId is NOT included (auto-generated)
        // OrderDate is typically set in the Service (DateTime.Now)

        [Required(ErrorMessage = "Payment type is required.")]
        [StringLength(50, ErrorMessage = "Payment type cannot exceed 50 characters.")]
        public string PaymentType { get; set; }

        [Required(ErrorMessage = "Customer selection is required.")]
        public string CustomerPesel { get; set; } // PESEL of the customer selected from a list

        [Required(ErrorMessage = "Employee selection is required.")]
        public string EmployeePesel { get; set; } // PESEL of the employee selected from a list (or logged in)

        // Collection of items added to the order
        [Required(ErrorMessage = "Order must contain at least one item.")]
        public List<OrderItemDTO> Items { get; set; } // Using the previously defined OrderItemDto
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