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


                public string CustomerNip { get; set; }
        public string CustomerCompanyName { get; set; } 
        public string EmployeePesel { get; set; }
        public string EmployeeFullName { get; set; } 
                public List<OrderSpecificationDTO> OrderSpecifications { get; set; }

                public decimal TotalAmount { get; set; }     }

        public class CreateOrderDTO
    {
        public string EmployeePesel { get; set; }
        public string CustomerNip { get; set; }
        public string PaymentType { get; set; }
        public List<OrderItemDTO> Items { get; set; } = new List<OrderItemDTO>();
        public List<RefuelingEntryInputDTO> RefuelingEntries { get; set; } = new List<RefuelingEntryInputDTO>();
    }

    
        public class OrderItemDTO
    {
        [Required(ErrorMessage = "Item ID is required.")]
        public int ItemId { get; set; } 
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be positive.")]
        public decimal Quantity { get; set; } 
        [Required(ErrorMessage = "Item type is required.")]
        public bool IsFuel { get; set; }     }

    public class RefuelingEntryInputDTO
    {
        [Required(ErrorMessage = "Fuel ID is required.")]
        public int FuelId { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive.")]
        public decimal Amount { get; set; } 
        [Required(ErrorMessage = "Price at sale is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive.")]
        public decimal PriceAtSale { get; set; }     }


}