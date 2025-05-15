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
            }).ToList();
        }

        public FuelDTO AddFuel(FuelDTO fuelDTO)
        {
            var fuel = new Fuel
            {
                FuelName = fuelDTO.FuelName,
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
		public List<FuelPriceHistoryDTO> GetAllCurrentPrices()
		{
			// Pobierz listę wszystkich paliw z bazy. Potrzebujemy FuelId każdego paliwa.
			var allFuels = _context.Fuels.ToList(); // Zakładamy, że masz kolekcję Fuels w kontekscie (_context.Fuels)

			// Utwórz pustą listę, do której będziemy dodawać DTO z aktualnymi cenami
			var currentPricesDTOs = new List<FuelPriceHistoryDTO>();

			// Przejdź przez każde paliwo, aby znaleźć jego aktualną cenę
			foreach (var fuel in allFuels)
			{
				// Wywołaj metodę GetCurrentFuelPrice, aby znaleźć encję historii cen aktywną TERAZ
				var currentPriceHistoryEntity = GetCurrentFuelPrice(fuel.FuelId, DateTime.Now);

				// Jeśli znaleziono encję historii cen dla tego paliwa (czyli jest aktywna cena)
				if (currentPriceHistoryEntity != null)
				{
					// Stwórz nowy obiekt FuelPriceHistoryDTO i zmapuj na niego dane z encji FuelPriceHistory
					currentPricesDTOs.Add(new FuelPriceHistoryDTO
					{
						FuelPriceHistoryId = currentPriceHistoryEntity.FuelPriceHistoryId, // Mapowanie Id historii ceny
						Price = currentPriceHistoryEntity.Price, // *** Mapowanie WŁAŚCIWEJ WŁAŚCIWOŚCI Z CENĄ z encji FuelPriceHistory ***
						DateFrom = currentPriceHistoryEntity.DateFrom, // Mapowanie daty rozpoczęcia
						DateTo = currentPriceHistoryEntity.DateTo,     // Mapowanie daty zakończenia (może być null)
						FuelId = currentPriceHistoryEntity.FuelId,     // Mapowanie Id paliwa
																	   // Pamiętaj, że FuelPriceHistoryDTO zawiera FuelName i EmployeeFullName/Pesel.
																	   // Jeśli potrzebujesz tych danych w tym DTO, a nie są one bezpośrednio w encji FuelPriceHistory,
																	   // musisz je pobrać z powiązanych encji Fuel i Employee i zmapować tutaj.
																	   // Jeśli na froncie potrzebujesz tylko FuelId i Price, te pola w DTO mogą być puste.
																	   // Przykład pobrania nazwy paliwa z encji Fuel, jeśli FuelPriceHistoryEntity jej nie zawiera:
																	   // FuelName = fuel.FuelName 
					});
				}
				// Jeśli dla danego FuelId nie ma aktywnej ceny (GetCurrentFuelPrice zwróci null), 
				// to paliwo nie zostanie dodane do listy zwracanych DTO z cenami.
			}

			return currentPricesDTOs; // Zwróć listę DTO z aktualnymi cenami
		}

		// ... Tutaj mogą znajdować się inne metody serwisu FuelService, 
		// np. do dodawania/usuwania paliw, zarządzania cenami, itp. ...
		// public List<FuelDTO> GetAllFuels() { ... }
		// public FuelDTO AddFuel(CreateFuelDTO fuelDTO) { ... } 
		// public void UpdateFuelPrice(CreateFuelPriceHistoryDTO priceDTO) { ... }
	}
}

