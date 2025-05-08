using System;
using System.Collections.Generic;
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
            var customer = new Customer
            {
                Nip = customerDTO.Nip,
                CompanyName = customerDTO.CompanyName,
                
            };
            _context.Customers.Add(customer);
            _context.SaveChanges();
            return customerDTO;
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