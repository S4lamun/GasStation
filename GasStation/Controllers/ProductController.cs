using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc; using GasStation.Services; using GasStation.DTO; using GasStation.Models; using System.Net; 
namespace GasStation.Controllers
{
        public class ProductController : Controller
    {
                private readonly ProductService _productService;

                public ProductController(ProductService productService)
        {
            _productService = productService;
        }

                        public ActionResult Index()
        {
            try
            {
                                List<ProductDTO> products = _productService.GetAllProducts();

                                return View(products);
            }
            catch (Exception ex)
            {
                                System.Diagnostics.Trace.TraceError($"Błąd podczas pobierania listy produktów: {ex.Message}");
                ViewBag.ErrorMessage = "Wystąpił błąd podczas pobierania listy produktów: " + ex.Message;
                return View("Error");             }
        }

                        [HttpGet]
        public JsonResult GetAllProductsJson()
        {
            try
            {
                var products = _productService.GetAllProducts();

                                                                /*
                var select2Data = products.Select(p => new {
                    id = p.ProductId,                     text = $"{p.Name} ({p.Price.ToString("N2", new System.Globalization.CultureInfo("pl-PL"))} zł)"                 }).ToList();
                return Json(select2Data, JsonRequestBehavior.AllowGet);
                */

                                return Json(products, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Błąd w GetAllProductsJson: {ex.Message}");
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;                 return Json(new { success = false, message = "Wystąpił błąd serwera podczas pobierania produktów: " + ex.Message },
                    JsonRequestBehavior.AllowGet);
            }
        }

                        public ActionResult Details(int? id)
        {
            if (id == null || id <= 0)             {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);             }

            try
            {
                                                                ProductDTO product = _productService.GetAllProducts().FirstOrDefault(p => p.ProductId == id.Value);

                if (product == null)
                {
                    return HttpNotFound();                 }

                                return View(product);             }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Błąd podczas pobierania szczegółów produktu o ID {id}: {ex.Message}");
                ViewBag.ErrorMessage = $"Wystąpił błąd podczas pobierania szczegółów produktu o ID {id}: " + ex.Message;
                return View("Error");
            }
        }

                        public ActionResult Create()
        {
                        return View(new ProductDTO());         }

                                [HttpPost]
        [ValidateAntiForgeryToken]         public ActionResult Create(ProductDTO productDto)         {
                                    bool isAjax = Request.IsAjaxRequest();

                        if (ModelState.IsValid)
            {
                try
                {
                                                            ProductDTO createdProduct = _productService.AddProduct(productDto);

                    if (isAjax)
                    {
                                                return Json(new { success = true, message = "Produkt został dodany pomyślnie!", product = createdProduct });
                    }
                    else
                    {
                                                                        TempData["SuccessMessage"] = "Produkt został pomyślnie dodany.";                         return RedirectToAction("Cashier_view", "Cashier");                                                                     }
                }
                                                                                                                                                                                                                                                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError($"Błąd podczas dodawania produktu: {ex.Message}");
                    ModelState.AddModelError("", "Wystąpił błąd podczas dodawania produktu: " + ex.Message);

                    if (isAjax)
                    {
                                                Response.StatusCode = (int)HttpStatusCode.InternalServerError;                                                 var errorList = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        ).Where(m => m.Value.Any()).ToList();
                        return Json(new { success = false, message = "Wystąpił błąd serwera podczas dodawania produktu: " + ex.Message, errors = errorList });
                    }
                    else
                    {
                                                return View(productDto);
                    }
                }
            }
            else             {
                if (isAjax)
                {
                                        Response.StatusCode = (int)HttpStatusCode.BadRequest;                     var errorList = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    ).Where(m => m.Value.Any()).ToList();

                    return Json(new { success = false, message = "Błędy walidacji formularza produktu.", isValidationError = true, errors = errorList });
                }
                else
                {
                                        return View(productDto);
                }
            }
        }

                        public ActionResult Edit(int? id)
        {
            if (id == null || id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                                ProductDTO product = _productService.GetAllProducts().FirstOrDefault(p => p.ProductId == id.Value);

                if (product == null)
                {
                    return HttpNotFound();
                }

                                return View(product);             }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Błąd podczas ładowania danych produktu o ID {id} do edycji: {ex.Message}");
                ViewBag.ErrorMessage = $"Wystąpił błąd podczas ładowania danych produktu o ID {id} do edycji: " + ex.Message;
                return View("Error");
            }
        }

                        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductDTO productDto)         {
            if (productDto.ProductId <= 0)
            {
                ModelState.AddModelError("", "ID produktu jest wymagane do edycji.");
                return View(productDto);
            }

                        if (ModelState.IsValid)
            {
                try
                {
                                                                                                                        _productService.UpdateProductPrice(productDto, productDto.Price);

                                        return RedirectToAction("Details", new { id = productDto.ProductId });
                }
                                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);                     return View(productDto);                 }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError($"Błąd podczas aktualizacji produktu: {ex.Message}");
                    ModelState.AddModelError("", "Wystąpił błąd podczas aktualizacji produktu: " + ex.Message);
                    return View(productDto);
                }
            }

                        return View(productDto);
        }

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
                    Response.StatusCode = (int)HttpStatusCode.NotFound;                     return Json(new { success = false, message = "Produkt nie został znaleziony" });
                }

                return Json(new { success = true, message = "Cena została zaktualizowana", product = result });
            }
            catch (ArgumentException ex)             {
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


                        public ActionResult Delete(int? id)
        {
            if (id == null || id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                                                ProductDTO product = _productService.GetAllProducts().FirstOrDefault(p => p.ProductId == id.Value);

                if (product == null)
                {
                    return HttpNotFound();
                }

                                return View(product);             }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Błąd podczas przygotowywania do usunięcia produktu o ID {id}: {ex.Message}");
                ViewBag.ErrorMessage = $"Wystąpił błąd podczas przygotowywania do usunięcia produktu o ID {id}: " + ex.Message;
                return View("Error");
            }
        }

                        [HttpPost, ActionName("Delete")]         [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)         {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                                ProductDTO productToDelete = new ProductDTO { ProductId = id };

                                _productService.RemoveProduct(productToDelete);

                                TempData["SuccessMessage"] = "Produkt został pomyślnie usunięty.";                 return RedirectToAction("Index");             }
                                                                                                            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Wystąpił błąd podczas usuwania produktu o ID {id}: {ex.Message}");
                ViewBag.ErrorMessage = $"Wystąpił błąd podczas usuwania produktu o ID {id}: " + ex.Message;
                                                ProductDTO product = _productService.GetAllProducts().FirstOrDefault(p => p.ProductId == id);                 return View("Delete", product);             }
        }
    }
}