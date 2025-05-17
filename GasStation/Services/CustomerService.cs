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
						var existingCustomer = _context.Customers.Any(c => c.Nip == customerDTO.Nip);
			if (existingCustomer)
			{
												throw new Exception($"Klient o NIP {customerDTO.Nip} już istnieje.");
							}

			var customer = new Customer
			{
				Nip = customerDTO.Nip,
				CompanyName = customerDTO.CompanyName,
							};

			_context.Customers.Add(customer);

						try
			{
				_context.SaveChanges();
			}
			catch (DbUpdateException ex)
			{
												throw new Exception($"Błąd bazy danych podczas dodawania klienta: {ex.InnerException?.Message ?? ex.Message}"); 																																			}
			catch (DbEntityValidationException ex)
			{
								var errorMessages = ex.EntityValidationErrors
					.SelectMany(x => x.ValidationErrors)
					.Select(x => x.ErrorMessage);
				var fullErrorMessage = string.Join("; ", errorMessages);
								throw new Exception($"Błędy walidacji encji: {fullErrorMessage}"); 																				   			}
			catch (Exception ex) 			{
								throw new Exception($"Wystąpił nieoczekiwany błąd podczas dodawania klienta: {ex.Message}"); 																											 			}


			return customerDTO; 		}

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
            return null;         }
    }
}