using System;
using System.Web.Mvc;
using GasStation.Services; // Pamiętaj o dodaniu using do Twoich serwisów
using GasStation.DTO;
using System.Web.Security; // Pamiętaj o dodaniu using do Twoich DTOs
                           // Jeśli masz własne wyjątki biznesowe, dodaj using
                           // using GasStation.Exceptions; // example
using System.Web;

namespace GasStation.Controllers
{
    public class HomeController : Controller // Zakładamy, że to Twój HomeController
    {
        // Prywatne pole do przechowywania instancji serwisu EmployeeService
        private readonly EmployeeService _employeeService;

        // Konstruktor, do którego kontener DI wstrzyknie instancję EmployeeService
        public HomeController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // GET: /Home/Index
        // Akcja wyświetlająca formularz logowania (ponieważ jest w Views/Home/Index.cshtml)
        [HttpGet] // Jawnie wskazujemy, że to akcja GET
        public ActionResult Index()
		{// Sprawdź, czy użytkownik jest już uwierzytelniony.
		 // Jeśli tak, można go przekierować bezpośrednio na stronę główną kasjera.
			if (User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Cashier_view", "Cashier");
			}

			// Jeśli nie, wyświetl formularz logowania.
			return View(new EmployeeLoginDTO());
		}

        // POST: /Home/Login
        // Akcja obsługująca wysłanie formularza logowania
        [HttpPost] // Jawnie wskazujemy, że to akcja POST
        [ValidateAntiForgeryToken] // Ochrona przed CSRF
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult Login(EmployeeLoginDTO loginDto) // Model binder wypełni ten obiekt danymi z formularza
		{
			if (ModelState.IsValid)
			{
				try
				{
					// 2. Wywołaj serwis do uwierzytelnienia pracownika (weryfikacja loginu/hasła w bazie)
					EmployeeDTO authenticatedEmployee = _employeeService.Authenticate(loginDto);

					// 3. Sprawdź wynik uwierzytelnienia przez serwis
					if (authenticatedEmployee != null)
					{
						// *** Uwierzytelnienie w serwisie powiodło się! ***

						// *** ZAPISZ DANE PRACOWNIKA W SESJI ***
						// Możesz zapisać cały obiekt DTO
						Session["LoggedInEmployee"] = authenticatedEmployee;
						// Możesz też zapisać tylko pełne imię i nazwisko, jeśli tylko tego potrzebujesz
						// Session["LoggedInEmployeeFullName"] = $"{authenticatedEmployee.Name} {authenticatedEmployee.Surname}";


						// Przekieruj użytkownika na stronę kasjera
						return RedirectToAction("Cashier_view", "Cashier");
					}
					else
					{
						// Uwierzytelnienie w serwisie nie powiodło się
						ModelState.AddModelError("", "Niepoprawna próba logowania. Sprawdź login i hasło.");
					}
				}
				catch (Exception ex)
				{
					// Obsługa innych nieoczekiwanych błędów
					ModelState.AddModelError("", "Wystąpił nieoczekiwany błąd podczas logowania: " + ex.Message);
					// Zaloguj wyjątek!
				}
			}

			return View("Index", loginDto);
		}


        // --- Inne akcje Home Controllera (jeśli istnieją) ---
        // public ActionResult About() { ... }
        // public ActionResult Contact() { ... }

        // Optional: Akcja błędu, jeśli nie masz jej w Shared
        // public ActionResult Error()
        // {
        //     return View();
        // }
        public ActionResult Logout()
        {
            // Wyloguj użytkownika
            FormsAuthentication.SignOut();

            // Wyczyść sesję
            Session.Remove("LoggedInEmployee");
            Session.Clear();
            Session.Abandon();

            // Unieważnij cache
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetNoStore();

            // Wyczyść również cookies autentykacji
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-20);
            }

            if (Request.Cookies["AuthToken"] != null)
            {
                Response.Cookies["AuthToken"].Value = string.Empty;
                Response.Cookies["AuthToken"].Expires = DateTime.Now.AddMonths(-20);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}