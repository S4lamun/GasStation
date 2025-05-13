using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using GasStation.Data;
using GasStation.DTO;
using GasStation.Models;

namespace GasStation.Services
{
    public class CustomerService
    {
        private readonly GasStationDbContext _context;
        public CustomerService(GasStationDbContext context)
        {
            _context = context;
        }

        public List<CustomerDTO> GetAllCustomers()
        {
            var customers = _context.Customers.ToList();
            return customers.Select(c => new CustomerDTO
            {
                Nip = c.Nip,
                CompanyName = c.CompanyName,
            }).ToList();
        }

        public CustomerDTO AddCustomer(CustomerDTO customerDTO)
		{
			// *** Nowa/Zmodyfikowana logika: Sprawdzenie unikalności NIP ***
			var existingCustomer = _context.Customers.Any(c => c.Nip == customerDTO.Nip);
			if (existingCustomer)
			{
				// Rzuć wyjątek biznesowy z konkretnym komunikatem
				// Uzyj wlasnej klasy BusinessLogicException jesli ja masz
				throw new Exception($"Klient o NIP {customerDTO.Nip} już istnieje.");
				// lub: throw new BusinessLogicException($"Klient o NIP {customerDTO.Nip} już istnieje."); 
			}

			var customer = new Customer
			{
				Nip = customerDTO.Nip,
				CompanyName = customerDTO.CompanyName,
				// ... inne pola, jesli sa
			};

			_context.Customers.Add(customer);

			// *** Obsługa błędów zapisu w bazie (DbUpdateException, DbEntityValidationException) ***
			try
			{
				_context.SaveChanges();
			}
			catch (DbUpdateException ex)
			{
				// Przechwyć błąd bazy danych (np. naruszenie unikalności, jeśli poprzednie Any() nie zadziałało w scenariuszu wyścigu)
				// Logger.LogError("DbUpdateException podczas dodawania klienta", ex); // Logowanie
				throw new Exception($"Błąd bazy danych podczas dodawania klienta: {ex.InnerException?.Message ?? ex.Message}"); // Rzuć nowy wyjątek lub BusinessLogicException
																																// lub: throw new BusinessLogicException($"Błąd bazy danych podczas dodawania klienta: {ex.InnerException?.Message ?? ex.Message}"); 
			}
			catch (DbEntityValidationException ex)
			{
				// Przechwyć błąd walidacji EF (np. naruszenie [Required] na encji, które przeszło przez DTO lub walidacja zostala pominieta)
				var errorMessages = ex.EntityValidationErrors
					.SelectMany(x => x.ValidationErrors)
					.Select(x => x.ErrorMessage);
				var fullErrorMessage = string.Join("; ", errorMessages);
				// Logger.LogError("DbEntityValidationException podczas dodawania klienta", ex); // Logowanie
				throw new Exception($"Błędy walidacji encji: {fullErrorMessage}"); // Rzuć nowy wyjątek lub BusinessLogicException
																				   // lub: throw new BusinessLogicException($"Błędy walidacji encji: {fullErrorMessage}"); 
			}
			catch (Exception ex) // Ogólny catch dla innych błędów
			{
				// Logger.LogError("Nieoczekiwany błąd podczas dodawania klienta", ex); // Logowanie
				throw new Exception($"Wystąpił nieoczekiwany błąd podczas dodawania klienta: {ex.Message}"); // Rzuć nowy wyjątek lub BusinessLogicException
																											 // lub: throw new BusinessLogicException($"Wystąpił nieoczekiwany błąd podczas dodawania klienta: {ex.Message}"); 
			}


			return customerDTO; // Zwróć DTO po sukcesie
		}

		public void RemoveCustomer(CustomerDTO customerDTO)
        {
            var customer = _context.Customers.Find(customerDTO.Nip);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                _context.SaveChanges();
            }
        }

        public CustomerDTO GetCustomerByPesel(string nip)
        {
            var customer = _context.Customers.Find(nip);
            if (customer != null)
            {
                return new CustomerDTO
                {
                    Nip = customer.Nip,
                    CompanyName = customer.CompanyName,
                };
            }
            return null; // Customer not found
        }
    }
}