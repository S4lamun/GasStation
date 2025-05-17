using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GasStation.Models
{
    [Table("FuelPriceHistory")]
    public class FuelPriceHistory
    {
        [Key]
        public int FuelPriceHistoryId { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public DateTime DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        
        public int FuelId { get; set; }
        [ForeignKey("FuelId")]
        public virtual Fuel Fuel { get; set; }

        
        public string EmployeePesel { get; set; }
        [ForeignKey("EmployeePesel")] 
        public virtual Employee Employee { get; set; }
    }
}