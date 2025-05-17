using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

// Choose ONE MVC framework and stick with it
// Option 1: Using ASP.NET Core MVC (recommended for new projects)


using System.Web.Mvc;

using GasStation.Services;
using GasStation.DTO;
using GasStation.Models;

namespace GasStation.Controllers
{
	// Kontroler do zarządzania zamówieniami (transakcjami)
	public class OrderController : Controller
	{
		// Prywatne pola do przechowywania instancji serwisów
		private readonly OrderService _orderService;
		private readonly CustomerService _customerService; // Potrzebny do listy klientów w formularzu Create
		private readonly EmployeeService _employeeService; // Potrzebny do listy pracowników w formularzu Create
														   // Możesz potrzebować FuelService i ProductService do pobrania listy produktów/paliw do wyboru w formularzu Create

		// Konstruktor, do którego kontener DI wstrzyknie instancje serwisów
		public OrderController(
			OrderService orderService,
			CustomerService customerService,
			EmployeeService employeeService) // Dodaj inne serwisy, jeśli potrzebne w konstruktorze
		{
			_orderService = orderService;
			_customerService = customerService;
			_employeeService = employeeService;
			// Zainicjuj inne serwisy...
		}

		// GET: Order
		// Akcja wyświetlająca listę wszystkich zamówień
		public ActionResult Index()
		{
			try
			{
				// Wywołaj metodę serwisu, aby pobrać listę zamówień
				List<OrderDTO> orders = _orderService.GetAllOrders();

				// Przekaż listę zamówień do widoku
				return View(orders);
			}
			catch (Exception ex)
			{
				// Obsługa błędów
				ViewBag.ErrorMessage = "Wystąpił błąd podczas pobierania listy zamówień: " + ex.Message;
				return View("Error"); // Zakładając, że masz widok Error.cshtml
			}
		}

		// GET: Order/Details/5 (gdzie 5 to OrderId)
		// Akcja wyświetlająca szczegóły pojedynczego zamówienia
		public ActionResult Details(int? id)
		{
			if (id == null)
			{
				// Jeśli ID nie zostało podane, zwróc błąd 400 Bad Request
				return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
			}

			try
			{
				// Wywołaj metodę serwisu, aby pobrać zamówienie po ID
				OrderDTO order = _orderService.GetOrderById(id.Value);

				if (order == null)
				{
					// Jeśli zamówienie o podanym ID nie zostało znalezione, zwróć błąd 404 Not Found
					return HttpNotFound();
				}

				// Przekaż obiekt zamówienia do widoku
				return View(order);
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"Wystąpił błąd podczas pobierania szczegółów zamówienia o ID {id}: " + ex.Message;
				return View("Error");
			}
		}

        // GET: Order/Create
        // Akcja wyświetlająca formularz do tworzenia nowego zamówienia
        public ActionResult Create()
        {
            try
            {
                // Pobierz dane z serwisu
                var customers = _customerService.GetAllCustomers() ?? new List<CustomerDTO>();
                var employees = _employeeService.GetAllEmployees() ?? new List<EmployeeDTO>();

                // Utwórz SelectList z właściwymi właściwościami
                ViewBag.CustomerList = new SelectList(customers, "Nip", "CompanyName");
                ViewBag.EmployeeList = employees.Select(e => new SelectListItem
                {
                    Value = e.Pesel,
                    Text = $"{e.Name} {e.Surname}" // Łączymy Name i Surname
                }).ToList();

                return View(new CreateOrderDTO { Items = new List<OrderItemDTO>() });
            }
            catch (Exception ex)
            {
                // Logowanie błędu

                // Zwróć puste listy w przypadku błędu
                ViewBag.CustomerList = new SelectList(new List<CustomerDTO>(), "Nip", "CompanyName");
                ViewBag.EmployeeList = new List<SelectListItem>();

                ViewBag.ErrorMessage = "Wystąpił błąd podczas ładowania danych";
                return View(new CreateOrderDTO { Items = new List<OrderItemDTO>() });
            }
        }

        [HttpGet]
        public JsonResult GetEmployeesAndCustomers()
        {
            try
            {
                var customers = _customerService.GetAllCustomers()
                    .Select(c => new { value = c.Nip, text = c.CompanyName })
                    .ToList();

                var employees = _employeeService.GetAllEmployees()
                    .Select(e => new { value = e.Pesel, text = $"{e.Name} {e.Surname}" })
                    .ToList();

                return Json(new { customers, employees });
            }
            catch (Exception ex)
			{ 
                return Json(new { customers = new List<object>(), employees = new List<object>() });
            }
        }


        // POST: Order/Create
        // Akcja obsługująca wysłanie formularza tworzenia nowego zamówienia
        [HttpPost]
        public ActionResult Create(CreateOrderDTO orderDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                    return Json(new
                    {
                        success = false,
                        message = "Wystąpiły błędy walidacji",
                        errors = errors
                    });
                }

                var order = _orderService.CreateOrder(orderDto);

                return Json(new
                {
                    success = true,
                    message = "Zamówienie zostało pomyślnie złożone",
                    orderId = order.OrderId
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // GET: Order/Delete/5 (gdzie 5 to OrderId)
        // Akcja wyświetlająca stronę z potwierdzeniem usunięcia zamówienia
        public ActionResult Delete(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
			}

			try
			{
				// Pobierz zamówienie, które chcemy usunąć, aby wyświetlić jego dane na stronie potwierdzenia
				OrderDTO order = _orderService.GetOrderById(id.Value);

				if (order == null)
				{
					return HttpNotFound();
				}

				// Przekaż dane zamówienia do widoku potwierdzenia
				return View(order);
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"Wystąpił błąd podczas przygotowywania do usunięcia zamówienia o ID {id}: " + ex.Message;
				return View("Error");
			}
		}

		// POST: Order/Delete/5
		// Akcja obsługująca potwierdzenie usunięcia zamówienia
		[HttpPost, ActionName("Delete")] // Określamy nazwę akcji jako "Delete" dla routingu POST
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id) // Przyjmujemy ID zamówienia do usunięcia
		{
			if (id == 0) // Zakładając, że ID > 0
			{
				return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
			}

			try
			{
				// Wywołaj metodę serwisu, aby usunąć zamówienie
				_orderService.DeleteOrder(id);

				// Po pomyślnym usunięciu, przekieruj użytkownika do listy zamówień
				return RedirectToAction("Index");
			}
			// Obsługa własnych wyjątków biznesowych (np. gdy zamówienie nie może być usunięte)
			// catch (BusinessLogicException ex)
			// {
			//     ViewBag.ErrorMessage = "Nie można usunąć zamówienia: " + ex.Message;
			//     // Możesz wrócić do strony potwierdzenia usunięcia z komunikatem o błędzie
			//     OrderDTO order = _orderService.GetOrderById(id); // Spróbujmy ponownie pobrać zamówienie
			//     return View("Delete", order); // Wróć do widoku Delete
			// }
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"Wystąpił błąd podczas usuwania zamówienia o ID {id}: " + ex.Message;
				// Możesz wrócić do strony potwierdzenia usunięcia z komunikatem o błędzie
				OrderDTO order = _orderService.GetOrderById(id); // Spróbujmy ponownie pobrać zamówienie
				return View("Delete", order); // Wróć do widoku Delete
			}
		}

		// --- Akcje pomocnicze dla AJAX/JavaScript (opcjonalnie) ---
		// Jeśli formularz tworzenia zamówienia będzie dynamiczny (dodawanie/usuwanie pozycji),
		// możesz potrzebować akcji, które zwracają częściowe widoki lub dane JSON.
		// Przykład akcji zwracającej listę paliw/produktów w formacie JSON:
		// public JsonResult GetItemsForOrder()
		// {
		//     var fuels = _fuelService.GetAllFuels().Select(f => new { id = f.FuelId, name = f.FuelName, isFuel = true });
		//     var products = _productService.GetAllProducts().Select(p => new { id = p.ProductId, name = p.ProductName, isFuel = false });
		//     var items = fuels.Concat(products).ToList();
		//     return Json(items, JsonRequestBehavior.AllowGet);
		// }


		// --- Potrzebne Views ---
		// Dla każdej akcji zwracającej View(), potrzebujesz odpowiadającego pliku .cshtml w folderze Views/Order/
		// Views/Order/Index.cshtml (Model: List<OrderDTO>) - wyświetla listę zamówień
		// Views/Order/Details.cshtml (Model: OrderDTO) - wyświetla szczegóły zamówienia (w tym pozycje OrderSpecificationDTO)
		// Views/Order/Create.cshtml (Model: CreateOrderDTO) - dla formularza tworzenia zamówienia (potrzebuje też ViewBag.CustomerList, ViewBag.EmployeeList, listy paliw/produktów)
		// Views/Order/Delete.cshtml (Model: OrderDTO) - dla potwierdzenia usunięcia
		// Views/Shared/Error.cshtml (opcjonalnie, dla ogólnych błędów)

		// Uwaga: Tworzenie formularza dla CreateOrderDTO z dynamiczną listą OrderItemDTO
		// w standardowym Razor View może być skomplikowane. Często wymaga użycia
		// Partial Views i JavaScriptu do dynamicznego dodawania/usuwania pól formularza
		// dla poszczególnych pozycji zamówienia. Nazwy pól formularza dla listy
		// powinny być w formacie indeksowanym, np. Items[0].ItemId, Items[0].Quantity, Items[0].IsFuel,
		// Items[1].ItemId, Items[1].Quantity, Items[1].IsFuel, itd., aby model binder
		// poprawnie zmapował je na List<OrderItemDTO>.
	}
}
