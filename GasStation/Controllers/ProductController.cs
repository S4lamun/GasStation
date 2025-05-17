// W pliku ProductController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc; // Upewnij się, że masz ten using
using GasStation.Services; // Upewnij się, że masz using do serwisu produktów
using GasStation.DTO; // Upewnij się, że masz using do DTO produktów
using GasStation.Models; // Upewnij się, że masz using do modeli bazy danych
using System.Net; // Potrzebne dla HttpStatusCodeResult

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
        // Akcja wyświetlająca listę wszystkich produktów (jeśli masz taką stronę)
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
                System.Diagnostics.Trace.TraceError($"Błąd podczas pobierania listy produktów: {ex.Message}");
                ViewBag.ErrorMessage = "Wystąpił błąd podczas pobierania listy produktów: " + ex.Message;
                return View("Error"); // Zakładając, że masz widok Error.cshtml
            }
        }

        // GET: Product/GetAllProductsJson
        // Akcja zwracająca wszystkie produkty jako JSON dla skryptu JavaScript (np. dla Select2 lub listy)
        [HttpGet]
        public JsonResult GetAllProductsJson()
        {
            try
            {
                var products = _productService.GetAllProducts();

                // Formatowanie danych dla Select2 lub ogólnej listy JS
                // Zakładamy, że chcesz zwrócić ProductDTO, ale jeśli używasz Select2,
                // możesz potrzebować zmapować do formatu { id: ..., text: ... }
                /*
                var select2Data = products.Select(p => new {
                    id = p.ProductId, // Użyj ProductId jako ID
                    text = $"{p.Name} ({p.Price.ToString("N2", new System.Globalization.CultureInfo("pl-PL"))} zł)" // Format tekstu
                }).ToList();
                return Json(select2Data, JsonRequestBehavior.AllowGet);
                */

                // Jeśli po prostu zwracasz listę ProductDTO:
                return Json(products, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Błąd w GetAllProductsJson: {ex.Message}");
                Response.StatusCode = (int)HttpStatusCode.InternalServerError; // Zwróć status 500
                return Json(new { success = false, message = "Wystąpił błąd serwera podczas pobierania produktów: " + ex.Message },
                    JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Product/Details/5 (gdzie 5 to ProductId)
        // Akcja wyświetlająca szczegóły pojedynczego produktu (potrzebuje widoku Views/Product/Details.cshtml)
        public ActionResult Details(int? id)
        {
            if (id == null || id <= 0) // Zakładając, że ProductId > 0
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest); // Zwróć błąd 400
            }

            try
            {
                // Twoja metoda serwisu GetProductByName nie jest idealna do wyszukiwania po ID.
                // Zaleca się dodanie metody GetProductById(int id) do ProductService.
                // Na potrzeby tego przykładu, zasymulujemy pobranie po ID:
                ProductDTO product = _productService.GetAllProducts().FirstOrDefault(p => p.ProductId == id.Value);

                if (product == null)
                {
                    return HttpNotFound(); // Zwróć błąd 404
                }

                // Przekaż obiekt produktu do widoku
                return View(product); // Ta akcja wymaga istnienia widoku Views/Product/Details.cshtml
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Błąd podczas pobierania szczegółów produktu o ID {id}: {ex.Message}");
                ViewBag.ErrorMessage = $"Wystąpił błąd podczas pobierania szczegółów produktu o ID {id}: " + ex.Message;
                return View("Error");
            }
        }

        // GET: Product/Create
        // Akcja wyświetlająca formularz do dodawania nowego produktu (potrzebuje widoku Views/Product/Create.cshtml)
        public ActionResult Create()
        {
            // Zwróć widok z formularzem. Używamy ProductDTO
            return View(new ProductDTO()); // Ta akcja wymaga istnienia widoku Views/Product/Create.cshtml
        }

        // POST: Product/Create
        // Akcja obsługująca wysłanie formularza dodawania nowego produktu
        // Obsługuje zarówno standardowe przesłania formularza, jak i żądania AJAX z modala
        [HttpPost]
        [ValidateAntiForgeryToken] // Ważne dla bezpieczeństwa - ochrona przed CSRF
        public ActionResult Create(ProductDTO productDto) // Przyjmujemy ProductDTO z danymi formularza
        {
            // Sprawdź, czy żądanie pochodzi z AJAX
            // Request.IsAjaxRequest() sprawdza nagłówek 'X-Requested-With: XMLHttpRequest'
            bool isAjax = Request.IsAjaxRequest();

            // Sprawdź, czy dane przesłane w formularzu są poprawnie zmapowane i walidowane
            if (ModelState.IsValid)
            {
                try
                {
                    // Wywołaj metodę serwisu, aby dodać nowy produkt
                    // Metoda AddProduct zwraca ProductDTO z ustawionym ProductId
                    ProductDTO createdProduct = _productService.AddProduct(productDto);

                    if (isAjax)
                    {
                        // Jeśli żądanie AJAX, zwróć odpowiedź sukcesu w formacie JSON
                        return Json(new { success = true, message = "Produkt został dodany pomyślnie!", product = createdProduct });
                    }
                    else
                    {
                        // Jeśli standardowe przesłanie formularza, przekieruj
                        // Przekieruj do widoku kasjera po dodaniu produktu ze standardowego formularza
                        TempData["SuccessMessage"] = "Produkt został pomyślnie dodany."; // Opcjonalny komunikat sukcesu
                        return RedirectToAction("Cashier_view", "Cashier"); // Przekieruj do akcji Cashier_view w kontrolerze Cashier
                        // return RedirectToAction("Index"); // Alternatywnie, przekieruj do listy produktów
                        // return RedirectToAction("Details", new { id = createdProduct.ProductId }); // Alternatywnie, przekieruj do szczegółów
                    }
                }
                // Możesz dodać bardziej szczegółowe bloki catch dla Twoich własnych wyjątków biznesowych
                // catch (BusinessLogicException ex)
                // {
                //     ModelState.AddModelError("", ex.Message); // Dodaj błąd do ModelState
                //     if (isAjax) {
                //         Response.StatusCode = (int)HttpStatusCode.BadRequest; // Zwróć 400 dla AJAX z błędami walidacji/biznesowymi
                //         // Zbieramy błędy i zwracamy JSON
                //         var errorList = ModelState.ToDictionary(...);
                //         return Json(new { success = false, message = "Błąd biznesowy.", isValidationError = true, errors = errorList });
                //     } else {
                //         // Wróć do widoku formularza z danymi i błędami dla standardowego POST
                //         return View(productDto);
                //     }
                // }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError($"Błąd podczas dodawania produktu: {ex.Message}");
                    ModelState.AddModelError("", "Wystąpił błąd podczas dodawania produktu: " + ex.Message);

                    if (isAjax)
                    {
                        // Jeśli żądanie AJAX, zwróć błąd serwera w formacie JSON
                        Response.StatusCode = (int)HttpStatusCode.InternalServerError; // Zwróć 500
                        // Zbieramy błędy (w tym te dodane do ModelState) i zwracamy JSON
                        var errorList = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        ).Where(m => m.Value.Any()).ToList();
                        return Json(new { success = false, message = "Wystąpił błąd serwera podczas dodawania produktu: " + ex.Message, errors = errorList });
                    }
                    else
                    {
                        // Jeśli standardowe przesłanie formularza, wróć do widoku z błędami
                        return View(productDto);
                    }
                }
            }
            else // Jeśli ModelState.IsValid jest false (błędy walidacji DTO)
            {
                if (isAjax)
                {
                    // Jeśli żądanie AJAX, zwróć błędy walidacji w formacie JSON
                    Response.StatusCode = (int)HttpStatusCode.BadRequest; // Zwróć 400
                    var errorList = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    ).Where(m => m.Value.Any()).ToList();

                    return Json(new { success = false, message = "Błędy walidacji formularza produktu.", isValidationError = true, errors = errorList });
                }
                else
                {
                    // Jeśli standardowe przesłanie formularza, wróć do widoku z błędami walidacji
                    return View(productDto);
                }
            }
        }

        // GET: Product/Edit/5 (potrzebuje widoku Views/Product/Edit.cshtml)
        // Akcja wyświetlająca formularz do edycji danych produktu
        public ActionResult Edit(int? id)
        {
            if (id == null || id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                // Zasymulujemy pobranie po ID:
                ProductDTO product = _productService.GetAllProducts().FirstOrDefault(p => p.ProductId == id.Value);

                if (product == null)
                {
                    return HttpNotFound();
                }

                // Przekaż dane produktu do widoku formularza edycji
                return View(product); // Ta akcja wymaga istnienia widoku Views/Product/Edit.cshtml
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Błąd podczas ładowania danych produktu o ID {id} do edycji: {ex.Message}");
                ViewBag.ErrorMessage = $"Wystąpił błąd podczas ładowania danych produktu o ID {id} do edycji: " + ex.Message;
                return View("Error");
            }
        }

        // POST: Product/Edit/5
        // Akcja obsługująca wysłanie formularza edycji danych produktu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductDTO productDto) // Przyjmujemy ProductDTO z zaktualizowanymi danymi
        {
            if (productDto.ProductId <= 0)
            {
                ModelState.AddModelError("", "ID produktu jest wymagane do edycji.");
                return View(productDto);
            }

            // Walidacja Model State
            if (ModelState.IsValid)
            {
                try
                {
                    // Twoja metoda serwisu UpdateProductPrice przyjmuje ProductDTO (dla ID) i decimal newPrice.
                    // W tym kontrolerze przyjmujemy ProductDTO z formularza, które zawiera ID, Name i Price.
                    // Wywołujemy serwis przekazując productDto (dla ID) i productDto.Price (jako newPrice).
                    // Pole Name z formularza jest ignorowane przez obecną metodę serwisu UpdateProductPrice.
                    // Jeśli chcesz edytować nazwę, musiałbyś zmodyfikować serwis.
                    _productService.UpdateProductPrice(productDto, productDto.Price);

                    // Po pomyślnej edycji, przekieruj do szczegółów produktu
                    return RedirectToAction("Details", new { id = productDto.ProductId });
                }
                // Obsługa własnych wyjątków biznesowych (np. ArgumentException z serwisu)
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message); // Dodaj komunikat o błędzie walidacji ceny
                    return View(productDto); // Wróć do widoku formularza z danymi i błędami
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError($"Błąd podczas aktualizacji produktu: {ex.Message}");
                    ModelState.AddModelError("", "Wystąpił błąd podczas aktualizacji produktu: " + ex.Message);
                    return View(productDto);
                }
            }

            // Jeśli Model state nie jest poprawny, wróć do widoku formularza z danymi i błędami walidacji
            return View(productDto);
        }

        // POST: Product/RemoveProduct - endpoint JSON do usuwania produktów za pomocą AJAX
        [HttpPost]
        public JsonResult RemoveProduct(int productId)
        {
            try
            {
                if (productId <= 0)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(new { success = false, message = "ID produktu jest wymagane do usunięcia." });
                }

                var productDto = new ProductDTO { ProductId = productId };
                _productService.RemoveProduct(productDto);
                return Json(new { success = true, message = "Produkt został usunięty" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Błąd podczas usuwania produktu o ID {productId}: {ex.Message}");
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { success = false, message = "Wystąpił błąd podczas usuwania produktu: " + ex.Message });
            }
        }

        // POST: Product/UpdateProductPrice - endpoint JSON do aktualizacji ceny produktu
        // Ta akcja może być używana przez modal zmiany ceny paliwa, jeśli paliwa są traktowane jako produkty
        // lub przez inną funkcjonalność.
        [HttpPost]
        public JsonResult UpdateProductPrice(int productId, decimal newPrice)
        {
            try
            {
                if (productId <= 0)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(new { success = false, message = "ID produktu jest wymagane do aktualizacji ceny." });
                }
                if (newPrice <= 0)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(new { success = false, message = "Nowa cena musi być dodatnia." });
                }

                var productDto = new ProductDTO { ProductId = productId };
                var result = _productService.UpdateProductPrice(productDto, newPrice);

                if (result == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound; // Zwróć 404 jeśli produkt nie znaleziono
                    return Json(new { success = false, message = "Produkt nie został znaleziony" });
                }

                return Json(new { success = true, message = "Cena została zaktualizowana", product = result });
            }
            catch (ArgumentException ex) // Łapiemy wyjątek z serwisu UpdateProductPrice
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Błąd podczas aktualizacji ceny produktu o ID {productId}: {ex.Message}");
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { success = false, message = "Wystąpił błąd podczas aktualizacji ceny: " + ex.Message });
            }
        }


        // GET: Product/Delete/5 (potrzebuje widoku Views/Product/Delete.cshtml)
        // Akcja wyświetlająca stronę z potwierdzeniem usunięcia produktu
        public ActionResult Delete(int? id)
        {
            if (id == null || id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                // Pobierz produkt, który chcemy usunąć, aby wyświetlić jego dane na stronie potwierdzenia
                // Zasymulujemy pobranie po ID:
                ProductDTO product = _productService.GetAllProducts().FirstOrDefault(p => p.ProductId == id.Value);

                if (product == null)
                {
                    return HttpNotFound();
                }

                // Przekaż dane produktu do widoku potwierdzenia
                return View(product); // Ta akcja wymaga istnienia widoku Views/Product/Delete.cshtml
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Błąd podczas przygotowywania do usunięcia produktu o ID {id}: {ex.Message}");
                ViewBag.ErrorMessage = $"Wystąpił błąd podczas przygotowywania do usunięcia produktu o ID {id}: " + ex.Message;
                return View("Error");
            }
        }

        // POST: Product/Delete/5
        // Akcja obsługująca potwierdzenie usunięcia produktu (ze standardowego formularza potwierdzenia)
        [HttpPost, ActionName("Delete")] // Określamy nazwę akcji jako "Delete" dla routingu POST
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id) // Przyjmujemy ID produktu do usunięcia
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                // Tworzymy tymczasowe DTO tylko z ID, aby przekazać je do serwisu.
                ProductDTO productToDelete = new ProductDTO { ProductId = id };

                // Wywołaj metodę serwisu, aby usunąć produkt
                _productService.RemoveProduct(productToDelete);

                // Po pomyślnym usunięciu, przekieruj użytkownika do listy produktów
                TempData["SuccessMessage"] = "Produkt został pomyślnie usunięty."; // Opcjonalny komunikat sukcesu
                return RedirectToAction("Index"); // Przekieruj do akcji Index (listy produktów)
            }
            // Możesz dodać bardziej szczegółowe bloki catch dla wyjątków biznesowych
            // catch (BusinessLogicException ex)
            // {
            //     ViewBag.ErrorMessage = "Nie można usunąć produktu: " + ex.Message;
            //     // Wróć do strony potwierdzenia usunięcia z komunikatem o błędzie
            //     ProductDTO product = _productService.GetAllProducts().FirstOrDefault(p => p.ProductId == id); // Spróbuj pobrać produkt ponownie
            //     return View("Delete", product); // Wróć do widoku Delete
            // }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Wystąpił błąd podczas usuwania produktu o ID {id}: {ex.Message}");
                ViewBag.ErrorMessage = $"Wystąpił błąd podczas usuwania produktu o ID {id}: " + ex.Message;
                // Wróć do strony potwierdzenia usunięcia z komunikatem o błędzie
                // Zasymulujemy pobranie po ID:
                ProductDTO product = _productService.GetAllProducts().FirstOrDefault(p => p.ProductId == id); // Spróbuj pobrać produkt ponownie
                return View("Delete", product); // Wróć do widoku Delete
            }
        }
    }
}