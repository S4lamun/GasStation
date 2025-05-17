using System;
using System.Collections.Generic;
using System.Linq;
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

        // Pobierz wszystkie paliwa (wersja podstawowa)
        public List<FuelDTO> GetAllFuels()
        {
            return _context.Fuels
                .Select(f => new FuelDTO
                {
                    FuelId = f.FuelId,
                    FuelName = f.FuelName
                })
                .ToList();
        }

        // Pobierz aktualne ceny wszystkich paliw
        public Dictionary<int, decimal> GetCurrentPrices()
        {
            var currentPrices = new Dictionary<int, decimal>();
            var allFuels = _context.Fuels.ToList();

            foreach (var fuel in allFuels)
            {
                var currentPrice = GetCurrentFuelPrice(fuel.FuelId, DateTime.Now);
                if (currentPrice != null)
                {
                    currentPrices.Add(fuel.FuelId, currentPrice.Price);
                }
            }

            return currentPrices;
        }

        // Pobierz aktualną cenę dla konkretnego paliwa
        public FuelPriceHistory GetCurrentFuelPrice(int fuelId, DateTime atTime)
        {
            return _context.FuelPriceHistories
                .Where(h => h.FuelId == fuelId)
                .Where(h => h.DateFrom <= atTime)
                .Where(h => h.DateTo == null || h.DateTo > atTime)
                .OrderByDescending(h => h.DateFrom)
                .FirstOrDefault();
        }

        // Pobierz wszystkie aktualne ceny jako DTO
        public List<FuelPriceHistoryDTO> GetAllCurrentPrices()
        {
            return _context.Fuels
                .Select(fuel => new
                {
                    Fuel = fuel,
                    CurrentPrice = _context.FuelPriceHistories
                        .Where(h => h.FuelId == fuel.FuelId)
                        .Where(h => h.DateFrom <= DateTime.Now)
                        .Where(h => h.DateTo == null || h.DateTo > DateTime.Now)
                        .OrderByDescending(h => h.DateFrom)
                        .FirstOrDefault()
                })
                .Where(x => x.CurrentPrice != null)
                .Select(x => new FuelPriceHistoryDTO
                {
                    FuelPriceHistoryId = x.CurrentPrice.FuelPriceHistoryId,
                    Price = x.CurrentPrice.Price,
                    DateFrom = x.CurrentPrice.DateFrom,
                    DateTo = x.CurrentPrice.DateTo,
                    FuelId = x.CurrentPrice.FuelId,
                    FuelName = x.Fuel.FuelName
                })
                .ToList();
        }

        // Pozostałe metody bez zmian
        public FuelDTO AddFuel(FuelDTO fuelDTO)
        {
            var fuel = new Fuel
            {
                FuelName = fuelDTO.FuelName,
            };
            _context.Fuels.Add(fuel);
            _context.SaveChanges();
            return new FuelDTO
            {
                FuelId = fuel.FuelId,
                FuelName = fuel.FuelName
            };
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
                return null;
            }
            return new FuelDTO
            {
                FuelId = fuel.FuelId,
                FuelName = fuel.FuelName,
            };
        }

    }
}