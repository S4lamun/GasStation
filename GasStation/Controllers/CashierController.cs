// Controllers/CashierController.cs
using GasStation.Services;
using GasStation.DTO;
using System.Web.Mvc;
using System.Linq;
using System;

// ... inne using ...

// Upewnij się, że dostęp do tego kontrolera wymaga logowania

public class CashierController : Controller
{
	private readonly EmployeeService _employeeService;
    private readonly FuelService _fuelService;
    // ... inne serwisy, jeśli są wstrzykiwane ...

    //public CashierController()
    //{

    //}

    public CashierController(EmployeeService employeeService, FuelService fuelService/*, inne serwisy */)
	{
		_employeeService = employeeService;
        _fuelService = fuelService;
		// ... przypisanie innych serwisów ...
	}

    public ActionResult Cashier_view()
    {
        try
        {
            // Pobierz aktualne ceny paliw
            var fuelPrices = _fuelService.GetAllCurrentPrices().ToDictionary(f => f.FuelId, f => f.Price);

            // Pobierz dostępne typy paliw
            var fuelTypes = _fuelService.GetAllFuels().ToDictionary(f => f.FuelId, f => f.FuelName);

            var items = Enumerable.Range(1, 6).Select(i =>
            {
                var fuelId = (i % 3) + 1; // Cykl 1-3 dla różnych paliw
                return new RefuelingEntryDTO
                {
                    RefuelingEntryId = i,
                    Amount = 10.0m + i * 2,
                    OrderId = 100 + i,
                    FuelId = fuelId,
                    FuelName = fuelTypes.ContainsKey(fuelId) ? fuelTypes[fuelId] : $"Paliwo {fuelId}",
                    PriceAtSale = fuelPrices.ContainsKey(fuelId) ? fuelPrices[fuelId] : 5.50m
                };
            }).ToList();

            ViewBag.Items = items;
            ViewBag.FuelPrices = fuelPrices; // Dodaj ceny do ViewBag

            return View();
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Wystąpił błąd podczas inicjalizacji widoku kasjera: " + ex.Message;
            return View("Error");
        }
    }
   

	// ... inne akcje kontrolera ...
}