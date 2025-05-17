using System;
using System.Web.Mvc;
using GasStation.Services; using GasStation.DTO;
using System.Web.Security;                                                       using System.Web;

namespace GasStation.Controllers
{
    public class HomeController : Controller     {
                private readonly EmployeeService _employeeService;

                public HomeController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

                        [HttpGet]         public ActionResult Index()
		{		 			if (User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Cashier_view", "Cashier");
			}

						return View(new EmployeeLoginDTO());
		}

                        [HttpPost]         [ValidateAntiForgeryToken]         [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult Login(EmployeeLoginDTO loginDto) 		{
			if (ModelState.IsValid)
			{
				try
				{
										EmployeeDTO authenticatedEmployee = _employeeService.Authenticate(loginDto);

										if (authenticatedEmployee != null)
					{
						
																		Session["LoggedInEmployee"] = authenticatedEmployee;
												

												return RedirectToAction("Cashier_view", "Cashier");
					}
					else
					{
												ModelState.AddModelError("", "Niepoprawna próba logowania. Sprawdź login i hasło.");
					}
				}
				catch (Exception ex)
				{
										ModelState.AddModelError("", "Wystąpił nieoczekiwany błąd podczas logowania: " + ex.Message);
									}
			}

			return View("Index", loginDto);
		}


                        
                                                public ActionResult Logout()
        {
                        FormsAuthentication.SignOut();

                        Session.Remove("LoggedInEmployee");
            Session.Clear();
            Session.Abandon();

                        Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetNoStore();

                        if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-20);
            }

            if (Request.Cookies["AuthToken"] != null)
            {
                Response.Cookies["AuthToken"].Value = string.Empty;
                Response.Cookies["AuthToken"].Expires = DateTime.Now.AddMonths(-20);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}