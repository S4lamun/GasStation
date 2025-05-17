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
        public decimal PriceAtSale { get; set; }
        public int FuelId { get; set; }
        public string FuelName { get; set; }

        public int OrderId { get; set; }
    }
}