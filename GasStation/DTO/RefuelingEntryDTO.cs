using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GasStation.DTO
{
	public class RefuelingEntryDTO
	{
        public int RefuelingEntryId { get; set; }

        public decimal Amount { get; set; }

        // Related entity data for display
        public int OrderId { get; set; }

        public int FuelId { get; set; }
        public string FuelName { get; set; } // Name of the fuel

        public decimal PriceAtSale { get; set; } // Price at the time of sale
    }
}