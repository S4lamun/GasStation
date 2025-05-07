using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GasStation.DTO
{
    // DTO for displaying, creating, or updating Fuel
    public class FuelDTO
    {
        // ID is included for display/update, not needed for creation input normally
        public int FuelId { get; set; }

        [Required(ErrorMessage = "Fuel name is required.")]
        [StringLength(50, ErrorMessage = "Fuel name cannot exceed 50 characters.")]
        public string FuelName { get; set; }

        [Required(ErrorMessage = "Distributor number is required.")]
        [StringLength(50, ErrorMessage = "Distributor number cannot exceed 50 characters.")]
        public string DistributorNumber { get; set; }
    }
}