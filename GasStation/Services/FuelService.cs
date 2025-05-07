using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GasStation.Data;
using GasStation.DTO;
using GasStation.Models;

namespace GasStation.Services
{
	public class FuelService
	{
        private readonly GasStationDbContext _context;
        public FuelService(GasStationDbContext context) 
		{
			_context = context; 
		}

        public List<FuelDTO> GetAllFuels()
        {
            var fuels = _context.Fuels.ToList();
            return fuels.Select(f => new FuelDTO
            {
                FuelId = f.FuelId,
                FuelName = f.FuelName,
                DistributorNumber = f.DistributorNumber
            }).ToList();
        }

        public FuelDTO AddFuel(FuelDTO fuelDTO)
        {
            var fuel = new Fuel
            {
                FuelName = fuelDTO.FuelName,
                DistributorNumber = fuelDTO.DistributorNumber
            };
            _context.Fuels.Add(fuel);
            _context.SaveChanges();
            return fuelDTO;
        }

        public void RemoveFuel(FuelDTO fuelDTO)
        {
            var fuel = _context.Fuels.Find(fuelDTO.FuelId);
            if (fuel != null)
            {
                _context.Fuels.Remove(fuel);
                _context.SaveChanges();
            }
        }

        public FuelDTO GetFuelById(int fuelId)
        {
            var fuel = _context.Fuels.Find(fuelId);
            if (fuel == null)
            {
                return null; // Fuel not found
            }
            return new FuelDTO
            {
                FuelId = fuel.FuelId,
                FuelName = fuel.FuelName,
                DistributorNumber = fuel.DistributorNumber
            };
        }

        public FuelPriceHistory GetCurrentFuelPrice(int fuelId, DateTime atTime)
        {
            var currentPrice = _context.FuelPriceHistories
                                   .Where(h => h.FuelId == fuelId)
                                   .Where(h => h.DateFrom <= atTime) // Start date is on or before the time
                                   .Where(h => h.DateTo == null || h.DateTo > atTime) // End date is null OR after the time
                                   .OrderByDescending(h => h.DateFrom) // Get the most recent applicable price
                                   .FirstOrDefault();

            return currentPrice; // Returns the entity or null
        }

    }
}
