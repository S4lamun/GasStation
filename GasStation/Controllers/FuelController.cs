// W pliku GasStation.Controllers/FuelController.cs

using System;
using System.Linq;
using System.Collections.Generic;
using System.Net; // Dodaj dla HttpStatusCode
using System.Web.Mvc;
using GasStation.Services;
using GasStation.DTO;
// Upewnij się, że masz using do klasy wyjątków biznesowych, jeśli jest w innym namespace
// using GasStation.Services.Exceptions; 

namespace GasStation.Controllers
{
    // Klasa kontrolera odpowiadająca za operacje związane z paliwami i ich cenami
    public class FuelController : Controller
    {
        // Zależności od serwisów - wstrzyknięte przez DI
        private readonly FuelService _fuelService;
        private readonly FuelPriceHistoryService _priceHistoryService; // Dodajemy zależność do serwisu historii cen

        // Konstruktor dla Dependency Injection (DI)
        public FuelController(FuelService fuelService, FuelPriceHistoryService priceHistoryService)
        {
            _fuelService = fuelService;
            _priceHistoryService = priceHistoryService; // Przypisanie wstrzykniętej zależności
        }

        // --- Akcje związane z paliwami (Fuel) ---

        // GET: /Fuel/Index (Opcjonalna akcja widoku - przykład)
        // Możesz tu wyświetlić listę paliw lub przekierować do API
        public ActionResult Index()
        {
            // Możesz tu np. przekazać listę paliw do widoku:
            // var fuels = _fuelService.GetAllFuels();
            // return View(fuels);

            // Lub po prostu zwrócić widok startowy dla tej sekcji
            return View();
        }

        // GET: /Fuel/GetAllFuelsJson
        // Zwraca listę wszystkich paliw w formacie JSON
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
                // Logowanie błędu na serwerze (bardzo ważne w rzeczywistej aplikacji!)
                // np. System.Diagnostics.Trace.TraceError($"Błąd w GetAllFuelsJson: {ex.Message}");

                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { message = "Wystąpił błąd serwera podczas ładowania listy paliw." }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Fuel/GetFuelByIdJson/{id}
        // Zwraca szczegóły paliwa o podanym ID w formacie JSON
        [HttpGet]
        public ActionResult GetFuelByIdJson(int id) // Używamy ActionResult, żeby zwrócić np. NotFound
        {
            try
            {
                var fuel = _fuelService.GetFuelById(id);

                if (fuel == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound; // 404 Not Found
                    return Json(new { message = $"Paliwo o ID {id} nie znaleziono." }, JsonRequestBehavior.AllowGet);
                }

                return Json(fuel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Logowanie błędu
                Response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500 Internal Server Error
                return Json(new { message = $"Wystąpił błąd serwera podczas ładowania paliwa o ID {id}." }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: /Fuel/AddFuel
        // Dodaje nowe paliwo na podstawie danych z DTO
        [HttpPost] // Odpowiada na żądania HTTP POST
        public ActionResult AddFuel(FuelDTO fuelDTO) // Model binding automatycznie zmapuje dane z żądania do obiektu FuelDTO
        {
            // Podstawowa walidacja modelu z atrybutów w DTO
            if (!ModelState.IsValid)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest; // 400 Bad Request
                // Zwróć błędy walidacji
                return Json(new { message = "Niepoprawne dane wejściowe.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                var addedFuel = _fuelService.AddFuel(fuelDTO);
                Response.StatusCode = (int)HttpStatusCode.Created; // 201 Created
                // Zwróć obiekt nowo dodanego paliwa (ewentualnie z nadanym ID)
                return Json(addedFuel); // JsonRequestBehavior.AllowGet nie jest potrzebne dla POST
            }
            catch (Exception ex)
            {
                // Logowanie błędu
                Response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500 Internal Server Error
                return Json(new { message = "Wystąpił błąd serwera podczas dodawania paliwa." }); // JsonRequestBehavior.AllowGet nie jest potrzebne dla POST
            }
        }

        // POST: /Fuel/RemoveFuel (Alternatywnie można użyć DELETE /Fuel/{id})
        // Usuwa paliwo o podanym ID
        [HttpPost] // Często używane w formularzach MVC, ale DELETE jest standardem RESTful API
        // [HttpDelete] // Można też użyć metody DELETE dla RESTful API i przyjąć id jako parametr trasy
        public ActionResult RemoveFuel(FuelDTO fuelDTO) // Przyjmujemy DTO, żeby mieć ID
        {
            if (fuelDTO == null || fuelDTO.FuelId <= 0)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { message = "Wymagane FuelId jest niepoprawne." });
            }

            try
            {
                // W zależności od implementacji RemoveFuel w serwisie, może być potrzebne łapanie wyjątków, np. gdy paliwo jest używane.
                _fuelService.RemoveFuel(fuelDTO);
                Response.StatusCode = (int)HttpStatusCode.NoContent; // 204 No Content - sukces, ale nic nie zwracamy

                // Można też zwrócić pusty JsonResult lub inny typ, zależnie od potrzeb klienta
                return new EmptyResult(); // Zwraca pustą odpowiedź 204
            }
            catch (Exception ex)
            {
                // Logowanie błędu
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { message = $"Wystąpił błąd serwera podczas usuwania paliwa o ID {fuelDTO.FuelId}." });
            }
        }


        // --- Akcje związane z historią cen paliw (FuelPriceHistory) ---

        // GET: /Fuel/GetCurrentPricesJson
        // *** WYMAGANA AKCJA: Zwraca aktualne ceny paliw w formacie JSON ***
        // Adres URL: /Fuel/GetCurrentPricesJson
        // Ta akcja korzysta z metody w FuelService, tak jak w oryginalnym kodzie
        [HttpGet]
        public JsonResult GetCurrentPricesJson()
        {
            try
            {
                // Wywołaj metodę serwisu, aby pobrać listę aktualnych cen
                // Metoda serwisu zwraca List<FuelPriceHistoryDTO>
                // UWAGA: Ta metoda GetAllCurrentPrices jest w FuelService w Twoim kodzie.
                // Używamy jej zgodnie z dostarczonym kodem, choć logicznie pasowałaby do FuelPriceHistoryService.
                var currentPricesFromService = _fuelService.GetAllCurrentPrices();

                // Mapuj DTO z serwisu na format oczekiwany przez kod JavaScript (tylko FuelId i Price)
                var dataForJson = currentPricesFromService.Select(p => new
                {
                    FuelId = p.FuelId,
                    Price = p.Price
                }).ToList();

                // Zwróć dane w formacie JSON
                return Json(dataForJson, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Logowanie błędu
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { message = "Wystąpił błąd serwera podczas ładowania aktualnych cen paliw." }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Fuel/GetAllPriceHistoriesJson
        // Zwraca całą historię cen dla wszystkich paliw
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
                // Logowanie błędu
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { message = "Wystąpił błąd serwera podczas ładowania całej historii cen paliw." }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Fuel/GetPriceHistoryByIdJson/{id}
        // Zwraca pojedynczy wpis historii cen o podanym ID
        [HttpGet]
        public ActionResult GetPriceHistoryByIdJson(int id)
        {
            try
            {
                var historyEntry = _priceHistoryService.GetPriceHistoryById(id);

                if (historyEntry == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound; // 404 Not Found
                    return Json(new { message = $"Historia cen o ID {id} nie znaleziono." }, JsonRequestBehavior.AllowGet);
                }

                return Json(historyEntry, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Logowanie błędu
                Response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500 Internal Server Error
                return Json(new { message = $"Wystąpił błąd serwera podczas ładowania historii cen o ID {id}." }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Fuel/GetPriceHistoryForFuelJson/{fuelId}
        // Zwraca historię cen dla konkretnego paliwa
        [HttpGet]
        public ActionResult GetPriceHistoryForFuelJson(int fuelId)
        {
            try
            {
                var history = _priceHistoryService.GetPriceHistoryForFuel(fuelId);
                // Metoda w serwisie rzuca BusinessLogicException jeśli paliwo nie istnieje
                return Json(history, JsonRequestBehavior.AllowGet);
            }
            catch (BusinessLogicException ble)
            {
                // Logowanie błędu biznesowego
                Response.StatusCode = (int)HttpStatusCode.BadRequest; // 400 Bad Request dla błędów walidacji biznesowej
                return Json(new { message = ble.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Logowanie błędu
                Response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500 Internal Server Error
                return Json(new { message = $"Wystąpił błąd serwera podczas ładowania historii cen dla paliwa o ID {fuelId}." }, JsonRequestBehavior.AllowGet);
            }
        }


        // POST: /Fuel/AddNewPrice
        // Dodaje nowy wpis historii cen
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
                Response.StatusCode = (int)HttpStatusCode.Created; // 201 Created
                return Json(addedEntry); // Zwracamy nowo dodany obiekt
            }
            catch (BusinessLogicException ble)
            {
                // Logowanie błędu biznesowego
                // 409 Conflict jest dobrym kodem dla błędów związanych z naruszeniem reguł biznesowych (np. daty nakładają się)
                // Możesz też użyć 400 Bad Request w zależności od konkretnego błędu
                Response.StatusCode = (int)HttpStatusCode.Conflict;
                return Json(new { message = ble.Message });
            }
            catch (Exception ex)
            {
                // Logowanie błędu
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { message = "Wystąpił błąd serwera podczas dodawania nowej ceny paliwa." });
            }
        }

        // PUT: /Fuel/UpdatePriceEntry (lub POST /Fuel/UpdatePriceEntry z ID w DTO)
        // Aktualizuje istniejący wpis historii cen
        [HttpPut] // RESTful convention for updates
        // [HttpPost] // Alternatywnie, jeśli używasz tylko POST
        public ActionResult UpdatePriceEntry(FuelPriceHistoryDTO historyDto)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { message = "Niepoprawne dane wejściowe.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                var updatedEntry = _priceHistoryService.UpdatePriceEntry(historyDto);
                Response.StatusCode = (int)HttpStatusCode.OK; // 200 OK
                return Json(updatedEntry); // Zwracamy zaktualizowany obiekt
            }
            catch (BusinessLogicException ble)
            {
                // Logowanie błędu biznesowego
                // Sprawdź wiadomość błędu z serwisu, żeby zwrócić odpowiedni status
                // "not found" -> 404
                // "overlaps", "Start date must be before end date" -> 409 lub 400
                if (ble.Message.Contains("not found"))
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound; // 404 Not Found
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.Conflict; // 409 Conflict
                }
                return Json(new { message = ble.Message });
            }
            catch (Exception ex)
            {
                // Logowanie błędu
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { message = $"Wystąpił błąd serwera podczas aktualizacji historii cen o ID {historyDto?.FuelPriceHistoryId}." });
            }
        }

        // DELETE: /Fuel/DeletePriceEntry/{id} (lub POST /Fuel/DeletePriceEntry z ID)
        // Usuwa wpis historii cen o podanym ID
        [HttpDelete] // RESTful convention for deletion
        // [HttpPost] // Alternatywnie, jeśli używasz tylko POST
        public ActionResult DeletePriceEntry(int id)
        {
            if (id <= 0)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { message = "Wymagane ID jest niepoprawne." });
            }

            try
            {
                _priceHistoryService.DeletePriceEntry(id);
                Response.StatusCode = (int)HttpStatusCode.NoContent; // 204 No Content - sukces, nic nie zwracamy

                // Zwróć pusty wynik dla statusu 204
                return new EmptyResult();
            }
            catch (BusinessLogicException ble)
            {
                // Logowanie błędu biznesowego (np. nie można usunąć, bo wpis jest używany)
                Response.StatusCode = (int)HttpStatusCode.Conflict; // 409 Conflict
                return Json(new { message = ble.Message });
            }
            catch (Exception ex)
            {
                // Logowanie błędu
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { message = $"Wystąpił błąd serwera podczas usuwania historii cen o ID {id}." });
            }
        }
    }
}