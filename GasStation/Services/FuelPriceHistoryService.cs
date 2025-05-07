// Services/FuelPriceHistoryService.cs
using GasStation.Data; // DbContext
using GasStation.DTO; // DTOs
using GasStation.Models; // EF Entities
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity; // For Include()
using System;

namespace GasStation.Services
{
    public class FuelPriceHistoryService
    {
        private readonly GasStationDbContext _context;
        private readonly FuelService _fuelService;
        private readonly EmployeeService _employeeService;

        public FuelPriceHistoryService(GasStationDbContext context, FuelService fuelService, EmployeeService employeeService)
        {
            _context = context;
            _fuelService = fuelService;
            _employeeService = employeeService;
        }

        // --- Private Helper Methods for Mapping ---

        // Maps FuelPriceHistory entity (with included related data) to FuelPriceHistoryDTO
        private FuelPriceHistoryDTO MapToDto(FuelPriceHistory history)
        {
            if (history == null) return null;

            return new FuelPriceHistoryDTO
            {
                FuelPriceHistoryId = history.FuelPriceHistoryId,
                Price = history.Price,
                DateFrom = history.DateFrom,
                DateTo = history.DateTo,

                FuelId = history.FuelId,
                FuelName = history.Fuel?.FuelName,

                EmployeePesel = history.EmployeePesel,
                EmployeeFullName = $"{history.Employee?.Name} {history.Employee?.Surname}"
            };
        }


        // --- Public Service Methods ---

        // Get all price history entries
        public List<FuelPriceHistoryDTO> GetAllPriceHistories()
        {
            var history = _context.FuelPriceHistories
                                  .Include(h => h.Fuel)
                                  .Include(h => h.Employee)
                                  .OrderByDescending(h => h.DateFrom)
                                  .ToList();

            return history.Select(h => MapToDto(h)).ToList();
        }

        // Get price history entry by ID
        public FuelPriceHistoryDTO GetPriceHistoryById(int id)
        {
            var history = _context.FuelPriceHistories
                                  .Include(h => h.Fuel)
                                  .Include(h => h.Employee)
                                  .SingleOrDefault(h => h.FuelPriceHistoryId == id);

            return MapToDto(history);
        }

        // Get price history entries for a specific fuel
        public List<FuelPriceHistoryDTO> GetPriceHistoryForFuel(int fuelId)
        {
            var fuelExists = _context.Fuels.Any(f => f.FuelId == fuelId);
            if (!fuelExists)
            {
                throw new BusinessLogicException($"Fuel with ID {fuelId} not found.");
            }

            var history = _context.FuelPriceHistories
                                  .Include(h => h.Fuel)
                                  .Include(h => h.Employee)
                                  .Where(h => h.FuelId == fuelId)
                                  .OrderByDescending(h => h.DateFrom)
                                  .ToList();

            return history.Select(h => MapToDto(h)).ToList();
        }


        // Add a new price entry
        public FuelPriceHistoryDTO AddNewPrice(CreateFuelPriceHistoryDTO newPriceDto)
        {
            if (newPriceDto == null) throw new ArgumentNullException(nameof(newPriceDto));

            var fuel = _context.Fuels.Find(newPriceDto.FuelId);
            if (fuel == null) throw new BusinessLogicException($"Fuel with ID {newPriceDto.FuelId} not found.");

            var employee = _context.Employees.Find(newPriceDto.EmployeePesel);
            if (employee == null) throw new BusinessLogicException($"Employee with PESEL {newPriceDto.EmployeePesel} not found.");

            // Determine the actual start date for the new price entry
            DateTime startDateForNew = newPriceDto.DateFrom ?? DateTime.Now;

            var currentActivePrice = _context.FuelPriceHistories
                                             .Where(h => h.FuelId == newPriceDto.FuelId)
                                             .Where(h => h.DateTo == null || h.DateTo > startDateForNew)
                                             .OrderByDescending(h => h.DateFrom)
                                             .FirstOrDefault();

            if (currentActivePrice != null)
            {
                if (startDateForNew <= currentActivePrice.DateFrom)
                {
                    throw new BusinessLogicException($"New price start date ({startDateForNew}) must be after the previous active price start date ({currentActivePrice.DateFrom}).");
                }

                // Set the end date of the previous price to just before the new price starts
                currentActivePrice.DateTo = startDateForNew.AddMilliseconds(-1);
            }

            var newPriceEntry = new FuelPriceHistory
            {
                Price = newPriceDto.Price,
                DateFrom = startDateForNew,
                DateTo = null,

                FuelId = newPriceDto.FuelId,
                EmployeePesel = newPriceDto.EmployeePesel
            };

            _context.FuelPriceHistories.Add(newPriceEntry);
            _context.SaveChanges();

            var createdEntry = _context.FuelPriceHistories
                                      .Include(h => h.Fuel)
                                      .Include(h => h.Employee)
                                      .SingleOrDefault(h => h.FuelPriceHistoryId == newPriceEntry.FuelPriceHistoryId);

            return MapToDto(createdEntry);
        }

        // Update an existing price history entry (use with caution)
        public FuelPriceHistoryDTO UpdatePriceEntry(FuelPriceHistoryDTO historyDto)
        {
            var existingEntry = _context.FuelPriceHistories.Find(historyDto.FuelPriceHistoryId);
            if (existingEntry == null)
            {
                throw new BusinessLogicException($"Fuel price history entry with ID {historyDto.FuelPriceHistoryId} not found for update.");
            }

            // Complex date range validation
            DateTime endDateToCheck = historyDto.DateTo ?? DateTime.MaxValue;

            var overlappingEntries = _context.FuelPriceHistories
                                             .Where(h => h.FuelId == existingEntry.FuelId)
                                             .Where(h => h.FuelPriceHistoryId != existingEntry.FuelPriceHistoryId)
                                             .Where(h =>
                                                 (h.DateFrom < endDateToCheck) &&
                                                 ((h.DateTo == null) || (h.DateTo > historyDto.DateFrom))
                                             )
                                             .ToList();

            if (overlappingEntries.Any())
            {
                throw new BusinessLogicException($"Updated dates for entry {historyDto.FuelPriceHistoryId} cause overlaps with other price history entries for fuel ID {existingEntry.FuelId}.");
            }

            // Check that start date is before end date if end date is provided
            if (historyDto.DateTo.HasValue && historyDto.DateFrom >= historyDto.DateTo.Value)
            {
                throw new BusinessLogicException("Start date must be before end date.");
            }

            // Update properties
            existingEntry.Price = historyDto.Price;
            existingEntry.DateFrom = historyDto.DateFrom;
            existingEntry.DateTo = historyDto.DateTo;

            _context.SaveChanges();

            var updatedEntry = _context.FuelPriceHistories
                                       .Include(h => h.Fuel)
                                       .Include(h => h.Employee)
                                       .SingleOrDefault(h => h.FuelPriceHistoryId == historyDto.FuelPriceHistoryId);

            return MapToDto(updatedEntry);
        }

        // Delete a price history entry (use with extreme caution)
        public void DeletePriceEntry(int id)
        {
            var entryToDelete = _context.FuelPriceHistories
                                        .SingleOrDefault(h => h.FuelPriceHistoryId == id);

            if (entryToDelete == null)
            {
                return;
            }

            // Complex dependency check
            var isUsedInRefueling = _context.RefuelingEntries
                                            .Include(re => re.Order)
                                            .Any(re => re.FuelId == entryToDelete.FuelId &&
                                                        re.Order.OrderDate >= entryToDelete.DateFrom &&
                                                        (entryToDelete.DateTo == null || re.Order.OrderDate <= entryToDelete.DateTo)
                                            );

            if (isUsedInRefueling)
            {
                throw new BusinessLogicException($"Cannot remove fuel price history entry {id} as it was used for refueling transactions.");
            }

            // Handle potential gaps/overlaps with adjacent entries after deletion (logic omitted)

            _context.FuelPriceHistories.Remove(entryToDelete);
            _context.SaveChanges();
        }
    }
}