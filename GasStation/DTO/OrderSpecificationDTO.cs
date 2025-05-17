using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GasStation.DTO
{
    public class OrderSpecificationDTO
    {
        public int OrderSpecificationId { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtSale { get; set; }
        public int? OrderId { get; set; }

                public int? ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal? ProductPrice { get; set; }

                public int? RefuelingEntryId { get; set; }
        public RefuelingEntryDTO RefuelingEntryDetails { get; set; }

                public decimal ItemTotal { get; set; }
        public bool IsFuel { get; set; }
    }

   
}