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
        public int FuelId { get; set; } // ID będzie generowane
        public string FuelName { get; set; }
       
    }

    public class CreateFuelDTO
    {
        [Required(ErrorMessage = "Nazwa paliwa jest wymagana.")]
        [StringLength(50, ErrorMessage = "Nazwa paliwa nie może przekraczać 50 znaków.")]
        public string FuelName { get; set; }

    }
}