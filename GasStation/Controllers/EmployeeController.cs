using System;
using System.Collections.Generic;
using System.Web.Mvc;
using GasStation.Services; // Pamiętaj o dodaniu using do Twoich serwisów
using GasStation.DTO; // Pamiętaj o dodaniu using do Twoich DTOs
using GasStation.Models; // Pamiętaj o dodaniu using do Twoich Models
						 // Jeśli masz własne wyjątki biznesowe (np. BusinessLogicException), dodaj using
						 // using GasStation.Exceptions;


namespace GasStation.Controllers
{
	// Kontroler do zarządzania pracownikami
	public class EmployeeController : Controller
	{
		// Prywatne pole do przechowywania instancji serwisu EmployeeService
		private readonly EmployeeService _employeeService;

		// Konstruktor, do którego kontener DI wstrzyknie instancję EmployeeService
		public EmployeeController(EmployeeService employeeService)
		{
			_employeeService = employeeService;
		}

		// GET: Employee
		// Akcja wyświetlająca listę wszystkich pracowników
		public ActionResult Index()
		{
			try
			{
				// Wywołaj metodę serwisu, aby pobrać listę pracowników
				List<EmployeeDTO> employees = _employeeService.GetAllEmployees();

				// Przekaż listę pracowników do widoku
				return View(employees);
			}
			catch (Exception ex)
			{
				// Obsługa błędów
				ViewBag.ErrorMessage = "Wystąpił błąd podczas pobierania listy pracowników: " + ex.Message;
				return View("Error"); // Zakładając, że masz widok Error.cshtml
			}
		}

		// GET: Employee/Details/12345678901 (gdzie 12345678901 to PESEL pracownika)
		// Akcja wyświetlająca szczegóły pojedynczego pracownika
		public ActionResult Details(string pesel)
		{
			if (string.IsNullOrEmpty(pesel))
			{
				return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
			}

			try
			{
				// Wywołaj metodę serwisu, aby pobrać pracownika po PESEL
				EmployeeDTO employee = _employeeService.GetEmployeeByPesel(pesel);

				if (employee == null)
				{
					return HttpNotFound();
				}

				// Przekaż obiekt pracownika do widoku
				return View(employee);
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"Wystąpił błąd podczas pobierania szczegółów pracownika o PESEL {pesel}: " + ex.Message;
				return View("Error");
			}
		}

		// GET: Employee/Create
		// Akcja wyświetlająca formularz do dodawania nowego pracownika
		public ActionResult Create()
		{
			// Zwróć widok z formularzem. Używamy CreateEmployeeDTO, ponieważ formularz będzie zawierał hasło
			return View(new CreateEmployeeDTO());
		}

		// POST: Employee/Create
		// Akcja obsługująca wysłanie formularza dodawania nowego pracownika
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(CreateEmployeeDTO employeeDto) // Przyjmujemy CreateEmployeeDTO z danymi formularza
		{
			if (ModelState.IsValid)
			{
				try
				{
					// Wywołaj metodę serwisu, aby dodać nowego pracownika
					_employeeService.AddEmployee(employeeDto);

					// Po pomyślnym dodaniu, przekieruj użytkownika do listy pracowników
					return RedirectToAction("Index");
				}
				// Obsługa własnych wyjątków biznesowych z serwisu
				// catch (BusinessLogicException ex)
				// {
				//     ModelState.AddModelError("", ex.Message);
				//     return View(employeeDto);
				// }
				catch (Exception ex)
				{
					ModelState.AddModelError("", "Wystąpił błąd podczas dodawania pracownika: " + ex.Message);
					return View(employeeDto);
				}
			}

			// Jeśli Model state nie jest poprawny, wróć do widoku formularza z danymi i błędami walidacji
			return View(employeeDto);
		}

		// GET: Employee/Edit/12345678901 (gdzie 12345678901 to PESEL pracownika)
		// Akcja wyświetlająca formularz do edycji danych pracownika
		public ActionResult Edit(string pesel)
		{
			if (string.IsNullOrEmpty(pesel))
			{
				return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
			}

			try
			{
				// Pobierz dane pracownika do edycji. Używamy EmployeeDTO (bez hasła),
				// ponieważ edycja danych i zmiana hasła to osobne operacje.
				EmployeeDTO employee = _employeeService.GetEmployeeByPesel(pesel);

				if (employee == null)
				{
					return HttpNotFound();
				}

				// Przekaż dane pracownika do widoku formularza edycji
				return View(employee);
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"Wystąpił błąd podczas ładowania danych pracownika o PESEL {pesel} do edycji: " + ex.Message;
				return View("Error");
			}
		}

		// POST: Employee/Edit/12345678901
		// Akcja obsługująca wysłanie formularza edycji danych pracownika
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(EmployeeDTO employeeDto) // Przyjmujemy EmployeeDTO z zaktualizowanymi danymi (bez hasła)
		{
			// PESEL powinien być zawarty w employeeDto, zazwyczaj jako ukryte pole w formularzu
			if (string.IsNullOrEmpty(employeeDto.Pesel))
			{
				ModelState.AddModelError("", "PESEL pracownika jest wymagany do edycji.");
				return View(employeeDto);
			}

			if (ModelState.IsValid)
			{
				try
				{
					// Wywołaj metodę serwisu, aby zaktualizować dane pracownika
					_employeeService.UpdateEmployee(employeeDto);

					// Po pomyślnej edycji, przekieruj do szczegółów lub listy
					return RedirectToAction("Details", new { pesel = employeeDto.Pesel }); // Przekieruj do szczegółów edytowanego pracownika
				}
				// Obsługa własnych wyjątków biznesowych
				// catch (BusinessLogicException ex)
				// {
				//     ModelState.AddModelError("", ex.Message);
				//     return View(employeeDto);
				// }
				catch (Exception ex)
				{
					ModelState.AddModelError("", "Wystąpił błąd podczas aktualizacji danych pracownika: " + ex.Message);
					return View(employeeDto);
				}
			}

			// Jeśli Model state nie jest poprawny, wróć do widoku formularza z danymi i błędami walidacji
			return View(employeeDto);
		}

		// GET: Employee/Delete/12345678901
		// Akcja wyświetlająca stronę z potwierdzeniem usunięcia pracownika
		public ActionResult Delete(string pesel)
		{
			if (string.IsNullOrEmpty(pesel))
			{
				return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
			}

			try
			{
				// Pobierz pracownika, którego chcemy usunąć, aby wyświetlić jego dane na stronie potwierdzenia
				EmployeeDTO employee = _employeeService.GetEmployeeByPesel(pesel);

				if (employee == null)
				{
					return HttpNotFound();
				}

				// Przekaż dane pracownika do widoku potwierdzenia
				return View(employee);
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"Wystąpił błąd podczas przygotowywania do usunięcia pracownika o PESEL {pesel}: " + ex.Message;
				return View("Error");
			}
		}

		// POST: Employee/Delete/12345678901
		// Akcja obsługująca potwierdzenie usunięcia pracownika
		[HttpPost, ActionName("Delete")] // Określamy nazwę akcji jako "Delete" dla routingu POST
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(string pesel) // Przyjmujemy PESEL pracownika do usunięcia
		{
			if (string.IsNullOrEmpty(pesel))
			{
				return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
			}

			try
			{
				// Wywołaj metodę serwisu, aby usunąć pracownika
				_employeeService.DeleteEmployee(pesel);

				// Po pomyślnym usunięciu, przekieruj użytkownika do listy pracowników
				return RedirectToAction("Index");
			}
			// Obsługa własnych wyjątków biznesowych (np. gdy pracownik jest powiązany z innymi danymi)
			// catch (BusinessLogicException ex)
			// {
			//     ViewBag.ErrorMessage = "Nie można usunąć pracownika: " + ex.Message;
			//     // Wróć do strony potwierdzenia usunięcia z komunikatem o błędzie
			//     EmployeeDTO employee = _employeeService.GetEmployeeByPesel(pesel); // Spróbujmy ponownie pobrać pracownika
			//     return View("Delete", employee); // Wróć do widoku Delete
			// }
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"Wystąpił błąd podczas usuwania pracownika o PESEL {pesel}: " + ex.Message;
				// Wróć do strony potwierdzenia usunięcia z komunikatem o błędzie
				EmployeeDTO employee = _employeeService.GetEmployeeByPesel(pesel); // Spróbujmy ponownie pobrać pracownika
				return View("Delete", employee); // Wróć do widoku Delete
			}
		}

		// --- Dodatkowe Akcje (opcjonalnie, często w osobnym kontrolerze np. AccountController) ---
		// Akcje do zmiany hasła czy logowania/uwierzytelniania często są umieszczane w dedykowanym kontrolerze
		// (np. AccountController lub AuthController) ze względów bezpieczeństwa i organizacji.
		// Jeśli chcesz je dodać tutaj, musiałbyś stworzyć odpowiednie widoki i logikę:

		// GET: Employee/ChangePassword/12345678901
		// Public ActionResult ChangePassword(string pesel) { ... }

		// POST: Employee/ChangePassword
		// [HttpPost]
		// [ValidateAntiForgeryToken]
		// Public ActionResult ChangePassword(ChangePasswordDTO passwordDto) { ... } // Potrzebujesz DTO dla zmiany hasła (stare, nowe)

		// GET: Employee/Login (mniej typowe, zazwyczaj w AccountController)
		// Public ActionResult Login() { ... }

		// POST: Employee/Login
		// [HttpPost]
		// [ValidateAntiForgeryToken]
		// Public ActionResult Login(EmployeeLoginDTO loginDto) { ... } // Używasz EmployeeLoginDTO i metody Authenticate z serwisu


		// --- Potrzebne Views ---
		// Dla każdej akcji zwracającej View(), potrzebujesz odpowiadającego pliku .cshtml w folderze Views/Employee/
		// Views/Employee/Index.cshtml (Model: List<EmployeeDTO>)
		// Views/Employee/Details.cshtml (Model: EmployeeDTO)
		// Views/Employee/Create.cshtml (Model: CreateEmployeeDTO) - dla formularza dodawania (zawiera hasło)
		// Views/Employee/Edit.cshtml (Model: EmployeeDTO) - dla formularza edycji danych (BEZ hasła)
		// Views/Employee/Delete.cshtml (Model: EmployeeDTO) - dla potwierdzenia usunięcia
		// Views/Shared/Error.cshtml (opcjonalnie, dla ogólnych błędów)
	}
}