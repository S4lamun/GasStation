namespace GasStation.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Customers",
                c => new
                    {
                        Nip = c.String(nullable: false, maxLength: 128),
                        CompanyName = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Nip);
            
            CreateTable(
                "dbo.Order",
                c => new
                    {
                        OrderId = c.Int(nullable: false, identity: true),
                        OrderDate = c.DateTime(nullable: false),
                        PaymentType = c.String(nullable: false, maxLength: 50),
                        CustomerPesel = c.String(maxLength: 128),
                        EmployeePesel = c.String(maxLength: 128),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.OrderId)
                .ForeignKey("dbo.Customers", t => t.CustomerPesel)
                .ForeignKey("dbo.Employees", t => t.EmployeePesel)
                .Index(t => t.CustomerPesel)
                .Index(t => t.EmployeePesel);
            
            CreateTable(
                "dbo.Employees",
                c => new
                    {
                        Pesel = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 50),
                        Surname = c.String(nullable: false, maxLength: 50),
                        Login = c.String(nullable: false, maxLength: 50),
                        Password = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Pesel);
            
            CreateTable(
                "dbo.FuelPriceHistory",
                c => new
                    {
                        FuelPriceHistoryId = c.Int(nullable: false, identity: true),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DateFrom = c.DateTime(nullable: false),
                        DateTo = c.DateTime(),
                        FuelId = c.Int(nullable: false),
                        EmployeePesel = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.FuelPriceHistoryId)
                .ForeignKey("dbo.Employees", t => t.EmployeePesel)
                .ForeignKey("dbo.Fuel", t => t.FuelId, cascadeDelete: true)
                .Index(t => t.FuelId)
                .Index(t => t.EmployeePesel);
            
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
                        PriceAtSale = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OrderId = c.Int(nullable: false),
                        FuelId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.RefuelingEntryId)
                .ForeignKey("dbo.Fuel", t => t.FuelId, cascadeDelete: true)
                .ForeignKey("dbo.Order", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId)
                .Index(t => t.FuelId);
            
            CreateTable(
                "dbo.OrderSpecification",
                c => new
                    {
                        OrderSpecificationId = c.Int(nullable: false, identity: true),
                        ProductId = c.Int(nullable: false),
                        PriceAtSale = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Quantity = c.Int(nullable: false),
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Order", "EmployeePesel", "dbo.Employees");
            DropForeignKey("dbo.OrderSpecification", "RefuelingEntryId", "dbo.RefuelingEntry");
            DropForeignKey("dbo.OrderSpecification", "ProductId", "dbo.Product");
            DropForeignKey("dbo.OrderSpecification", "OrderId", "dbo.Order");
            DropForeignKey("dbo.RefuelingEntry", "OrderId", "dbo.Order");
            DropForeignKey("dbo.RefuelingEntry", "FuelId", "dbo.Fuel");
            DropForeignKey("dbo.FuelPriceHistory", "FuelId", "dbo.Fuel");
            DropForeignKey("dbo.FuelPriceHistory", "EmployeePesel", "dbo.Employees");
            DropForeignKey("dbo.Order", "CustomerPesel", "dbo.Customers");
            DropIndex("dbo.OrderSpecification", new[] { "OrderId" });
            DropIndex("dbo.OrderSpecification", new[] { "RefuelingEntryId" });
            DropIndex("dbo.OrderSpecification", new[] { "ProductId" });
            DropIndex("dbo.RefuelingEntry", new[] { "FuelId" });
            DropIndex("dbo.RefuelingEntry", new[] { "OrderId" });
            DropIndex("dbo.FuelPriceHistory", new[] { "EmployeePesel" });
            DropIndex("dbo.FuelPriceHistory", new[] { "FuelId" });
            DropIndex("dbo.Order", new[] { "EmployeePesel" });
            DropIndex("dbo.Order", new[] { "CustomerPesel" });
            DropTable("dbo.Product");
            DropTable("dbo.OrderSpecification");
            DropTable("dbo.RefuelingEntry");
            DropTable("dbo.Fuel");
            DropTable("dbo.FuelPriceHistory");
            DropTable("dbo.Employees");
            DropTable("dbo.Order");
            DropTable("dbo.Customers");
        }
    }
}
