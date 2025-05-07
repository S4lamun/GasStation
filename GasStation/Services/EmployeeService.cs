using GasStation.Data;
using GasStation.Models;
using GasStation.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System;



namespace GasStation.Services
{
    public class EmployeeService
    {
        private readonly GasStationDbContext _context;
        private readonly PersonService _personService; 

        public EmployeeService(GasStationDbContext context, PersonService personService)
        {
            _context = context;
            _personService = personService;
        }

        // --- Private Helper Methods ---

        // Maping Employee entity to EmployeeDTO
        private EmployeeDTO MapToDto(Employee employee)
        {
            if (employee == null) return null;

            return new EmployeeDTO
            {
                Pesel = employee.Pesel,
                Name = employee.Name,
                Surname = employee.Surname,
                Login = employee.Login
                // Password exluded (for safety)
            };
        }

        //Reverse maping
        private Employee MapToEntity(CreateEmployeeDTO employeeDto)
        {
            if (employeeDto == null) return null;

            return new Employee
            {
                Pesel = employeeDto.Pesel,
                Name = employeeDto.Name,
                Surname = employeeDto.Surname,
                Login = employeeDto.Login,
                
            };
        }

        // Metoda walidacji złożoności hasła (NOWE)
        private bool IsPasswordComplex(string password)
        {
            if (string.IsNullOrEmpty(password)) return false; // Hasło Required jest w DTO, ale na wszelki wypadek

            var regex = new System.Text.RegularExpressions.Regex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$");
            return regex.IsMatch(password);
        }


        // --- Public Service Methods ---

        // Pobierz wszystkich pracowników
        public List<EmployeeDTO> GetAllEmployees()
        {
            var employees = _context.Employees.ToList();
            return employees.Select(e => MapToDto(e)).ToList();
        }

        // Pobierz pracownika po PESEL (klucz TPT)
        public EmployeeDTO GetEmployeeByPesel(string pesel)
        {
            var employee = _context.Employees.Find(pesel);
            return MapToDto(employee);
        }

        // Pobierz pracownika po Loginie (do uwierzytelniania)
        public EmployeeDTO GetEmployeeByLogin(string login)
        {
            var employee = _context.Employees.SingleOrDefault(e => e.Login == login); // Zakładamy unikalność loginu
            return MapToDto(employee);
        }


        // Dodaj nowego pracownika
        public EmployeeDTO AddEmployee(CreateEmployeeDTO employeeDto)
        {
            // 1. Walidacja formatu PESEL i unikalności loginu (częściowo w DTO, reszta w Service)
            // Walidacja złożoności hasła (NOWE - sprawdzamy ponownie tutaj)
            if (!IsPasswordComplex(employeeDto.Password))
            {
                throw new BusinessLogicException("Password does not meet complexity requirements (min 8 chars, 1 uppercase, 1 lowercase, 1 digit).");
            }

            // Sprawdzenie unikalności loginu (jeśli nie ma Unique Index w bazie)
            var existingEmployeeWithLogin = _context.Employees.Any(e => e.Login == employeeDto.Login);
            if (existingEmployeeWithLogin)
            {
                throw new BusinessLogicException($"Login '{employeeDto.Login}' is already taken.");
            }


            // 2. Tworzenie/linkowanie encji Person (logika TPT)
            var existingPerson = _context.People.Find(employeeDto.Pesel);
            if (existingPerson != null)
            {
                // Osoba o tym PESELu istnieje, sprawdź czy nie jest już Pracownikiem
                var existingEmployee = _context.Employees.Find(employeeDto.Pesel);
                if (existingEmployee != null)
                {
                    throw new BusinessLogicException($"Person with PESEL {employeeDto.Pesel} is already registered as an Employee.");
                }
                // Uaktualnij dane Osoby jeśli istnieją
                existingPerson.Name = employeeDto.Name;
                existingPerson.Surname = employeeDto.Surname;
                _context.Entry(existingPerson).State = EntityState.Modified;

            }
            else
            {
                // Osoba nie istnieje, utwórz ją
                var newPerson = new Person
                {
                    Pesel = employeeDto.Pesel,
                    Name = employeeDto.Name,
                    Surname = employeeDto.Surname
                };
                _context.People.Add(newPerson);
            }


            // 3. Mapuj DTO na encję Employee i PRZYPISZ JAWNE HASŁO (MODYFIKACJA + INSECURE!)
            var employee = MapToEntity(employeeDto); // Mapuje Pesel, Name, Surname, Login
            employee.Password = employeeDto.Password; // PRZECHOWUJEMY JAWNY TEKST HASŁA (INSECURE!)


            // 4. Dodaj encję Employee i zapisz
            _context.Employees.Add(employee);
            _context.SaveChanges();

            // 5. Zwróć DTO pracownika (bez hasła)
            return MapToDto(employee);
        }

        // Aktualizuj dane pracownika (MODYFIKACJA - bez hasła)
        public EmployeeDTO UpdateEmployee(EmployeeDTO employeeDto)
        {
            // Nie zmieniamy PESELu
            var employee = _context.Employees.Find(employeeDto.Pesel);
            if (employee == null)
            {
                throw new BusinessLogicException($"Employee with PESEL {employeeDto.Pesel} not found for update.");
            }

            // Sprawdź unikalność loginu, jeśli zmieniono login
            if (employee.Login != employeeDto.Login) // Jeśli login został zmieniony
            {
                var existingEmployeeWithLogin = _context.Employees.Any(e => e.Login == employeeDto.Login && e.Pesel != employeeDto.Pesel); // Sprawdź, czy nowy login jest unikalny (wykluczając aktualnego pracownika)
                if (existingEmployeeWithLogin)
                {
                    throw new BusinessLogicException($"Login '{employeeDto.Login}' is already taken.");
                }
            }


            // Aktualizuj dane (bez hasła)
            employee.Name = employeeDto.Name; // Dziedziczone
            employee.Surname = employeeDto.Surname; // Dziedziczone
            employee.Login = employeeDto.Login; // Pole loginu

            // Hasło aktualizujemy oddzielnie metodą ChangePassword

            _context.SaveChanges();

            return MapToDto(employee); // Zwróć zaktualizowane DTO (bez hasła)
        }

        // Zmień hasło pracownika 
        public void ChangePassword(string pesel, string oldPassword, string newPassword)
        {
            var employee = _context.Employees.Find(pesel);
            if (employee == null)
            {
                throw new BusinessLogicException($"Employee with PESEL {pesel} not found.");
            }

            // Weryfikuj stare hasło (PORÓWNANIE JAWNEGO TEKSTU - INSECURE!)
            if (employee.Password != oldPassword)
            {
                throw new BusinessLogicException("Invalid old password.");
            }

            // Walidacja złożoności nowego hasła (NOWE)
            if (!IsPasswordComplex(newPassword))
            {
                throw new BusinessLogicException("New password does not meet complexity requirements (min 8 chars, 1 uppercase, 1 lowercase, 1 digit).");
            }

            // Ustaw nowe hasło (JAWNY TEKST - INSECURE!)
            employee.Password = newPassword;

            _context.SaveChanges();
        }

        // Uwierzytelnij dane logowania pracownika (MODYFIKACJA - JAWNY TEKST + WARUNEK login == hasło + INSECURE!)
        // Zwraca EmployeeDto jeśli sukces, null jeśli niepowodzenie (login zły, hasło złe, login != hasło)
        public EmployeeDTO Authenticate(EmployeeLoginDTO loginDto)
        {
            // 1. Znajdź pracownika po loginie
            var employee = _context.Employees.SingleOrDefault(e => e.Login == loginDto.Login);

            if (employee == null)
            {
                // Pracownik o takim loginie nie znaleziony
                return null; // Uwierzytelnienie niepowodzenie
            }

            // 2. Weryfikuj hasło (PORÓWNANIE JAWNEGO TEKSTU - INSECURE!)
            if (employee.Password != loginDto.Password)
            {
                // Hasło nie pasuje
                return null; // Uwierzytelnienie niepowodzenie
            }

            // 3. WALIDUJ NOWY WARUNEK BIZNESOWY: login i hasło muszą być identyczne (NOWE)
            if (loginDto.Login != loginDto.Password)
            {
                // Login i hasło pasują do zapisanego, ale nie są IDENTYCZNE - warunek biznesowy nie spełniony
                return null; // Uwierzytelnienie niepowodzenie
                             // Lub throw new BusinessLogicException("Login and password must be the same.");
            }


            // 4. Uwierzytelnienie pomyślne
            return MapToDto(employee); // Zwróć DTO pracownika (bez hasła)
        }


        // Usuń pracownika po PESEL (bez zmian względem poprzedniej wersji)
        public void DeleteEmployee(string pesel)
        {
            var employee = _context.Employees
                             .Include(e => e.HistoriaCenPaliwZmiany)
                             .Include(e => e.Orders)
                             .SingleOrDefault(e => e.Pesel == pesel);

            if (employee == null)
            {
                return; // Nic nie robimy jeśli nie znaleziono
            }

            // Walidacja biznesowa: Czy pracownik nie jest powiązany z transakcjami
            if (employee.HistoriaCenPaliwZmiany != null && employee.HistoriaCenPaliwZmiany.Any())
            {
                throw new BusinessLogicException($"Cannot remove employee {employee.Pesel} as they are linked to fuel price history changes.");
            }
            if (employee.Orders != null && employee.Orders.Any())
            {
                throw new BusinessLogicException($"Cannot remove employee {employee.Pesel} as they are linked to orders.");
            }

            // Usuń encję Employee (z tabeli Employees w TPT)
            _context.Employees.Remove(employee);

            _context.SaveChanges();
        }
    }

}