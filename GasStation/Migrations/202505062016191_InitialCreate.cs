namespace GasStation.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.People",
                c => new
                    {
                        Pesel = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 50),
                        Surname = c.String(nullable: false, maxLength: 50),
                        Customer_Pesel = c.String(maxLength: 128),
                        Employee_Pesel = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Pesel)
                .ForeignKey("dbo.Customers", t => t.Customer_Pesel)
                .ForeignKey("dbo.Employees", t => t.Employee_Pesel)
                .Index(t => t.Customer_Pesel)
                .Index(t => t.Employee_Pesel);
            
            CreateTable(
                "dbo.FuelPriceHistory",
                c => new
                    {
                        FuelPriceHistoryId = c.Int(nullable: false, identity: true),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DateFrom = c.DateTime(nullable: false),
                        DateTo = c.DateTime(nullable: false),
                        FuelId = c.Int(nullable: false),
                        EmployeeId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.FuelPriceHistoryId)
                .ForeignKey("dbo.Employees", t => t.EmployeeId)
                .ForeignKey("dbo.Fuel", t => t.FuelId, cascadeDelete: true)
                .Index(t => t.FuelId)
                .Index(t => t.EmployeeId);
            
            CreateTable(
                "dbo.Fuel",
                c => new
                    {
                        FuelId = c.Int(nullable: false, identity: true),
                        FuelName = c.String(nullable: false, maxLength: 50),
                        DistributorNumber = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.FuelId);
            
            CreateTable(
                "dbo.RefuelingEntry",
                c => new
                    {
                        RefuelingEntryId = c.Int(nullable: false, identity: true),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OrderId = c.Int(nullable: false),
                        FuelId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.RefuelingEntryId)
                .ForeignKey("dbo.Fuel", t => t.FuelId, cascadeDelete: true)
                .ForeignKey("dbo.Order", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId)
                .Index(t => t.FuelId);
            
            CreateTable(
                "dbo.Order",
                c => new
                    {
                        OrderId = c.Int(nullable: false, identity: true),
                        OrderDate = c.DateTime(nullable: false),
                        PaymentType = c.String(nullable: false, maxLength: 50),
                        CustomerId = c.String(maxLength: 128),
                        EmployeeId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.OrderId)
                .ForeignKey("dbo.Customers", t => t.CustomerId)
                .ForeignKey("dbo.Employees", t => t.EmployeeId)
                .Index(t => t.CustomerId)
                .Index(t => t.EmployeeId);
            
            CreateTable(
                "dbo.OrderSpecification",
                c => new
                    {
                        OrderSpecificationId = c.Int(nullable: false, identity: true),
                        ProductId = c.Int(nullable: false),
                        Ilosc = c.Int(nullable: false),
                        RefuelingEntryId = c.Int(),
                        OrderId = c.Int(),
                    })
                .PrimaryKey(t => t.OrderSpecificationId)
                .ForeignKey("dbo.Order", t => t.OrderId)
                .ForeignKey("dbo.Product", t => t.ProductId, cascadeDelete: true)
                .ForeignKey("dbo.RefuelingEntry", t => t.RefuelingEntryId)
                .Index(t => t.ProductId)
                .Index(t => t.RefuelingEntryId)
                .Index(t => t.OrderId);
            
            CreateTable(
                "dbo.Product",
                c => new
                    {
                        ProductId = c.Int(nullable: false, identity: true),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.ProductId);
            
            CreateTable(
                "dbo.Customers",
                c => new
                    {
                        Pesel1 = c.String(nullable: false, maxLength: 128),
                        Pesel = c.String(),
                        CardNumber = c.String(maxLength: 20),
                        Company = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Pesel1)
                .ForeignKey("dbo.People", t => t.Pesel1)
                .Index(t => t.Pesel1);
            
            CreateTable(
                "dbo.Employees",
                c => new
                    {
                        Pesel1 = c.String(nullable: false, maxLength: 128),
                        Pesel = c.String(),
                        Login = c.String(nullable: false, maxLength: 50),
                        Password = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Pesel1)
                .ForeignKey("dbo.People", t => t.Pesel1)
                .Index(t => t.Pesel1);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Employees", "Pesel1", "dbo.People");
            DropForeignKey("dbo.Customers", "Pesel1", "dbo.People");
            DropForeignKey("dbo.People", "Employee_Pesel", "dbo.Employees");
            DropForeignKey("dbo.People", "Customer_Pesel", "dbo.Customers");
            DropForeignKey("dbo.FuelPriceHistory", "FuelId", "dbo.Fuel");
            DropForeignKey("dbo.RefuelingEntry", "OrderId", "dbo.Order");
            DropForeignKey("dbo.OrderSpecification", "RefuelingEntryId", "dbo.RefuelingEntry");
            DropForeignKey("dbo.OrderSpecification", "ProductId", "dbo.Product");
            DropForeignKey("dbo.OrderSpecification", "OrderId", "dbo.Order");
            DropForeignKey("dbo.Order", "EmployeeId", "dbo.Employees");
            DropForeignKey("dbo.Order", "CustomerId", "dbo.Customers");
            DropForeignKey("dbo.RefuelingEntry", "FuelId", "dbo.Fuel");
            DropForeignKey("dbo.FuelPriceHistory", "EmployeeId", "dbo.Employees");
            DropIndex("dbo.Employees", new[] { "Pesel1" });
            DropIndex("dbo.Customers", new[] { "Pesel1" });
            DropIndex("dbo.OrderSpecification", new[] { "OrderId" });
            DropIndex("dbo.OrderSpecification", new[] { "RefuelingEntryId" });
            DropIndex("dbo.OrderSpecification", new[] { "ProductId" });
            DropIndex("dbo.Order", new[] { "EmployeeId" });
            DropIndex("dbo.Order", new[] { "CustomerId" });
            DropIndex("dbo.RefuelingEntry", new[] { "FuelId" });
            DropIndex("dbo.RefuelingEntry", new[] { "OrderId" });
            DropIndex("dbo.FuelPriceHistory", new[] { "EmployeeId" });
            DropIndex("dbo.FuelPriceHistory", new[] { "FuelId" });
            DropIndex("dbo.People", new[] { "Employee_Pesel" });
            DropIndex("dbo.People", new[] { "Customer_Pesel" });
            DropTable("dbo.Employees");
            DropTable("dbo.Customers");
            DropTable("dbo.Product");
            DropTable("dbo.OrderSpecification");
            DropTable("dbo.Order");
            DropTable("dbo.RefuelingEntry");
            DropTable("dbo.Fuel");
            DropTable("dbo.FuelPriceHistory");
            DropTable("dbo.People");
        }
    }
}
