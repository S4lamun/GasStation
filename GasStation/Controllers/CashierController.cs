using System;
using System.Collections.Generic;
using System.Web.Mvc;
using GasStation.Services; using GasStation.DTO; using GasStation.Models;
using System.Linq;                                       

namespace GasStation.Controllers
{
        public class CashierController : Controller
    {
        private readonly FuelService _fuelService;
        private readonly CustomerService _customerService;         private static readonly Dictionary<int, string> fuelTypesMap = new Dictionary<int, string>
        {
            {1, "Benzyna"},
            {2, "Diesel"},
            {3, "Gaz"}
        };

        public CashierController()
        {
                    }

        public CashierController(FuelService fuelService, CustomerService customerService)
        {
            _fuelService = fuelService;
            _customerService = customerService;
        }

        public ActionResult Cashier_view()
        {
            if (Session["LoggedInEmployee"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var employee = Session["LoggedInEmployee"] as EmployeeDTO;
            ViewBag.CurrentCashierFullName = $"{employee.Name} {employee.Surname}";
            ViewBag.CurrentCashierPesel = employee.Pesel;

            try
            {
                                var fuelPrices = _fuelService.GetAllCurrentPrices().ToDictionary(f => f.FuelId, f => f.Price);

                var items = Enumerable.Range(1, 6).Select(i =>
                {
                    var fuelId = (i % 3) + 1;                     return new RefuelingEntryDTO
                    {
                        RefuelingEntryId = i,
                        Amount = 10.0m + i * 2,
                        OrderId = 100 + i,
                        FuelId = fuelId,
                        FuelName = fuelTypesMap.ContainsKey(fuelId) ? fuelTypesMap[fuelId] : $"Paliwo {fuelId}",
                        PriceAtSale = fuelPrices.ContainsKey(fuelId) ? fuelPrices[fuelId] : 5.50m
                    };
                }).ToList();

                ViewBag.Items = new List<RefuelingEntryDTO>(items);
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Wystąpił błąd podczas inicjalizacji widoku kasjera: " + ex.Message;
                return View("Error");
            }
        }


                        public ActionResult Details(string nip)
        {
            if (string.IsNullOrEmpty(nip))
            {
                                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                                CustomerDTO customer = _customerService.GetCustomerByPesel(nip); 
                if (customer == null)
                {
                                        return HttpNotFound();
                }

                                return View(customer);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Wystąpił błąd podczas pobierania szczegółów klienta o NIP {nip}: " + ex.Message;
                return View("Error");
            }
        }

                        public ActionResult Create()
        {
                                    return View(new CustomerDTO());
        }

                        

                        [HttpPost, ActionName("Delete")]         [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string nip)         {
            if (string.IsNullOrEmpty(nip))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                                                                
                CustomerDTO customerToDelete = _customerService.GetCustomerByPesel(nip);

                if (customerToDelete == null)
                {
                                        return RedirectToAction("Index");
                }

                                _customerService.RemoveCustomer(customerToDelete);

                                return RedirectToAction("Index");
            }
                                                                                                                                    catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Wystąpił błąd podczas usuwania klienta o NIP {nip}: " + ex.Message;
                                CustomerDTO customer = _customerService.GetCustomerByPesel(nip);                 return View("Delete", customer);             }
        }

                                                                
        [HttpPost]
        [ValidateAntiForgeryToken]         public ActionResult Create(CustomerDTO customerDTO)         {
                        if (Request.IsAjaxRequest())
            {
                                            }

            if (ModelState.IsValid)
            {
                try
                {
                    _customerService.AddCustomer(customerDTO); 
                    if (Request.IsAjaxRequest())
                    {
                                                return Json(new { success = true, message = "Klient został pomyślnie dodany." });
                    }
                                        TempData["SuccessMessage"] = "Klient został pomyślnie dodany.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                                        
                    if (Request.IsAjaxRequest())
                    {
                                                return Json(new { success = false, errors = new[] { "Wystąpił błąd serwera podczas dodawania klienta: " + ex.Message } });
                    }
                    ModelState.AddModelError("", "Wystąpił błąd podczas dodawania klienta: " + ex.Message);
                }
            }

                        if (Request.IsAjaxRequest())
            {
                                var errorList = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                ).Where(m => m.Value.Any()).ToList();

                return Json(new { success = false, errors = errorList, isValidationError = true });
            }

                        return View(customerDTO);
        }
    }
}