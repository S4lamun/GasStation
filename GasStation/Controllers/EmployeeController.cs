using System;
using System.Collections.Generic;
using System.Web.Mvc;
using GasStation.Services;
using GasStation.DTO;
using GasStation.Models;
// Jeśli masz własne wyjątki biznesowe, dodaj using
// using GasStation.Exceptions; // example

namespace GasStation.Controllers
{
    // Kontroler do zarządzania pracownikami (lub zmień nazwę na AccountController jeśli ma tylko logowanie)
    public class EmployeeController : Controller // Consider renaming to AccountController
    {
        private readonly EmployeeService _employeeService;

        public EmployeeController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // --- Login Actions ---

        // GET: Employee/Login (or Account/Login)
        // Akcja wyświetlająca formularz logowania
        [HttpGet] // Explicitly state this is for GET requests
        public ActionResult Login()
        {
            // Return the login view, passing an empty DTO for helpers
            return View(new EmployeeLoginDTO());
        }

        // POST: Employee/Login (or Account/Login)
        // Akcja obsługująca wysłanie formularza logowania
        [HttpPost] // Explicitly state this is for POST requests
        [ValidateAntiForgeryToken] // Recommended for security
        public ActionResult Login(EmployeeLoginDTO loginDto) // Model binder will populate this DTO
        {
            // 1. Check if the data received from the form is valid based on DTO annotations
            if (ModelState.IsValid)
            {
                try
                {
                    // 2. Call the service to authenticate the employee
                    EmployeeDTO authenticatedEmployee = _employeeService.Authenticate(loginDto);

                    // 3. Check the authentication result
                    if (authenticatedEmployee != null)
                    {
                        // Authentication successful!
                        // TODO: Implement actual authentication mechanism (e.g., Forms Authentication, Identity)
                        // For now, let's simulate success by redirecting to the Cashier view
                        // In a real app, you would set authentication cookie/principal here

                        // Example: Redirect to the Cashier view (as in your original HTML link)
                        return RedirectToAction("Cashier_view", "Cashier"); // Assuming Cashier_view action exists in CashierController
                    }
                    else
                    {
                        // Authentication failed (login not found, password incorrect, or login != password business rule)
                        ModelState.AddModelError("", "Invalid login attempt. Please check your login and password.");
                        // Note: A generic error message is better for security (doesn't reveal if login exists)
                    }
                }
                // Catch specific business logic exceptions if you want to show specific messages
                // catch (BusinessLogicException ex)
                // {
                //     ModelState.AddModelError("", ex.Message);
                // }
                catch (Exception ex)
                {
                    // Catch any other unexpected errors during the process
                    ModelState.AddModelError("", "An unexpected error occurred during login: " + ex.Message);
                    // Log the exception (recommended in a real app)
                }
            }

            // If ModelState is not valid, or authentication failed, return the view
            // with the entered data and validation/error messages.
            return View(loginDto);
        }

        // --- Existing Employee Management Actions ---
        // (Index, Details, Create, Edit, Delete etc. go here)

        // GET: Employee
        public ActionResult Index()
        {
            // ... existing Index action ...
            return View(); // Placeholder
        }

        // GET: Employee/Details/12345678901
        public ActionResult Details(string pesel)
        {
            // ... existing Details action ...
            return View(); // Placeholder
        }

        // GET: Employee/Create
        public ActionResult Create()
        {
            // ... existing Create action ...
            return View(new CreateEmployeeDTO()); // Placeholder
        }

        // POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateEmployeeDTO employeeDto)
        {
            // ... existing Create POST action ...
            return View(employeeDto); // Placeholder
        }

        // GET: Employee/Edit/12345678901
        public ActionResult Edit(string pesel)
        {
            // ... existing Edit action ...
            return View(); // Placeholder
        }

        // POST: Employee/Edit/12345678901
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EmployeeDTO employeeDto)
        {
            // ... existing Edit POST action ...
            return View(employeeDto); // Placeholder
        }

        // GET: Employee/Delete/12345678901
        public ActionResult Delete(string pesel)
        {
            // ... existing Delete action ...
            return View(); // Placeholder
        }

        // POST: Employee/Delete/12345678901
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string pesel)
        {
            // ... existing DeleteConfirmed action ...
            return RedirectToAction("Index"); // Placeholder
        }

        // --- Optional Error View (if not already in Shared) ---
        // public ActionResult Error()
        // {
        //     return View();
        // }
    }
}