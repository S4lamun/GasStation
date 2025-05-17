using System;
using System.Collections.Generic;
using System.Web.Mvc;
using GasStation.Services; using GasStation.DTO; using GasStation.Models;
using System.Linq;                                       

namespace GasStation.Controllers
{
        public class CustomerController : Controller
    {
                private readonly CustomerService _customerService;

                public CustomerController()
        {

        }
        public CustomerController(CustomerService customerService)
        {
            _customerService = customerService;
        }

                        public ActionResult Index()
        {
            try
            {
                                List<CustomerDTO> customers = _customerService.GetAllCustomers();

                                return View(customers);
            }
            catch (Exception ex)
            {
                                                ViewBag.ErrorMessage = "Wystąpił błąd podczas pobierania listy klientów: " + ex.Message;
                return View("Error");             }
        }
        [HttpGet]
        public JsonResult GetAllCustomersJson()
        {
            try
            {
                var customers = _customerService.GetAllCustomers()
                    .Select(c => new {
                        Nip = c.Nip,
                        CompanyName = c.CompanyName
                    }).ToList();

                return Json(customers, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
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