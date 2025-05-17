using System;
using System.Collections.Generic;
using System.Web.Mvc;
using GasStation.Services;
using GasStation.DTO;
using GasStation.Models;
using System.Linq;

namespace GasStation.Controllers
{
        public class EmployeeController : Controller     {
        private readonly EmployeeService _employeeService;

        public EmployeeController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        
                        [HttpGet]         public ActionResult Login()
        {
                        return View(new EmployeeLoginDTO());
        }

                        [HttpPost]         [ValidateAntiForgeryToken]         public ActionResult Login(EmployeeLoginDTO loginDto)         {
                        if (ModelState.IsValid)
            {
                try
                {
                                        EmployeeDTO authenticatedEmployee = _employeeService.Authenticate(loginDto);

                                        if (authenticatedEmployee != null)
                    {
                                                                                                
                                                return RedirectToAction("Cashier_view", "Cashier");                     }
                    else
                    {
                                                ModelState.AddModelError("", "Invalid login attempt. Please check your login and password.");
                                            }
                }
                                                                                                catch (Exception ex)
                {
                                        ModelState.AddModelError("", "An unexpected error occurred during login: " + ex.Message);
                                    }
            }

                                    return View(loginDto);
        }

                
                public ActionResult Index()
        {
                        return View();         }

                public ActionResult Details(string pesel)
        {
                        return View();         }

                public ActionResult Create()
        {
                        return View(new CreateEmployeeDTO());         }

                [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateEmployeeDTO employeeDto)
        {
                        return View(employeeDto);         }

                public ActionResult Edit(string pesel)
        {
                        return View();         }

                [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EmployeeDTO employeeDto)
        {
                        return View(employeeDto);         }

                public ActionResult Delete(string pesel)
        {
                        return View();         }

        [HttpGet]
        public JsonResult GetAllEmployeesJson()
        {
            try
            {
                                List<EmployeeDTO> employees = _employeeService.GetAllEmployees();

                                var result = employees.Select(c => new
                {
                    Pesel = c.Pesel,
                    Name = c.Name,
                    Surname = c.Surname
                                    }).ToList();

                                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                                return Json(new { success = false, message = "Wystąpił błąd podczas pobierania listy klientów: " + ex.Message },
                           JsonRequestBehavior.AllowGet);
            }
        }

                [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string pesel)
        {
                        return RedirectToAction("Index");         }

                                            }
}