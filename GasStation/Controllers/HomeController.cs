using System;
using System.Web.Mvc;
using GasStation.Services; // Pamiętaj o dodaniu using do Twoich serwisów
using GasStation.DTO; // Pamiętaj o dodaniu using do Twoich DTOs
// Jeśli masz własne wyjątki biznesowe, dodaj using
// using GasStation.Exceptions; // example

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
        {
            // Zwracamy widok Index.cshtml, przekazując pusty model DTO do formularza
            return View(new EmployeeLoginDTO());
        }

        // POST: /Home/Login
        // Akcja obsługująca wysłanie formularza logowania
        [HttpPost] // Jawnie wskazujemy, że to akcja POST
        [ValidateAntiForgeryToken] // Ochrona przed CSRF
        public ActionResult Login(EmployeeLoginDTO loginDto) // Model binder wypełni ten obiekt danymi z formularza
        {
            // 1. Sprawdź, czy dane z formularza są poprawne (wg atrybutów w DTO)
            if (ModelState.IsValid)
            {
                try
                {
                    // 2. Wywołaj serwis do uwierzytelnienia pracownika
                    EmployeeDTO authenticatedEmployee = _employeeService.Authenticate(loginDto);

                    // 3. Sprawdź wynik uwierzytelnienia
                    if (authenticatedEmployee != null)
                    {
                        // Uwierzytelnienie pomyślne!
                        // TODO: Zaimplementuj prawdziwy mechanizm autoryzacji/sesji
                        // (np. Forms Authentication, ASP.NET Identity).
                        // Na razie przekierowujemy do widoku Kasjera.

                        // Przykładowe przekierowanie po udanym logowaniu
                        // Zakładamy, że masz kontroler CashierController i akcję Cashier_view
                        return RedirectToAction("Cashier_view", "Cashier");
                    }
                    else
                    {
                        // Uwierzytelnienie nie powiodło się
                        // Dodaj ogólny komunikat błędu do ModelState
                        ModelState.AddModelError("", "Niepoprawna próba logowania. Sprawdź login i hasło.");
                        // Ogólny komunikat jest lepszy dla bezpieczeństwa (nie zdradza, czy login istnieje)
                    }
                }
                // Możesz dodać obsługę własnych wyjątków biznesowych z serwisu, jeśli je rzucasz
                // catch (BusinessLogicException ex)
                // {
                //     ModelState.AddModelError("", ex.Message);
                // }
                catch (Exception ex)
                {
                    // Obsługa innych nieoczekiwanych błędów
                    ModelState.AddModelError("", "Wystąpił nieoczekiwany błąd podczas logowania: " + ex.Message);
                    // Zaloguj wyjątek w rzeczywistej aplikacji!
                }
            }

            // Jeśli ModelState nie jest poprawny lub uwierzytelnienie się nie powiodło,
            // wróć do widoku Index (który wyświetla formularz logowania)
            // z danymi wprowadzonymi przez użytkownika (bez hasła) i komunikatami błędów.
            return View("Index", loginDto); // Jawnie wskazujemy widok Index
        }


        // --- Inne akcje Home Controllera (jeśli istnieją) ---
        // public ActionResult About() { ... }
        // public ActionResult Contact() { ... }

        // Optional: Akcja błędu, jeśli nie masz jej w Shared
        // public ActionResult Error()
        // {
        //     return View();
        // }
    }
}