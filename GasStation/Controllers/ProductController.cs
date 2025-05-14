using System;
using System.Collections.Generic;
using System.Web.Mvc;
using GasStation.Services;
using GasStation.DTO;
using GasStation.Models;
using System.Linq;

namespace GasStation.Controllers
{
	// Kontroler do zarządzania produktami (innymi niż paliwa)
	public class ProductController : Controller
	{
		// Prywatne pole do przechowywania instancji serwisu ProductService
		private readonly ProductService _productService;

		// Konstruktor, do którego kontener DI wstrzyknie instancję ProductService
		public ProductController(ProductService productService)
		{
			_productService = productService;
		}

		// GET: Product
		// Akcja wyświetlająca listę wszystkich produktów
		public ActionResult Index()
		{
			try
			{
				// Wywołaj metodę serwisu, aby pobrać listę produktów
				List<ProductDTO> products = _productService.GetAllProducts();

				// Przekaż listę produktów do widoku
				return View(products);
			}
			catch (Exception ex)
			{
				// Obsługa błędów
				ViewBag.ErrorMessage = "Wystąpił błąd podczas pobierania listy produktów: " + ex.Message;
				return View("Error"); // Zakładając, że masz widok Error.cshtml
			}
		}

		// Nowa akcja zwracająca wszystkie produkty jako JSON dla skryptu JavaScript
		[HttpGet]
		public JsonResult GetAllProductsJson()
		{
			try
			{
				var products = _productService.GetAllProducts();
				return Json(products, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { error = true, message = "Wystąpił błąd podczas pobierania produktów: " + ex.Message },
					JsonRequestBehavior.AllowGet);
			}
		}

		// GET: Product/Details/5 (gdzie 5 to ProductId)
		// Akcja wyświetlająca szczegóły pojedynczego produktu
		public ActionResult Details(int? id)
		{
			if (id == null || id == 0) // Zakładając, że ProductId > 0
			{
				// Jeśli ID nie zostało podane, zwróc błąd 400 Bad Request
				return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
			}

			try
			{
				// Twój serwis nie ma publicznej metody GetProductById(int id).
				// Na potrzeby tego kontrolera będziemy musieli albo:
				// 1. Dodać taką metodę do ProductService (zalecane).
				// 2. Użyć GetProductByName, jeśli nazwy są unikalne i używane w URL (mniej typowe dla ID).
				// 3. Dostać się do kontekstu bazy danych bezpośrednio (niezalecane w kontrolerze).

				// Zakładając, że dodasz metodę GetProductById do serwisu:
				// ProductDTO product = _productService.GetProductById(id.Value);

				// Alternatywnie, jeśli GetProductByName jest jedyną opcją wyszukiwania w serwisie:
				// (Wymagałoby zmiany routingu i sposobu przekazywania ID/nazwy)
				// string productName = "Nazwa Produktu"; // Jak uzyskać nazwę? Zazwyczaj używa się ID.
				// ProductDTO product = _productService.GetProductByName(productName);

				// Na potrzeby tego przykładu, zasymulujemy pobranie po ID, zakładając, że serwis zostanie rozszerzony
				ProductDTO product = _productService.GetAllProducts().FirstOrDefault(p => p.ProductId == id.Value);


				if (product == null)
				{
					// Jeśli produkt o podanym ID nie został znaleziony, zwróć błąd 404 Not Found
					return HttpNotFound();
				}

				// Przekaż obiekt produktu do widoku
				return View(product);
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"Wystąpił błąd podczas pobierania szczegółów produktu o ID {id}: " + ex.Message;
				return View("Error");
			}
		}

		// GET: Product/Create
		// Akcja wyświetlająca formularz do dodawania nowego produktu
		public ActionResult Create()
		{
			// Zwróć widok z formularzem. Używamy ProductDTO, ponieważ zawiera atrybuty walidacyjne i jest używane w serwisie AddProduct
			return View(new ProductDTO());
		}

		// POST: Product/Create
		// Akcja obsługująca wysłanie formularza dodawania nowego produktu
		[HttpPost]
		[ValidateAntiForgeryToken] // Ważne dla bezpieczeństwa - ochrona przed CSRF
		public ActionResult Create(ProductDTO productDto) // Przyjmujemy ProductDTO z danymi formularza
		{
			// Sprawdź, czy dane przesłane w formularzu są poprawnie zmapowane i walidowane
			if (ModelState.IsValid)
			{
				try
				{
					// Wywołaj metodę serwisu, aby dodać nowy produkt
					// Metoda AddProduct zwraca ProductDTO z ustawionym ProductId
					ProductDTO createdProduct = _productService.AddProduct(productDto);

					// Po pomyślnym dodaniu, przekieruj użytkownika do listy produktów lub szczegółów nowego produktu
					return RedirectToAction("Details", new { id = createdProduct.ProductId }); // Przekieruj do szczegółów nowo dodanego produktu
																							   // return RedirectToAction("Index"); // Alternatywnie, przekieruj do listy
				}
				// Możesz dodać bardziej szczegółowe bloki catch dla Twoich własnych wyjątków biznesowych
				// catch (BusinessLogicException ex)
				// {
				//     ModelState.AddModelError("", ex.Message); // Dodaj błąd do ModelState
				//     // Wróć do widoku formularza z danymi i błędami
				//     return View(productDto);
				// }
				catch (Exception ex)
				{
					ModelState.AddModelError("", "Wystąpił błąd podczas dodawania produktu: " + ex.Message);
					return View(productDto);
				}
			}

			// Jeśli Model state nie jest poprawny, wróć do widoku formularza z danymi
			// wprowadzonymi przez użytkownika i informacjami o błędach walidacji
			return View(productDto);
		}

		// GET: Product/Edit/5 (gdzie 5 to ProductId)
		// Akcja wyświetlająca formularz do edycji danych produktu (głównie ceny, zgodnie z serwisem)
		public ActionResult Edit(int? id)
		{
			if (id == null || id == 0)
			{
				return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
			}

			try
			{
				// Pobierz dane produktu do edycji.
				// Zakładając, że dodasz metodę GetProductById do serwisu:
				// ProductDTO product = _productService.GetProductById(id.Value);

				// Zasymulujemy pobranie po ID:
				ProductDTO product = _productService.GetAllProducts().FirstOrDefault(p => p.ProductId == id.Value);


				if (product == null)
				{
					return HttpNotFound();
				}

				// Przekaż dane produktu do widoku formularza edycji
				return View(product);
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"Wystąpił błąd podczas ładowania danych produktu o ID {id} do edycji: " + ex.Message;
				return View("Error");
			}
		}

		// POST: Product/Edit/5
		// Akcja obsługująca wysłanie formularza edycji danych produktu (głównie ceny)
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(ProductDTO productDto) // Przyjmujemy ProductDTO z zaktualizowanymi danymi (ID, Nazwa, Cena)
		{
			// Sprawdź, czy ID produktu jest podane (powinno być w ukrytym polu formularza)
			if (productDto.ProductId == 0)
			{
				ModelState.AddModelError("", "ID produktu jest wymagane do edycji.");
				return View(productDto);
			}

			// Walidacja Model State dla pól Name i Price z ProductDTO
			// Note: Twoja metoda serwisu UpdateProductPrice przyjmuje ProductDTO (dla ID) i decimal newPrice.
			// Ten kontroler przyjmuje ProductDTO z formularza, które zawiera ID, Name i Price.
			// Będziemy musieli wywołać serwis przekazując productDto (dla ID) i productDto.Price (jako newPrice).
			// Pole Name z formularza zostanie zignorowane przez metodę serwisu UpdateProductPrice.
			// Jeśli chcesz edytować nazwę, musiałbyś zmodyfikować serwis.

			if (ModelState.IsValid) // Sprawdza walidację dla Name i Price
			{
				try
				{
					// Wywołaj metodę serwisu, aby zaktualizować cenę produktu
					// Przekazujemy productDto (zawierające ID) i nową cenę z productDto.Price
					_productService.UpdateProductPrice(productDto, productDto.Price);

					// Po pomyślnej edycji, przekieruj do szczegółów produktu
					return RedirectToAction("Details", new { id = productDto.ProductId });
				}
				// Obsługa własnych wyjątków biznesowych (np. ArgumentException z serwisu)
				catch (ArgumentException ex) // Łapiemy specyficzny wyjątek z serwisu
				{
					ModelState.AddModelError("", ex.Message); // Dodaj komunikat o błędzie walidacji ceny
					return View(productDto); // Wróć do widoku formularza z danymi i błędami
				}
				// catch (BusinessLogicException ex)
				// {
				//     ModelState.AddModelError("", ex.Message);
				//     return View(productDto);
				// }
				catch (Exception ex)
				{
					ModelState.AddModelError("", "Wystąpił błąd podczas aktualizacji produktu: " + ex.Message);
					return View(productDto);
				}
			}

			// Jeśli Model state nie jest poprawny, wróć do widoku formularza z danymi i błędami walidacji
			return View(productDto);
		}

		// POST: Product/UpdateProductPrice - endpoint JSON do aktualizacji ceny produktu
		[HttpPost]
		public JsonResult UpdateProductPrice(int productId, decimal newPrice)
		{
			try
			{
				var productDto = new ProductDTO { ProductId = productId };
				var result = _productService.UpdateProductPrice(productDto, newPrice);

				if (result == null)
				{
					return Json(new { success = false, message = "Produkt nie został znaleziony" });
				}

				return Json(new { success = true, message = "Cena została zaktualizowana", product = result });
			}
			catch (ArgumentException ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
			catch (Exception)
			{
				return Json(new { success = false, message = "Wystąpił błąd podczas aktualizacji ceny" });
			}
		}

		// GET: Product/Delete/5 (gdzie 5 to ProductId)
		// Akcja wyświetlająca stronę z potwierdzeniem usunięcia produktu
		public ActionResult Delete(int? id)
		{
			if (id == null || id == 0)
			{
				return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
			}

			try
			{
				// Pobierz produkt, który chcemy usunąć, aby wyświetlić jego dane na stronie potwierdzenia
				// Zakładając, że dodasz metodę GetProductById do serwisu:
				// ProductDTO product = _productService.GetProductById(id.Value);

				// Zasymulujemy pobranie po ID:
				ProductDTO product = _productService.GetAllProducts().FirstOrDefault(p => p.ProductId == id.Value);


				if (product == null)
				{
					return HttpNotFound();
				}

				// Przekaż dane produktu do widoku potwierdzenia
				return View(product);
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"Wystąpił błąd podczas przygotowywania do usunięcia produktu o ID {id}: " + ex.Message;
				return View("Error");
			}
		}

		// POST: Product/Delete/5
		// Akcja obsługująca potwierdzenie usunięcia produktu
		[HttpPost, ActionName("Delete")] // Określamy nazwę akcji jako "Delete" dla routingu POST
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id) // Przyjmujemy ID produktu do usunięcia
		{
			if (id == 0)
			{
				return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
			}

			try
			{
				// Twoja metoda RemoveProduct przyjmuje ProductDTO.
				// Tworzymy tymczasowe DTO tylko z ID, aby przekazać je do serwisu.
				ProductDTO productToDelete = new ProductDTO { ProductId = id };

				// Wywołaj metodę serwisu, aby usunąć produkt
				_productService.RemoveProduct(productToDelete);

				// Po pomyślnym usunięciu, przekieruj użytkownika do listy produktów
				return RedirectToAction("Index");
			}
			// Możesz dodać bardziej szczegółowe bloki catch dla wyjątków, np.
			// jeśli produktu nie można usunąć z powodu powiązań w bazie danych.
			// catch (BusinessLogicException ex)
			// {
			//     ViewBag.ErrorMessage = "Nie można usunąć produktu: " + ex.Message;
			//     // Możesz wrócić do strony potwierdzenia usunięcia z komunikatem o błędzie
			//     // Zasymulujemy pobranie po ID, jeśli serwis zostanie rozszerzony:
			//     // ProductDTO product = _productService.GetProductById(id);
			//     ProductDTO product = new ProductDTO { ProductId = id }; // Minimalne DTO do wyświetlenia
			//     return View("Delete", product); // Wróć do widoku Delete
			// }
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"Wystąpił błąd podczas usuwania produktu o ID {id}: " + ex.Message;
				// Możesz wrócić do strony potwierdzenia usunięcia z komunikatem o błędzie
				// Zasymulujemy pobranie po ID, jeśli serwis zostanie rozszerzony:
				// ProductDTO product = _productService.GetProductById(id);
				ProductDTO product = new ProductDTO { ProductId = id }; // Minimalne DTO do wyświetlenia
				return View("Delete", product); // Wróć do widoku Delete
			}
		}

		// Dodatkowa akcja JSON do usuwania produktów za pomocą AJAX
		[HttpPost]
		public JsonResult RemoveProduct(int productId)
		{
			try
			{
				var productDto = new ProductDTO { ProductId = productId };
				_productService.RemoveProduct(productDto);
				return Json(new { success = true, message = "Produkt został usunięty" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Wystąpił błąd podczas usuwania produktu: " + ex.Message });
			}
		}

		// Dodatkowa akcja JSON do dodawania produktów za pomocą AJAX
		[HttpPost]
		public JsonResult AddProduct(ProductDTO productDTO)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(productDTO.Name) || productDTO.Price <= 0)
				{
					return Json(new { success = false, message = "Nazwa produktu i poprawna cena są wymagane" });
				}

				var result = _productService.AddProduct(productDTO);
				return Json(new { success = true, message = "Produkt został dodany", product = result });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Wystąpił błąd podczas dodawania produktu: " + ex.Message });
			}
		}
	}
}