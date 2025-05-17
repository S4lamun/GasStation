using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GasStation.DTO
{
        public class FuelDTO
    {
        public int FuelId { get; set; }         public string FuelName { get; set; }
       
    }

    public class CreateFuelDTO
    {
        [Required(ErrorMessage = "Nazwa paliwa jest wymagana.")]
        [StringLength(50, ErrorMessage = "Nazwa paliwa nie może przekraczać 50 znaków.")]
        public string FuelName { get; set; }

    }
}