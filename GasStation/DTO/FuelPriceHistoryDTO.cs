using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GasStation.DTO
{
    public class FuelPriceHistoryDTO
    {
        public int FuelPriceHistoryId { get; set; }


        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal Price { get; set; }


        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime DateFrom { get; set; }


        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", NullDisplayText = "Current", ApplyFormatInEditMode = false)]
        public DateTime? DateTo { get; set; }


        // Related entity data for display
        public int FuelId { get; set; }
        public string FuelName { get; set; }

        public string EmployeePesel { get; set; }
        public string EmployeeFullName { get; set; }
    }

    // DTO for creating a new Fuel Price History entry
    public class CreateFuelPriceHistoryDTO
    {
        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        // DateFrom can be optionally provided by user or set to DateTime.Now in Service
        [DataType(DataType.DateTime)]
        public DateTime? DateFrom { get; set; }

        [Required(ErrorMessage = "Fuel selection is required.")]
        public int FuelId { get; set; }

        // Employee Pesel is typically set by the system based on the logged-in user
        [Required(ErrorMessage = "Employee identifier is required.")]
        public string EmployeePesel { get; set; }
    }
}