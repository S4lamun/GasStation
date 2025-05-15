using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GasStation.Models
{
    [Table("Fuel")]
    public class Fuel
    {
        [Key]
        public int FuelId { get; set; }

        [Required]
        [StringLength(50)]
        public string FuelName { get; set; } //petrol, diesel, LPG

        //Navigation properties
        public virtual ICollection<FuelPriceHistory> FuelPriceHistories { get; set; }
        public virtual ICollection<RefuelingEntry> RefuelingEntries { get; set; }

    }
}