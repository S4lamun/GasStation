using System.Data.Entity;
using GasStation.Models;

namespace GasStation.Data
{
    public class GasStationDbContext : DbContext
    {
        public GasStationDbContext() : base("name=DefaultConnection")
        {
        }

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
                        
                                    modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Employee>().ToTable("Employees");

                                                                        
                                                
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