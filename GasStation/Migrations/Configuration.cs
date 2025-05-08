using GasStation.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
// using System.Web.Compilation; // Zazwyczaj niepotrzebne w Configuration.cs, można usunąć jeśli nieużywane

namespace GasStation.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<GasStation.Data.GasStationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(GasStation.Data.GasStationDbContext context)
        {
            // Usunięto zewnętrzny warunek if (!context.Customers.Any() && !context.Employees.Any())
            // Aby seeding uruchamiał się przy każdym Update-Database (gdy są migracje)
            // i polegał na AddOrUpdate do zarządzania istniejącymi danymi.

            Trace.TraceInformation("Starting Seed method execution...");

            // --- Dodaj Klientów ---
            // Dodajemy kilku przykładowych klientów
            var customers = new List<Customer>
            {
                new Customer
                {
                    Nip = "0987654321",
                    CompanyName = "NieFajnaFirma",
                },
                new Customer
                {
                   Nip = "1111111111",
                    CompanyName = "SuperFajnaFirma",
                },
                new Customer
                {
                    Nip = "2222222222",
                    CompanyName = "MegaZłaFirma",
                },
                new Customer
                {
                    Nip = "3333333333",
                    CompanyName = "MegaFajnaFirma",
                },
                 new Customer
                {
                    Nip = "1234567890",
                    CompanyName = "FajnaFirma",
                    
                }
            };

            context.Customers.AddOrUpdate(
                c => c.Nip, // Klucz używany do sprawdzania czy obiekt już istnieje
                customers.ToArray()
            );
            Trace.TraceInformation("Customers staged (AddOrUpdate).");

            // --- Dodaj Pracowników ---
            // Dodajemy kilku przykładowych pracowników (Admin i inni)
            var employees = new List<Employee>
            {
                 new Employee // Admin Systemowy - pierwszy pracownik
                {
                    Pesel = "22334455667",
                    Name = "Admin",
                    Surname = "Systemowy",
                    Login = "admin",
                    Password = "SecurePassword123" // Pamiętaj o zabezpieczaniu haseł w rzeczywistej aplikacji!
                },
                 new Employee
                {
                    Pesel = "55667788990",
                    Name = "Janusz",
                    Surname = "Sprzedawca",
                    Login = "janusz",
                    Password = "Password456"
                },
                 new Employee
                {
                    Pesel = "66778899001",
                    Name = "Alicja",
                    Surname = "Kasjerka",
                    Login = "alicja",
                    Password = "AnotherPassword789"
                }
            };

            context.Employees.AddOrUpdate(
                e => e.Pesel, // Klucz używany do sprawdzania czy obiekt już istnieje
                employees.ToArray()
            );
            Trace.TraceInformation("Employees staged (AddOrUpdate).");

            // --- ZAPISZ ZMIANY DLA KLIENTÓW I PRACOWNIKÓW ---
            // Zapisujemy teraz, aby obiekty Employees istniały w bazie i mogły być pobrane
            // do powiązania z FuelPriceHistory.
            try
            {
                Trace.TraceInformation("Saving staged Customers and Employees...");
                context.SaveChanges();
                Trace.TraceInformation("Customers and Employees saved successfully.");
            }
            catch (DbEntityValidationException ex)
            {
                Trace.TraceError("DbEntityValidationException during saving Customers/Employees:");
                Debug.WriteLine("DbEntityValidationException during saving Customers/Employees:");
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    Debug.WriteLine($"Entity of type \"{validationErrors.Entry.Entity.GetType().Name}\" in state \"{validationErrors.Entry.State}\" has validation errors:");
                    Trace.TraceError($"Entity of type \"{validationErrors.Entry.Entity.GetType().Name}\" in state \"{validationErrors.Entry.State}\" has validation errors:");
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Debug.WriteLine($"- Property: \"{validationError.PropertyName}\", Error: \"{validationError.ErrorMessage}\"");
                        Trace.TraceError($"- Property: \"{validationError.PropertyName}\", Error: \"{validationError.ErrorMessage}\"");
                    }
                }
                Debug.WriteLine("-----------------------------------------------------");
                throw; // Rethrow after logging
            }
            catch (Exception ex)
            {
                Trace.TraceError("An unexpected error occurred during saving Customers/Employees: {0}", ex.Message);
                Debug.WriteLine("An unexpected error occurred during saving Customers/Employees: " + ex.Message);
                Debug.WriteLine(ex.StackTrace);
                throw; // Rethrow
            }


            // --- Dodaj Typy Paliw ---
            // Dodajemy podstawowe typy paliw
            var fuels = new List<Fuel>
            {
                new Fuel { FuelName = "Benzyna 95", DistributorNumber = "Dystr1" },
                new Fuel { FuelName = "Diesel", DistributorNumber = "Dystr2" },
                new Fuel { FuelName = "LPG", DistributorNumber = "Dystr3" },
                
            };

            context.Fuels.AddOrUpdate(
                f => f.FuelName, // Klucz do sprawdzania, czy paliwo o danej nazwie już istnieje
                fuels.ToArray()
            );
            Trace.TraceInformation("Fuels staged (AddOrUpdate).");

            // --- ZAPISZ ZMIANY DLA PALIW ---
            // Zapisujemy teraz, aby obiekty Fuel istniały w bazie i mogły być pobrane
            // do powiązania z FuelPriceHistory.
            try
            {
                Trace.TraceInformation("Saving staged Fuels...");
                context.SaveChanges();
                Trace.TraceInformation("Fuels saved successfully.");
            }
            catch (DbEntityValidationException ex)
            {
                Trace.TraceError("DbEntityValidationException during saving Fuels:");
                Debug.WriteLine("DbEntityValidationException during saving Fuels:");
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    Debug.WriteLine($"Entity of type \"{validationErrors.Entry.Entity.GetType().Name}\" in state \"{validationErrors.Entry.State}\" has validation errors:");
                    Trace.TraceError($"Entity of type \"{validationErrors.Entry.Entity.GetType().Name}\" in state \"{validationErrors.Entry.State}\" has validation errors:");
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Debug.WriteLine($"- Property: \"{validationError.PropertyName}\", Error: \"{validationError.ErrorMessage}\"");
                        Trace.TraceError($"- Property: \"{validationError.PropertyName}\", Error: \"{validationError.ErrorMessage}\"");
                    }
                }
                Debug.WriteLine("-----------------------------------------------------");
                throw; // Rethrow after logging
            }
            catch (Exception ex)
            {
                Trace.TraceError("An unexpected error occurred during saving Fuels: {0}", ex.Message);
                Debug.WriteLine("An unexpected error occurred during saving Fuels: " + ex.Message);
                Debug.WriteLine(ex.StackTrace);
                throw; // Rethrow
            }


            // --- Dodaj Historię Cen Paliw ---
            // Teraz, gdy Pracownicy i Paliwa są zapisani w bazie, możemy ich bezpiecznie pobrać
            // i użyć ich do zbudowania relacji w FuelPriceHistory.
            Trace.TraceInformation("Retrieving Employees and Fuels for relationships...");
            // Użyj Single() - zakłada, że rekord ZAWSZE istnieje po poprzednich AddOrUpdate i SaveChanges.
            // Jeśli może nie istnieć (co w seedingu jest mało prawdopodobne dla danych podstawowych),
            // użyj SingleOrDefault() i sprawdź czy nie jest nullem.
            var adminEmployee = context.Employees.Single(e => e.Pesel == "22334455667");
            var januszEmployee = context.Employees.Single(e => e.Pesel == "55667788990");

            var benzyna95 = context.Fuels.Single(f => f.FuelName == "Benzyna 95");
            var diesel = context.Fuels.Single(f => f.FuelName == "Diesel");
            var lpg = context.Fuels.Single(f => f.FuelName == "LPG");
            Trace.TraceInformation("Employees and Fuels retrieved.");


            var priceHistories = new List<FuelPriceHistory>
            {
                // Historia cen dla Benzyny 95 (dwie zmiany ceny)
                new FuelPriceHistory
                {
                    Fuel = benzyna95, // Powiąż z obiektem paliwa
                    Price = 6.50m, // Użyj 'm' dla typu decimal
                    DateFrom = new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Utc), // Data rozpoczęcia obowiązywania ceny
                    DateTo = new DateTime(2025, 5, 7, 23, 59, 59, DateTimeKind.Utc), // Data zakończenia obowiązywania ceny
                    Employee = adminEmployee // Powiąż z obiektem pracownika, który ustalił cenę
                },
                 new FuelPriceHistory
                {
                    Fuel = benzyna95,
                    Price = 6.70m,
                    DateFrom = new DateTime(2025, 5, 8, 0, 0, 0, DateTimeKind.Utc), // Nowa cena od tej daty
                    DateTo = null, // null oznacza cenę aktualnie obowiązującą
                    Employee = januszEmployee // Przykładowa zmiana ceny przez innego pracownika
                },

                // Historia cen dla Diesla (dwie zmiany ceny)
                 new FuelPriceHistory
                {
                    Fuel = diesel,
                    Price = 6.30m,
                    DateFrom = new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Utc),
                    DateTo = new DateTime(2025, 5, 7, 23, 59, 59, DateTimeKind.Utc),
                    Employee = adminEmployee
                },
                 new FuelPriceHistory
                {
                    Fuel = diesel,
                    Price = 6.45m,
                    DateFrom = new DateTime(2025, 5, 8, 0, 0, 0, DateTimeKind.Utc),
                    DateTo = null,
                    Employee = adminEmployee
                },

                // Historia cen dla LPG (jedna cena)
                 new FuelPriceHistory
                {
                    Fuel = lpg,
                    Price = 2.80m,
                    DateFrom = new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Utc),
                    DateTo = null, // Cena aktualna od tej daty
                    Employee = januszEmployee // Przykładowa cena ustalona przez innego pracownika
                },

                
                
            };

            // Dodaj lub zaktualizuj historię cen. Kluczem identyfikującym historyczny wpis
            // jest kombinacja ID paliwa i daty rozpoczęcia obowiązywania ceny (DateFrom).
            // Zapewnia to, że dla tego samego paliwa, zmiana ceny od innej daty jest
            // dodawana jako nowy rekord historii, a nie aktualizuje poprzedniego.
            context.FuelPriceHistories.AddOrUpdate(
                h => new { h.FuelId, h.DateFrom }, // Klucz złożony: ID paliwa i data rozpoczęcia
                priceHistories.ToArray()
            );
            Trace.TraceInformation("Fuel price history staged (AddOrUpdate).");


            // --- ZAPISZ WSZYSTKIE POZOSTAŁE ZMIANY ---
            // Ten SaveChanges zapisze przede wszystkim FuelPriceHistory.
            try
            {
                Trace.TraceInformation("Saving final staged changes...");
                context.SaveChanges();
                Trace.TraceInformation("Final changes saved successfully.");
            }
            catch (DbEntityValidationException ex)
            {
                // Logowanie błędów walidacji encji - bardzo przydatne przy problemach z Seed
                Trace.TraceError("DbEntityValidationException during final SaveChanges:");
                Debug.WriteLine("DbEntityValidationException during final SaveChanges:");
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    Debug.WriteLine($"Entity of type \"{validationErrors.Entry.Entity.GetType().Name}\" in state \"{validationErrors.Entry.State}\" has validation errors:");
                    Trace.TraceError($"Entity of type \"{validationErrors.Entry.Entity.GetType().Name}\" in state \"{validationErrors.Entry.State}\" has validation errors:");
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Debug.WriteLine($"- Property: \"{validationError.PropertyName}\", Error: \"{validationError.ErrorMessage}\"");
                        Trace.TraceError($"- Property: \"{validationError.PropertyName}\", Error: \"{validationError.ErrorMessage}\"");
                    }
                }
                Debug.WriteLine("-----------------------------------------------------");
                throw; // Ponownie rzuć wyjątek po zalogowaniu błędów
            }
            catch (Exception ex)
            {
                // Logowanie innych błędów
                Trace.TraceError("An unexpected error occurred during final SaveChanges: {0}", ex.Message);
                Debug.WriteLine("An unexpected error occurred during final SaveChanges: " + ex.Message);
                Debug.WriteLine(ex.StackTrace);
                throw; // Ponownie rzuć wyjątek
            }

            Trace.TraceInformation("Seed method execution finished.");
        }
    }
}