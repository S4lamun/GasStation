using System;
using System.Collections.Generic;
using System.Web.Mvc;
using GasStation.Services; // Pamiętaj o dodaniu using do Twoich serwisów
using GasStation.DTO; // Pamiętaj o dodaniu using do Twoich DTOs
using GasStation.Models;
using System.Linq; // Pamiętaj o dodaniu using do Twoich Models
				   // Jeśli masz własne wyjątki biznesowe, dodaj using
				   // using GasStation.Exceptions;


namespace GasStation.Controllers
{
    // Kontroler do zarządzania klientami
    public class CashierController : Controller
    {
        private readonly FuelService _fuelService;
		private readonly CustomerService _customerService; // Dodajemy serwis klienta
        private static readonly Dictionary<int, string> fuelTypesMap = new Dictionary<int, string>
        {
            {1, "Benzyna"},
            {2, "Diesel"},
            {3, "Gaz"}
        };

        public CashierController()
        {
            // Domyślny konstruktor
        }

        public CashierController(FuelService fuelService, CustomerService customerService)
        {
            _fuelService = fuelService;
			_customerService = customerService;
        }

        public ActionResult Cashier_view()
        {
            try
            {
                // Pobierz aktualne ceny paliw przez serwis
                var fuelPrices = _fuelService.GetAllCurrentPrices().ToDictionary(f => f.FuelId, f => f.Price);

                var items = Enumerable.Range(1, 6).Select(i =>
                {
                    var fuelId = (i % 3) + 1; // Cykl 1-3 dla różnych paliw
                    return new RefuelingEntryDTO
                    {
                        RefuelingEntryId = i,
                        Amount = 10.0m + i * 2,
                        OrderId = 100 + i,
                        FuelId = fuelId,
                        FuelName = fuelTypesMap.ContainsKey(fuelId) ? fuelTypesMap[fuelId] : $"Paliwo {fuelId}",
                        PriceAtSale = fuelPrices.ContainsKey(fuelId) ? fuelPrices[fuelId] : 5.50m
                    };
                }).ToList();

                ViewBag.Items =  new List<RefuelingEntryDTO>(items); 
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Wystąpił błąd podczas inicjalizacji widoku kasjera: " + ex.Message;
                return View("Error");
            }
        }
    

		// GET: Customer/Details/5 (gdzie 5 to NIP klienta)
		// Akcja wyświetlająca szczegóły pojedynczego klienta
		public ActionResult Details(string nip)
		{
			if (string.IsNullOrEmpty(nip))
			{
				// Jeśli NIP nie został podany, zwróc błąd 400 Bad Request
				return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
			}

			try
			{
				// Wywołaj metodę serwisu, aby pobrać klienta po NIP
				CustomerDTO customer = _customerService.GetCustomerByPesel(nip); // Metoda nazywa się GetCustomerByPesel, ale przyjmuje nip

				if (customer == null)
				{
					// Jeśli klient o podanym NIP nie został znaleziony, zwróć błąd 404 Not Found
					return HttpNotFound();
				}

				// Przekaż obiekt klienta do widoku
				return View(customer);
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"Wystąpił błąd podczas pobierania szczegółów klienta o NIP {nip}: " + ex.Message;
				return View("Error");
			}
		}

		// GET: Customer/Create
		// Akcja wyświetlająca formularz do dodawania nowego klienta
		public ActionResult Create()
		{
			// Zwróć widok z formularzem. Możesz przekazać pusty obiekt DTO,
			// aby ułatwić generowanie pól formularza w widoku (@model CustomerDTO)
			return View(new CustomerDTO());
		}

		// POST: Customer/Create
		// Akcja obsługująca wysłanie formularza dodawania nowego klienta
		// W GasStation.Controllers.CustomerController


		// POST: Customer/Delete/5
		// Akcja obsługująca potwierdzenie usunięcia klienta
		[HttpPost, ActionName("Delete")] // Określamy nazwę akcji jako "Delete", aby odróżnić POST od GET o tej samej nazwie
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(string nip) // Przyjmujemy NIP klienta do usunięcia
		{
			if (string.IsNullOrEmpty(nip))
			{
				return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
			}

			try
			{
				// Twoja metoda RemoveCustomer przyjmuje CustomerDTO.
				// Możemy albo pobrać CustomerDTO najpierw, albo zmienić metodę serwisu,
				// aby przyjmowała sam NIP (co jest częstsze przy usuwaniu po ID).
				// Zakładając, że chwilowo używamy Twojej obecnej metody, pobierzmy DTO:

				CustomerDTO customerToDelete = _customerService.GetCustomerByPesel(nip);

				if (customerToDelete == null)
				{
					// Klient już nie istnieje, możemy uznać to za sukces i przekierować
					return RedirectToAction("Index");
				}

				// Wywołaj metodę serwisu, aby usunąć klienta
				_customerService.RemoveCustomer(customerToDelete);

				// Po pomyślnym usunięciu, przekieruj użytkownika do listy klientów
				return RedirectToAction("Index");
			}
			// Możesz dodać bardziej szczegółowe bloki catch dla wyjątków, np.
			// jeśli klienta nie można usunąć z powodu powiązań w bazie danych.
			// catch (BusinessLogicException ex)
			// {
			//     // Jeśli serwis rzucił wyjątek biznesowy (np. nie można usunąć klienta powiązanego z danymi)
			//     ViewBag.ErrorMessage = "Nie można usunąć klienta: " + ex.Message;
			//     // Możesz wrócić do strony potwierdzenia usunięcia z komunikatem o błędzie
			//     CustomerDTO customer = _customerService.GetCustomerByPesel(nip); // Spróbujmy ponownie pobrać klienta
			//     return View("Delete", customer); // Wróć do widoku Delete
			// }
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"Wystąpił błąd podczas usuwania klienta o NIP {nip}: " + ex.Message;
				// Możesz wrócić do strony potwierdzenia usunięcia z komunikatem o błędzie
				CustomerDTO customer = _customerService.GetCustomerByPesel(nip); // Spróbujmy ponownie pobrać klienta
				return View("Delete", customer); // Wróć do widoku Delete
			}
		}

		// --- Potrzebne Views ---
		// Dla każdej akcji zwracającej View, potrzebujesz odpowiadającego pliku .cshtml w folderze Views/Customer/
		// Views/Customer/Index.cshtml (Model: List<CustomerDTO>)
		// Views/Customer/Details.cshtml (Model: CustomerDTO)
		// Views/Customer/Create.cshtml (Model: CustomerDTO) - dla formularza dodawania
		// Views/Customer/Delete.cshtml (Model: CustomerDTO) - dla potwierdzenia usunięcia
		// Views/Shared/Error.cshtml (opcjonalnie, dla ogólnych błędów)
		// W GasStation.Controllers.CustomerController

[HttpPost]
[ValidateAntiForgeryToken] // Ten atrybut sprawdzi token wysłany z formularzem
public ActionResult Create(CustomerDTO customerDTO) // Używamy CustomerDTO zgodnie z ustaleniami
{
    // Najpierw czyścimy poprzednie błędy specyficzne dla AJAX, jeśli jakieś były
    if (Request.IsAjaxRequest())
    {
        // Można by wyczyścić ModelState dla pewności, ale zwykle nie jest to konieczne
        // jeśli logika ModelState.AddModelError jest poprawna
    }

    if (ModelState.IsValid)
    {
        try
        {
            _customerService.AddCustomer(customerDTO); // Zakładamy, że ta metoda działa i rzuca wyjątki w razie problemów

            if (Request.IsAjaxRequest())
            {
                // Jeśli żądanie AJAX, zwracamy JSON o sukcesie
                return Json(new { success = true, message = "Klient został pomyślnie dodany." });
            }
            // Dla standardowego żądania, przekierowujemy
            TempData["SuccessMessage"] = "Klient został pomyślnie dodany.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            // Logowanie błędu (ważne!)
            // Logger.LogError(ex, "Błąd podczas dodawania klienta"); 

            if (Request.IsAjaxRequest())
            {
                // Zwracamy ogólny błąd, jeśli nie ma błędów walidacji
                return Json(new { success = false, errors = new[] { "Wystąpił błąd serwera podczas dodawania klienta: " + ex.Message } });
            }
            ModelState.AddModelError("", "Wystąpił błąd podczas dodawania klienta: " + ex.Message);
        }
    }

    // Jeśli ModelState nie jest poprawny
    if (Request.IsAjaxRequest())
    {
        // Zbieramy wszystkie błędy walidacji i zwracamy jako JSON
        var errorList = ModelState.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
        ).Where(m => m.Value.Any()).ToList();

        return Json(new { success = false, errors = errorList, isValidationError = true });
    }

    // Dla standardowego żądania, wracamy do widoku z błędami walidacji
    return View(customerDTO);
}
	}
}