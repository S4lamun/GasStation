using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using GasStation.Data;
using GasStation.DTO;
using GasStation.Models;

namespace GasStation.Services
{
    public class PersonService
    {
        private GasStationDbContext _context;

        public PersonService(GasStationDbContext context)
        {
            _context = context;
        }

        public List<PersonDTO> GetAllPersons()
        {
            var persons = _context.People.ToList();
            return persons.Select(p => new PersonDTO
            {
                Pesel = p.Pesel,
                Name = p.Name,
                Surname = p.Surname,
            }).ToList();
        }
        public PersonDTO AddPerson(PersonDTO personDTO)
        {
            if (string.IsNullOrEmpty(personDTO.Pesel) || personDTO.Pesel.Length != 11)
            {
                throw new BusinessLogicException("PESEL must be exactly 11 characters long.");
            }

            if (!Regex.IsMatch(personDTO.Pesel, @"^\d{11}$"))
            {
                throw new BusinessLogicException("PESEL must contain only digits.");
            }

            var person = new Person
            {
                Name = personDTO.Name,
                Surname = personDTO.Surname,
                Pesel = personDTO.Pesel,
            };
            _context.People.Add(person);
            _context.SaveChanges();
            return personDTO;
        }
        public void RemovePerson(PersonDTO personDTO)
        {
            var person = _context.People.Find(personDTO.Pesel);
            if (person != null)
            {
                _context.People.Remove(person);
                _context.SaveChanges();
            }
        }
        public PersonDTO GetPersonByPesel(string pesel)
        {
            var person = _context.People.Find(pesel);

            if (person == null)
            {
                return null; // Person not found
            }
            return new PersonDTO
            {
                Name = person.Name,
                Surname = person.Surname,
                Pesel = person.Pesel,
            };
        }
    }
}