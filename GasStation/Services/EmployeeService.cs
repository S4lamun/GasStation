using GasStation.Data;
using GasStation.Models;
using GasStation.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System;
using System.Data.Entity.Validation;
using System.Text.RegularExpressions; // Upewnij się, że to jest

// Zakładamy, że masz zdefiniowaną tę klasę wyjątków
// np. public class BusinessLogicException : Exception { public BusinessLogicException(string message) : base(message) { } public BusinessLogicException(string message, Exception innerException) : base(message, innerException) { } }
// using GasStation.Exceptions; // Odkomentuj, jeśli masz osobną przestrzeń nazw dla wyjątków


namespace GasStation.Services
{
    public class EmployeeService
    {
        private readonly GasStationDbContext _context;
        // Usunięto referencję do PersonService

        public EmployeeService(GasStationDbContext context) // Usunięto parametr PersonService
        {
            _context = context;
            // Usunięto przypisanie _personService
        }

        // --- Private Helper Methods ---

        // Mapping Employee entity to EmployeeDTO
        private EmployeeDTO MapToDto(Employee employee)
        {
            if (employee == null) return null;

            return new EmployeeDTO
            {
                Pesel = employee.Pesel,
                Name = employee.Name,
                Surname = employee.Surname,
                Login = employee.Login
                // Password excluded (for safety)
            };
        }

        // Reverse mapping
        private Employee MapToEntity(CreateEmployeeDTO employeeDto)
        {
            if (employeeDto == null) return null;

            return new Employee
            {
                Pesel = employeeDto.Pesel,
                Name = employeeDto.Name,
                Surname = employeeDto.Surname,
                Login = employeeDto.Login
                // Hasło przypisywane jest w AddEmployee po walidacji/hashowaniu
            };
        }

        // Metoda walidacji złożoności hasła (Poprawiono Regex)
        private bool IsPasswordComplex(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;

            // Poprawiony Regex
            var regex = new Regex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$");
            return regex.IsMatch(password);
        }


        // --- Public Service Methods ---

        // Pobierz wszystkich pracowników (Bez zmian)
        public List<EmployeeDTO> GetAllEmployees()
        {
            var employees = _context.Employees.ToList();
            return employees.Select(e => MapToDto(e)).ToList();
        }

        // Pobierz pracownika po PESEL (Bez zmian - PESEL jest teraz kluczem głównym Employee)
        public EmployeeDTO GetEmployeeByPesel(string pesel)
        {
            var employee = _context.Employees.Find(pesel);
            return MapToDto(employee);
        }

        // Pobierz pracownika po Loginie (Bez zmian)
        public EmployeeDTO GetEmployeeByLogin(string login)
        {
            var employee = _context.Employees.SingleOrDefault(e => e.Login == login); // Zakładamy unikalność loginu
            return MapToDto(employee);
        }

        // Dodaj nowego pracownika (Logika bez Person, dodano obsługę wyjątków)
        public EmployeeDTO AddEmployee(CreateEmployeeDTO employeeDto)
        {
            // 1. Walidacja (format PESEL w DTO)
            // Walidacja złożoności hasła
            if (!IsPasswordComplex(employeeDto.Password))
            {
                // Załóżmy, że masz taką klasę wyjątków
                throw new BusinessLogicException("Password does not meet complexity requirements (min 8 chars, 1 uppercase, 1 lowercase, 1 digit).");
            }

            // Sprawdzenie unikalności PESELu (jest kluczem głównym, EF by to wykrył, ale jawna walidacja daje lepszy komunikat)
            var existingEmployeeWithPesel = _context.Employees.Any(e => e.Pesel == employeeDto.Pesel);
            if (existingEmployeeWithPesel)
            {
                throw new BusinessLogicException($"Employee with PESEL {employeeDto.Pesel} already exists.");
            }

            // Sprawdzenie unikalności loginu (jeśli nie ma Unique Index w bazie, a nawet jeśli jest, jawna walidacja jest dobra)
            var existingEmployeeWithLogin = _context.Employees.Any(e => e.Login == employeeDto.Login);
            if (existingEmployeeWithLogin)
            {
                throw new BusinessLogicException($"Login '{employeeDto.Login}' is already taken.");
            }

            // 2. Mapuj DTO na encję Employee i PRZYPISZ JAWNE HASŁO (INSECURE - wymagane hashowanie)
            var employee = MapToEntity(employeeDto); // Mapuje Pesel, Name, Surname, Login
            employee.Password = employeeDto.Password; // PRZECHOWUJEMY JAWNY TEKST HASŁA - ZMIEŃ TO NA HASZOWANIE!

            // 3. Dodaj encję Employee i zapisz
            _context.Employees.Add(employee);

            try
            {
                _context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                // Obsługa błędów walidacji EF (np. StringLength)
                var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);

                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = $"Validation errors: {fullErrorMessage}";

                throw new BusinessLogicException(exceptionMessage); // Rzuć własny wyjątek z detalami
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                // Obsługa błędów bazy danych (np. naruszenie unikalności klucza, jeśli nie było walidacji Any())
                // Sprawdź InnerException dla szczegółów bazy danych
                throw new BusinessLogicException($"Database error while adding employee: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                // Ogólna obsługa innych błędów
                throw new BusinessLogicException($"An unexpected error occurred while adding employee: {ex.Message}");
            }

            // 4. Zwróć DTO pracownika (bez hasła)
            return MapToDto(employee);
        }

        // Aktualizuj dane pracownika (Bez zmian logicznych, usunięto odniesienie do Person, dodano obsługę wyjątków)
        public EmployeeDTO UpdateEmployee(EmployeeDTO employeeDto)
        {
            // Nie zmieniamy PESELu - używamy go do znalezienia pracownika
            var employee = _context.Employees.Find(employeeDto.Pesel);
            if (employee == null)
            {
                throw new BusinessLogicException($"Employee with PESEL {employeeDto.Pesel} not found for update.");
            }

            // Sprawdź unikalność loginu, jeśli zmieniono login
            if (employee.Login != employeeDto.Login) // Jeśli login został zmieniony
            {
                // Sprawdź, czy nowy login jest unikalny (wykluczając aktualnego pracownika)
                var existingEmployeeWithLogin = _context.Employees.Any(e => e.Login == employeeDto.Login && e.Pesel != employeeDto.Pesel);
                if (existingEmployeeWithLogin)
                {
                    throw new BusinessLogicException($"Login '{employeeDto.Login}' is already taken.");
                }
            }

            // Aktualizuj dane
            employee.Name = employeeDto.Name;
            employee.Surname = employeeDto.Surname;
            employee.Login = employeeDto.Login;

            // Hasło aktualizujemy oddzielnie metodą ChangePassword

            try
            {
                _context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);

                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = $"Validation errors: {fullErrorMessage}";

                throw new BusinessLogicException(exceptionMessage);
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                throw new BusinessLogicException($"Database error while updating employee: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException($"An unexpected error occurred while updating employee: {ex.Message}");
            }

            return MapToDto(employee); // Zwróć zaktualizowane DTO (bez hasła)
        }

        // Zmień hasło pracownika (Bez zmian logicznych, usunięto odniesienie do Person, dodano obsługę wyjątków)
        public void ChangePassword(string pesel, string oldPassword, string newPassword)
        {
            var employee = _context.Employees.Find(pesel);
            if (employee == null)
            {
                throw new BusinessLogicException($"Employee with PESEL {pesel} not found.");
            }

            // Weryfikuj stare hasło (PORÓWNANIE JAWNEGO TEKSTU - ZMIEŃ TO NA WERYFIKACJĘ HASZU!)
            if (employee.Password != oldPassword)
            {
                throw new BusinessLogicException("Invalid old password.");
            }

            // Walidacja złożoności nowego hasła
            if (!IsPasswordComplex(newPassword))
            {
                throw new BusinessLogicException("New password does not meet complexity requirements (min 8 chars, 1 uppercase, 1 lowercase, 1 digit).");
            }

            // Ustaw nowe hasło (JAWNY TEKST - ZMIEŃ TO NA HASZOWANIE!)
            employee.Password = newPassword;

            try
            {
                _context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);

                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = $"Validation errors: {fullErrorMessage}";

                throw new BusinessLogicException(exceptionMessage);
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                throw new BusinessLogicException($"Database error while changing password for employee: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException($"An unexpected error occurred while changing password for employee: {ex.Message}");
            }
        }

        // Uwierzytelnij dane logowania pracownika (Bez zmian logicznych, usunięto odniesienie do Person)
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

            // 2. Weryfikuj hasło (PORÓWNANIE JAWNEGO TEKSTU - ZMIEŃ TO NA WERYFIKACJĘ HASZU!)
            // To jest punkt, który MUSISZ zmienić na WERYFIKACJĘ HASZU HASŁA
            if (employee.Password != loginDto.Password) // Ta linia porównuje JAWNY TEKST hasła z bazy z JAWNYM TEKSTEM podanym przez użytkownika
            {
                // Hasło nie pasuje
                return null; // Uwierzytelnienie niepowodzenie
            }

            // Usunięto warunek "login i hasło muszą być identyczne"
            // if (loginDto.Login != loginDto.Password) { return null; } // <<< USUŃ TĘ SEKCJE

            // 3. Uwierzytelnienie pomyślne (login znaleziony i hasło pasuje do zapisanego)
            return MapToDto(employee); // Zwróć DTO pracownika (bez hasła)
        }

        // Usuń pracownika po PESEL (Bez zmian logicznych, usunięto odniesienie do Person, dodano obsługę wyjątków)
        public void DeleteEmployee(string pesel)
        {
            // Pobierz pracownika wraz z powiązanymi encjami
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

            // Usuń encję Employee
            _context.Employees.Remove(employee);

            try
            {
                _context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);

                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = $"Validation errors: {fullErrorMessage}";

                throw new BusinessLogicException(exceptionMessage);
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                // Ten catch może złapać np. błąd klucza obcego jeśli nie wychwyciły tego wcześniejsze sprawdzenia .Any()
                throw new BusinessLogicException($"Database error while deleting employee: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException($"An unexpected error occurred while deleting employee: {ex.Message}");
            }
        }
    }

}