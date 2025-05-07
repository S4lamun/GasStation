//// Controllers/AccountController.cs
//using GasStation.DTO;
//using GasStation.DTOs; // Używasz DTO, które nazwałeś np. EmployeeLoginDTO
//using GasStation.Services;
//using System.Web.Mvc;
//using System.Web.Security; // Dla FormsAuthentication

//namespace GasStation.Controllers
//{
//	public class AccountController : Controller
//	{
//		private readonly EmployeeService _employeeService;

//		public AccountController(EmployeeService employeeService)
//		{
//			_employeeService = employeeService;
//		}

//		[AllowAnonymous]
//		public ActionResult Login(string returnUrl)
//		{
//			ViewBag.ReturnUrl = returnUrl;
//			return View();
//		}

//		[HttpPost]
//		[AllowAnonymous]
//		[ValidateAntiForgeryToken]
//		public ActionResult Login(EmployeeLoginDto model, string returnUrl)
//		{
//			if (!ModelState.IsValid)
//			{
//				return View(model);
//			}

//			var employee = _employeeService.Authenticate(model);

//			if (employee != null)
//			{
//				FormsAuthentication.SetAuthCookie(model.Login, false); // Zaloguj użytkownika
//																	   // Tutaj możesz dodać logikę ról, jeśli ją zaimplementujesz
//																	   // np. zapisując role w UserData ciasteczka lub w sesji.

//				if (Url.IsLocalUrl(returnUrl))
//				{
//					return Redirect(returnUrl);
//				}
//				return RedirectToAction("Index", "Home"); // Lub do panelu kasjera
//			}
//			else
//			{
//				ModelState.AddModelError("", "Invalid login or password.");
//				return View(model);
//			}
//		}

//		[HttpPost]
//		[ValidateAntiForgeryToken]
//		public ActionResult LogOff()
//		{
//			FormsAuthentication.SignOut();
//			return RedirectToAction("Index", "Home");
//		}
//	}
//}