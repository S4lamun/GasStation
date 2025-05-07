using System.Data.Entity;
using GasStation.Models;

namespace GasStation.Data
{
    public class GasStationDbContext : DbContext
    {
        public GasStationDbContext() : base("name=DefaultConnection")
        {
        }

        // Usunięto DbSet<Person>
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
            // Usunięto konfigurację mapowania dla Person
            // modelBuilder.Entity<Person>().ToTable("People");

            // Konfiguracje dla Customers i Employees pozostają,
            // ponieważ są teraz niezależnymi tabelami z własnymi kluczami.
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Employee>().ToTable("Employees");

            // Usunięto poprzednie konfiguracje HasRequired/WithOptional, które powodują problemy
            // (te były związane z mapowaniem TPT i nie są już potrzebne)
            // modelBuilder.Entity<Employee>()
            //     .HasRequired(e => e.Person)
            //     .WithOptional()
            //     .Map(m => m.MapKey("Pesel"));

            // modelBuilder.Entity<Customer>()
            //     .HasRequired(c => c.Person)
            //     .WithOptional()
            //     .Map(m => m.MapKey("Pesel"));

            // Te linie konfiguracyjne dla opcjonalnych kluczy obcych są poprawne i pozostają
            modelBuilder.Entity<OrderSpecification>()
                .HasOptional(os => os.RefuelingEntry)
                .WithMany(re => re.OrderSpecifications)
                .HasForeignKey(os => os.RefuelingEntryId);

            modelBuilder.Entity<OrderSpecification>()
                .HasOptional(os => os.Order)
                .WithMany(o => o.OrderSpecifications)
                .HasForeignKey(os => os.OrderId);
        }
    }
}