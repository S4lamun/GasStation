// Controllers/CashierController.cs
using GasStation.Services;
using GasStation.DTO;
using System.Web.Mvc;

// ... inne using ...

// Upewnij się, że dostęp do tego kontrolera wymaga logowania

public class CashierController : Controller
{
	private readonly EmployeeService _employeeService;
	// ... inne serwisy, jeśli są wstrzykiwane ...

	//public CashierController()
	//{
	
	//}

	public CashierController(EmployeeService employeeService /*, inne serwisy */)
	{
		_employeeService = employeeService;
		// ... przypisanie innych serwisów ...
	}

	public ActionResult Index()
	{
		string cashierName = "N/A!!"; // Domyślna wartość
		if (User.Identity.IsAuthenticated)
		{
			string login = User.Identity.Name; // Pobierz login zalogowanego użytkownika
			EmployeeDTO cashier = _employeeService.GetEmployeeByLogin(login); // Użyj serwisu do pobrania danych pracownika
			if (cashier != null)
			{
				cashierName = $"{cashier.Name} {cashier.Surname}"; // Połącz imię i nazwisko
			}
			else
			{
				// Sytuacja awaryjna: użytkownik jest zalogowany, ale nie znaleziono go w bazie danych
				// Możesz tu dodać logowanie błędu lub specjalną obsługę
				cashierName = "Error: Cashier not found i elo";
			}
		}
		ViewBag.CurrentCashierFullName = cashierName;

		// ... reszta logiki akcji Index (np. ładowanie zamówienia z sesji, przygotowanie ViewBag dla produktów itp.) ...
		var currentOrder = Session["CurrentOrderSessionKey"] as CreateOrderDTO; // Użyj stałej z poprzedniego przykładu
		if (currentOrder == null)
		{
			currentOrder = new CreateOrderDTO
			{
				Items = new System.Collections.Generic.List<OrderItemDTO>(),
				EmployeePesel = User.Identity.IsAuthenticated ? _employeeService.GetEmployeeByLogin(User.Identity.Name)?.Pesel : null
			};
			Session["CurrentOrderSessionKey"] = currentOrder;
		}
		// Przekazanie PESEL zalogowanego pracownika, jeśli jest potrzebny w formularzu (np. w ukrytym polu)
		if (User.Identity.IsAuthenticated)
		{
			var loggedInEmployee = _employeeService.GetEmployeeByLogin(User.Identity.Name);
			ViewBag.CurrentEmployeePesel = loggedInEmployee?.Pesel; // Przekaż PESEL
			if (string.IsNullOrEmpty(currentOrder.EmployeePesel) && loggedInEmployee != null)
			{
				currentOrder.EmployeePesel = loggedInEmployee.Pesel; // Ustaw w modelu zamówienia jeśli puste
			}
		}


		// Przygotuj dane dla dropdownlist klientów, itp.
		// ViewBag.Customers = new SelectList(_customerService.GetAllCustomers(), "Pesel", "CustomerDisplayFullName");


		return View(currentOrder); // Przekaż model zamówienia do widoku
	}
	public ActionResult Cashier_view()
	{
		// Dodaj logikę pobierania imienia i nazwiska kasjera
		string cashierName = "N/A"; // Domyślna wartość
		EmployeeDTO loggedInEmployee = Session["LoggedInEmployee"] as EmployeeDTO;
		if (loggedInEmployee != null)
		{
			string login = User.Identity.Name; // Pobierz login zalogowanego użytkownika
			System.Diagnostics.Trace.TraceInformation($"Zalogowany login: {login}");

			EmployeeDTO cashier = new EmployeeDTO();
			cashier = _employeeService.GetEmployeeByLogin(login);
		
			if (cashier != null)
			{
				cashierName = $"{cashier.Name} {cashier.Surname}"; // Połącz imię i nazwisko
			}
			else
			{
				// Sytuacja awaryjna: użytkownik jest zalogowany, ale nie znaleziono go w bazie danych
				cashierName = "Error: Cashier not found wiec blad";
				
				// Tutaj warto rozważyć logowanie błędu lub nawet wylogowanie użytkownika
			}
		}
		// *** Ustaw ViewBag.CurrentCashierFullName, aby był dostępny w widoku Cashier_view ***
		ViewBag.CurrentCashierFullName = cashierName;

		// Możesz tutaj dodać inną logikę potrzebną tylko dla widoku kasjera,
		// np. ładowanie danych dla przycisków produktów (jeśli nie są statyczne)
		// lub przygotowanie danych dla modali (jeśli nie są ładowane AJAXem).
		// Jeśli Twój widok Cashier_view używa modelu, musisz go tutaj utworzyć i przekazać.

		// Zwróć widok Cashier_view.cshtml
		return View();
	}

	// ... inne akcje kontrolera ...
}