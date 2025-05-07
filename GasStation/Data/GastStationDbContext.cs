// GasStationDbContext.cs
using System.Data.Entity; // Upewnij się, że używasz System.Data.Entity
using GasStation.Models;

namespace GasStation.Data
{
    public class GasStationDbContext : DbContext
    {
        public GasStationDbContext() : base("name=DefaultConnection")
        {
        }

        public DbSet<Person> People { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Fuel> Fuels { get; set; }
        public DbSet<FuelPriceHistory> FuelPriceHistories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<RefuelingEntry> RefuelingEntries { get; set; }
        public DbSet<OrderSpecification> OrderSpecifications { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Konfiguracja mapowania TPT
            modelBuilder.Entity<Person>().ToTable("People");
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Employee>().ToTable("Employees");

            // Te linie konfiguracyjne dla opcjonalnych kluczy obcych są poprawne i pozostają
            modelBuilder.Entity<OrderSpecification>()
                .HasOptional(os => os.RefuelingEntry)
                .WithMany(re => re.OrderSpecifications)
                .HasForeignKey(os => os.RefuelingEntryId);

            modelBuilder.Entity<OrderSpecification>()
                .HasOptional(os => os.Order)
                .WithMany(o => o.OrderSpecifications)
                .HasForeignKey(os => os.OrderId);

            // Pamiętaj, aby usunąć base.OnModelCreating(modelBuilder); w EF6, jeśli implementujesz OnModelCreating
            // aby uniknąć podwójnej konfiguracji.
        }
    }
}