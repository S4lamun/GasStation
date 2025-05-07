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
                Pesel = c.Pesel,
                Name = c.Name,
                Surname = c.Surname,
                CardNumber = c.CardNumber,
                Company = c.Company
            }).ToList();
        }

        public CustomerDTO AddCustomer(CustomerDTO customerDTO)
        {
            var customer = new Customer
            {
                Pesel = customerDTO.Pesel,
                Name = customerDTO.Name,
                Surname = customerDTO.Surname,
                CardNumber = customerDTO.CardNumber,
                Company = customerDTO.Company
            };
            _context.Customers.Add(customer);
            _context.SaveChanges();
            return customerDTO;
        }

        public void RemoveCustomer(CustomerDTO customerDTO)
        {
            var customer = _context.Customers.Find(customerDTO.Pesel);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                _context.SaveChanges();
            }
        }

        public CustomerDTO GetCustomerByPesel(string pesel)
        {
            var customer = _context.Customers.Find(pesel);
            if (customer != null)
            {
                return new CustomerDTO
                {
                    Pesel = customer.Pesel,
                    Name = customer.Name,
                    Surname = customer.Surname,
                    CardNumber = customer.CardNumber,
                    Company = customer.Company
                };
            }
            return null; // Customer not found
        }
    }
}