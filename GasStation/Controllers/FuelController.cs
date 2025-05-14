// W pliku GasStation.Controllers/FuelController.cs

using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using GasStation.Services; // Upewnij się, że masz using do serwisu
using GasStation.DTO; // Upewnij się, że masz using do DTO

using GasStation.Services; // Upewnij się, że masz using do modeli
namespace GasStation.Controllers
{

	public class FuelController : Controller
	{
		private readonly FuelService _fuelService; // Zależność od serwisu

		// Konstruktor dla Dependency Injection (DI)
		public FuelController(FuelService fuelService)
		{
			_fuelService = fuelService;
		}

		// ... (inne akcje kontrolera, np. do zarządzania paliwami) ...

		// *** WYMAGANA AKCJA: Zwraca aktualne ceny paliw w formacie JSON ***
		// Adres URL: /Fuel/GetCurrentPricesJson
		[HttpGet] // Odpowiada na żądania HTTP GET
	
		public JsonResult GetCurrentPricesJson()
		{
			try
			{
				// Wywołaj metodę serwisu, aby pobrać listę aktualnych cen
				// Metoda serwisu zwraca List<FuelPriceHistoryDTO>
				var currentPricesFromService = _fuelService.GetAllCurrentPrices();

				// Mapuj DTO z serwisu na format oczekiwany przez kod JavaScript
				// Kod JS oczekuje obiektów z właściwościami FuelId i Price
				var dataForJson = currentPricesFromService.Select(p => new
				{
					FuelId = p.FuelId, // Pobierz FuelId z FuelPriceHistoryDTO
					Price = p.Price   // Pobierz Price z FuelPriceHistoryDTO
				}).ToList();

				// Zwróć dane w formacie JSON
				// JsonRequestBehavior.AllowGet jest wymagane dla żądań GET zwracających JSON
				return Json(dataForJson, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				// Logowanie błędu na serwerze (bardzo ważne w rzeczywistej aplikacji!)
				// np. System.Diagnostics.Trace.TraceError($"Błąd w GetCurrentPricesJson: {ex.Message}");

				// W przypadku błędu, zwróć odpowiedź z błędem HTTP (np. 500 Internal Server Error)
				Response.StatusCode = 500;
				// Opcjonalnie zwróć komunikat błędu w formacie JSON
				return Json(new { message = "Wystąpił błąd serwera podczas ładowania cen paliw." }, JsonRequestBehavior.AllowGet);
			}
		}

		// ... (inne akcje kontrolera) ...
	}
}