using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GasStation.DTO
{
	public class OrderSpecificationDTO
    {
        public int OrderSpecificationId { get; set; }

        public int Quantity { get; set; } // Quantity


        // Related entity data for display
        // Can be either a Product or a RefuelingEntry
        public int? ProductId { get; set; }
        public string ProductName { get; set; } // Name of the product (if ProductId is not null)
        public decimal? ProductPrice { get; set; } // Price of the product at the time of order (if ProductId is not null)


        public int? RefuelingEntryId { get; set; } // ID of the refueling entry (if RefuelingEntryId is not null)
        public RefuelingEntryDTO RefuelingEntryDetails { get; set; } // Details of the refueling entry

        // It's linked to an Order, but you usually display items *within* an order details page,
        // so OrderId might not be strictly needed in the item DTO itself, but include it for completeness.
        public int? OrderId { get; set; }

        // Calculated field for display
        public decimal ItemTotal { get; set; } // Quantity * Price (for product) or Amount * Price (for fuel)
    }
}