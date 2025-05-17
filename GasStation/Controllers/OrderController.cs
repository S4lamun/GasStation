using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;



using System.Web.Mvc;

using GasStation.Services;
using GasStation.DTO;
using GasStation.Models;

namespace GasStation.Controllers
{
		public class OrderController : Controller
	{
				private readonly OrderService _orderService;
		private readonly CustomerService _customerService; 		private readonly EmployeeService _employeeService; 														   
				public OrderController(
			OrderService orderService,
			CustomerService customerService,
			EmployeeService employeeService) 		{
			_orderService = orderService;
			_customerService = customerService;
			_employeeService = employeeService;
					}

						public ActionResult Index()
		{
			try
			{
								List<OrderDTO> orders = _orderService.GetAllOrders();

								return View(orders);
			}
			catch (Exception ex)
			{
								ViewBag.ErrorMessage = "Wystąpił błąd podczas pobierania listy zamówień: " + ex.Message;
				return View("Error"); 			}
		}

						public ActionResult Details(int? id)
		{
			if (id == null)
			{
								return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
			}

			try
			{
								OrderDTO order = _orderService.GetOrderById(id.Value);

				if (order == null)
				{
										return HttpNotFound();
				}

								return View(order);
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"Wystąpił błąd podczas pobierania szczegółów zamówienia o ID {id}: " + ex.Message;
				return View("Error");
			}
		}

                        public ActionResult Create()
        {
            try
            {
                                var customers = _customerService.GetAllCustomers() ?? new List<CustomerDTO>();
                var employees = _employeeService.GetAllEmployees() ?? new List<EmployeeDTO>();

                                ViewBag.CustomerList = new SelectList(customers, "Nip", "CompanyName");
                ViewBag.EmployeeList = employees.Select(e => new SelectListItem
                {
                    Value = e.Pesel,
                    Text = $"{e.Name} {e.Surname}"                 }).ToList();

                return View(new CreateOrderDTO { Items = new List<OrderItemDTO>() });
            }
            catch (Exception ex)
            {
                
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

                        public ActionResult Delete(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
			}

			try
			{
								OrderDTO order = _orderService.GetOrderById(id.Value);

				if (order == null)
				{
					return HttpNotFound();
				}

								return View(order);
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"Wystąpił błąd podczas przygotowywania do usunięcia zamówienia o ID {id}: " + ex.Message;
				return View("Error");
			}
		}

						[HttpPost, ActionName("Delete")] 		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id) 		{
			if (id == 0) 			{
				return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
			}

			try
			{
								_orderService.DeleteOrder(id);

								return RedirectToAction("Index");
			}
																											catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"Wystąpił błąd podczas usuwania zamówienia o ID {id}: " + ex.Message;
								OrderDTO order = _orderService.GetOrderById(id); 				return View("Delete", order); 			}
		}

																						

														
															}
}
