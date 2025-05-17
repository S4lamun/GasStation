using GasStation.Data;
using GasStation.Models;
using GasStation.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System;
using System.Data.Entity.Validation;
using System.Text.RegularExpressions; 


namespace GasStation.Services
{
    public class EmployeeService
    {
        private readonly GasStationDbContext _context;
        
        public EmployeeService(GasStationDbContext context)         {
            _context = context;
                    }

        
                private EmployeeDTO MapToDto(Employee employee)
        {
            if (employee == null) return null;

            return new EmployeeDTO
            {
                Pesel = employee.Pesel,
                Name = employee.Name,
                Surname = employee.Surname,
                Login = employee.Login
                            };
        }

                private Employee MapToEntity(CreateEmployeeDTO employeeDto)
        {
            if (employeeDto == null) return null;

            return new Employee
            {
                Pesel = employeeDto.Pesel,
                Name = employeeDto.Name,
                Surname = employeeDto.Surname,
                Login = employeeDto.Login
                            };
        }

                private bool IsPasswordComplex(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;

                        var regex = new Regex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$");
            return regex.IsMatch(password);
        }


        
                public List<EmployeeDTO> GetAllEmployees()
        {
            var employees = _context.Employees.ToList();
            return employees.Select(e => MapToDto(e)).ToList();
        }

                public EmployeeDTO GetEmployeeByPesel(string pesel)
        {
            var employee = _context.Employees.Find(pesel);
            return MapToDto(employee);
        }

                public EmployeeDTO GetEmployeeByLogin(string login)
        {
            var employee = _context.Employees.SingleOrDefault(e => e.Login == login);             return MapToDto(employee);
        }

                public EmployeeDTO AddEmployee(CreateEmployeeDTO employeeDto)
        {
                                    if (!IsPasswordComplex(employeeDto.Password))
            {
                                throw new BusinessLogicException("Password does not meet complexity requirements (min 8 chars, 1 uppercase, 1 lowercase, 1 digit).");
            }

                        var existingEmployeeWithPesel = _context.Employees.Any(e => e.Pesel == employeeDto.Pesel);
            if (existingEmployeeWithPesel)
            {
                throw new BusinessLogicException($"Employee with PESEL {employeeDto.Pesel} already exists.");
            }

                        var existingEmployeeWithLogin = _context.Employees.Any(e => e.Login == employeeDto.Login);
            if (existingEmployeeWithLogin)
            {
                throw new BusinessLogicException($"Login '{employeeDto.Login}' is already taken.");
            }

                        var employee = MapToEntity(employeeDto);             employee.Password = employeeDto.Password; 
                        _context.Employees.Add(employee);

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

                throw new BusinessLogicException(exceptionMessage);             }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                                                throw new BusinessLogicException($"Database error while adding employee: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                                throw new BusinessLogicException($"An unexpected error occurred while adding employee: {ex.Message}");
            }

                        return MapToDto(employee);
        }

                public EmployeeDTO UpdateEmployee(EmployeeDTO employeeDto)
        {
                        var employee = _context.Employees.Find(employeeDto.Pesel);
            if (employee == null)
            {
                throw new BusinessLogicException($"Employee with PESEL {employeeDto.Pesel} not found for update.");
            }

                        if (employee.Login != employeeDto.Login)             {
                                var existingEmployeeWithLogin = _context.Employees.Any(e => e.Login == employeeDto.Login && e.Pesel != employeeDto.Pesel);
                if (existingEmployeeWithLogin)
                {
                    throw new BusinessLogicException($"Login '{employeeDto.Login}' is already taken.");
                }
            }

                        employee.Name = employeeDto.Name;
            employee.Surname = employeeDto.Surname;
            employee.Login = employeeDto.Login;

            
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

            return MapToDto(employee);         }

                public void ChangePassword(string pesel, string oldPassword, string newPassword)
        {
            var employee = _context.Employees.Find(pesel);
            if (employee == null)
            {
                throw new BusinessLogicException($"Employee with PESEL {pesel} not found.");
            }

                        if (employee.Password != oldPassword)
            {
                throw new BusinessLogicException("Invalid old password.");
            }

                        if (!IsPasswordComplex(newPassword))
            {
                throw new BusinessLogicException("New password does not meet complexity requirements (min 8 chars, 1 uppercase, 1 lowercase, 1 digit).");
            }

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

                        public EmployeeDTO Authenticate(EmployeeLoginDTO loginDto)
        {
                        var employee = _context.Employees.SingleOrDefault(e => e.Login == loginDto.Login);

            if (employee == null)
            {
                                return null;             }

                                    if (employee.Password != loginDto.Password)             {
                                return null;             }

                        
                        return MapToDto(employee);         }

                public void DeleteEmployee(string pesel)
        {
                        var employee = _context.Employees
                                   .Include(e => e.HistoriaCenPaliwZmiany)
                                   .Include(e => e.Orders)
                                   .SingleOrDefault(e => e.Pesel == pesel);

            if (employee == null)
            {
                return;             }

                        if (employee.HistoriaCenPaliwZmiany != null && employee.HistoriaCenPaliwZmiany.Any())
            {
                throw new BusinessLogicException($"Cannot remove employee {employee.Pesel} as they are linked to fuel price history changes.");
            }
            if (employee.Orders != null && employee.Orders.Any())
            {
                throw new BusinessLogicException($"Cannot remove employee {employee.Pesel} as they are linked to orders.");
            }

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
                                throw new BusinessLogicException($"Database error while deleting employee: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException($"An unexpected error occurred while deleting employee: {ex.Message}");
            }
        }
    }

}