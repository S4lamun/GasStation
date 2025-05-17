
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net; using System.Web.Mvc;
using GasStation.Services;
using GasStation.DTO;

namespace GasStation.Controllers
{
        public class FuelController : Controller
    {
                private readonly FuelService _fuelService;
        private readonly FuelPriceHistoryService _priceHistoryService; 
                public FuelController(FuelService fuelService, FuelPriceHistoryService priceHistoryService)
        {
            _fuelService = fuelService;
            _priceHistoryService = priceHistoryService;         }

        
                        public ActionResult Index()
        {
                                    
                        return View();
        }

                        [HttpGet]
        public JsonResult GetAllFuelsJson()
        {
            try
            {
                var fuels = _fuelService.GetAllFuels();
                return Json(fuels, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                                
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { message = "Wystąpił błąd serwera podczas ładowania listy paliw." }, JsonRequestBehavior.AllowGet);
            }
        }

                        [HttpGet]
        public ActionResult GetFuelByIdJson(int id)         {
            try
            {
                var fuel = _fuelService.GetFuelById(id);

                if (fuel == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;                     return Json(new { message = $"Paliwo o ID {id} nie znaleziono." }, JsonRequestBehavior.AllowGet);
                }

                return Json(fuel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                                Response.StatusCode = (int)HttpStatusCode.InternalServerError;                 return Json(new { message = $"Wystąpił błąd serwera podczas ładowania paliwa o ID {id}." }, JsonRequestBehavior.AllowGet);
            }
        }

                        [HttpPost]         public ActionResult AddFuel(FuelDTO fuelDTO)         {
                        if (!ModelState.IsValid)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;                                 return Json(new { message = "Niepoprawne dane wejściowe.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                var addedFuel = _fuelService.AddFuel(fuelDTO);
                Response.StatusCode = (int)HttpStatusCode.Created;                                 return Json(addedFuel);             }
            catch (Exception ex)
            {
                                Response.StatusCode = (int)HttpStatusCode.InternalServerError;                 return Json(new { message = "Wystąpił błąd serwera podczas dodawania paliwa." });             }
        }

                        [HttpPost]                 public ActionResult RemoveFuel(FuelDTO fuelDTO)         {
            if (fuelDTO == null || fuelDTO.FuelId <= 0)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { message = "Wymagane FuelId jest niepoprawne." });
            }

            try
            {
                                _fuelService.RemoveFuel(fuelDTO);
                Response.StatusCode = (int)HttpStatusCode.NoContent; 
                                return new EmptyResult();             }
            catch (Exception ex)
            {
                                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { message = $"Wystąpił błąd serwera podczas usuwania paliwa o ID {fuelDTO.FuelId}." });
            }
        }


        
                                        [HttpGet]
        public JsonResult GetCurrentPricesJson()
        {
            try
            {
                                                                                var currentPricesFromService = _fuelService.GetAllCurrentPrices();

                                var dataForJson = currentPricesFromService.Select(p => new
                {
                    FuelId = p.FuelId,
                    Price = p.Price
                }).ToList();

                                return Json(dataForJson, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { message = "Wystąpił błąd serwera podczas ładowania aktualnych cen paliw." }, JsonRequestBehavior.AllowGet);
            }
        }

                        [HttpGet]
        public JsonResult GetAllPriceHistoriesJson()
        {
            try
            {
                var history = _priceHistoryService.GetAllPriceHistories();
                return Json(history, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { message = "Wystąpił błąd serwera podczas ładowania całej historii cen paliw." }, JsonRequestBehavior.AllowGet);
            }
        }

                        [HttpGet]
        public ActionResult GetPriceHistoryByIdJson(int id)
        {
            try
            {
                var historyEntry = _priceHistoryService.GetPriceHistoryById(id);

                if (historyEntry == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;                     return Json(new { message = $"Historia cen o ID {id} nie znaleziono." }, JsonRequestBehavior.AllowGet);
                }

                return Json(historyEntry, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                                Response.StatusCode = (int)HttpStatusCode.InternalServerError;                 return Json(new { message = $"Wystąpił błąd serwera podczas ładowania historii cen o ID {id}." }, JsonRequestBehavior.AllowGet);
            }
        }

                        [HttpGet]
        public ActionResult GetPriceHistoryForFuelJson(int fuelId)
        {
            try
            {
                var history = _priceHistoryService.GetPriceHistoryForFuel(fuelId);
                                return Json(history, JsonRequestBehavior.AllowGet);
            }
            catch (BusinessLogicException ble)
            {
                                Response.StatusCode = (int)HttpStatusCode.BadRequest;                 return Json(new { message = ble.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                                Response.StatusCode = (int)HttpStatusCode.InternalServerError;                 return Json(new { message = $"Wystąpił błąd serwera podczas ładowania historii cen dla paliwa o ID {fuelId}." }, JsonRequestBehavior.AllowGet);
            }
        }


                        [HttpPost]
        public ActionResult AddNewPrice(CreateFuelPriceHistoryDTO newPriceDto)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { message = "Niepoprawne dane wejściowe.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                var addedEntry = _priceHistoryService.AddNewPrice(newPriceDto);
                Response.StatusCode = (int)HttpStatusCode.Created;                 return Json(addedEntry);             }
            catch (BusinessLogicException ble)
            {
                                                                Response.StatusCode = (int)HttpStatusCode.Conflict;
                return Json(new { message = ble.Message });
            }
            catch (Exception ex)
            {
                                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { message = "Wystąpił błąd serwera podczas dodawania nowej ceny paliwa." });
            }
        }

                        [HttpPut]                 public ActionResult UpdatePriceEntry(FuelPriceHistoryDTO historyDto)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { message = "Niepoprawne dane wejściowe.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                var updatedEntry = _priceHistoryService.UpdatePriceEntry(historyDto);
                Response.StatusCode = (int)HttpStatusCode.OK;                 return Json(updatedEntry);             }
            catch (BusinessLogicException ble)
            {
                                                                                if (ble.Message.Contains("not found"))
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;                 }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.Conflict;                 }
                return Json(new { message = ble.Message });
            }
            catch (Exception ex)
            {
                                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { message = $"Wystąpił błąd serwera podczas aktualizacji historii cen o ID {historyDto?.FuelPriceHistoryId}." });
            }
        }

                        [HttpDelete]                 public ActionResult DeletePriceEntry(int id)
        {
            if (id <= 0)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { message = "Wymagane ID jest niepoprawne." });
            }

            try
            {
                _priceHistoryService.DeletePriceEntry(id);
                Response.StatusCode = (int)HttpStatusCode.NoContent; 
                                return new EmptyResult();
            }
            catch (BusinessLogicException ble)
            {
                                Response.StatusCode = (int)HttpStatusCode.Conflict;                 return Json(new { message = ble.Message });
            }
            catch (Exception ex)
            {
                                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { message = $"Wystąpił błąd serwera podczas usuwania historii cen o ID {id}." });
            }
        }
    }
}