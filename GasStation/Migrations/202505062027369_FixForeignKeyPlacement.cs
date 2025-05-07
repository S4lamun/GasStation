namespace GasStation.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixForeignKeyPlacement : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Order", "CustomerId", "dbo.Customers");
            DropForeignKey("dbo.FuelPriceHistory", "EmployeeId", "dbo.Employees");
            DropForeignKey("dbo.Order", "EmployeeId", "dbo.Employees");
            DropIndex("dbo.Customers", new[] { "Pesel1" });
            DropIndex("dbo.Employees", new[] { "Pesel1" });
            DropColumn("dbo.Customers", "Pesel");
            DropColumn("dbo.Employees", "Pesel");
            RenameColumn(table: "dbo.Customers", name: "Pesel1", newName: "Pesel");
            RenameColumn(table: "dbo.Employees", name: "Pesel1", newName: "Pesel");
            RenameColumn(table: "dbo.Order", name: "CustomerId", newName: "CustomerPesel");
            RenameColumn(table: "dbo.Order", name: "EmployeeId", newName: "EmployeePesel");
            RenameColumn(table: "dbo.FuelPriceHistory", name: "EmployeeId", newName: "EmployeePesel");
            RenameIndex(table: "dbo.Order", name: "IX_CustomerId", newName: "IX_CustomerPesel");
            RenameIndex(table: "dbo.Order", name: "IX_EmployeeId", newName: "IX_EmployeePesel");
            RenameIndex(table: "dbo.FuelPriceHistory", name: "IX_EmployeeId", newName: "IX_EmployeePesel");
            DropPrimaryKey("dbo.Customers");
            DropPrimaryKey("dbo.Employees");
            AlterColumn("dbo.Customers", "Pesel", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.Employees", "Pesel", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.Customers", "Pesel");
            AddPrimaryKey("dbo.Employees", "Pesel");
            CreateIndex("dbo.Customers", "Pesel");
            CreateIndex("dbo.Employees", "Pesel");
            AddForeignKey("dbo.Order", "CustomerPesel", "dbo.Customers", "Pesel");
            AddForeignKey("dbo.FuelPriceHistory", "EmployeePesel", "dbo.Employees", "Pesel");
            AddForeignKey("dbo.Order", "EmployeePesel", "dbo.Employees", "Pesel");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Order", "EmployeePesel", "dbo.Employees");
            DropForeignKey("dbo.FuelPriceHistory", "EmployeePesel", "dbo.Employees");
            DropForeignKey("dbo.Order", "CustomerPesel", "dbo.Customers");
            DropIndex("dbo.Employees", new[] { "Pesel" });
            DropIndex("dbo.Customers", new[] { "Pesel" });
            DropPrimaryKey("dbo.Employees");
            DropPrimaryKey("dbo.Customers");
            AlterColumn("dbo.Employees", "Pesel", c => c.String());
            AlterColumn("dbo.Customers", "Pesel", c => c.String());
            AddPrimaryKey("dbo.Employees", "Pesel1");
            AddPrimaryKey("dbo.Customers", "Pesel1");
            RenameIndex(table: "dbo.FuelPriceHistory", name: "IX_EmployeePesel", newName: "IX_EmployeeId");
            RenameIndex(table: "dbo.Order", name: "IX_EmployeePesel", newName: "IX_EmployeeId");
            RenameIndex(table: "dbo.Order", name: "IX_CustomerPesel", newName: "IX_CustomerId");
            RenameColumn(table: "dbo.FuelPriceHistory", name: "EmployeePesel", newName: "EmployeeId");
            RenameColumn(table: "dbo.Order", name: "EmployeePesel", newName: "EmployeeId");
            RenameColumn(table: "dbo.Order", name: "CustomerPesel", newName: "CustomerId");
            RenameColumn(table: "dbo.Employees", name: "Pesel", newName: "Pesel1");
            RenameColumn(table: "dbo.Customers", name: "Pesel", newName: "Pesel1");
            AddColumn("dbo.Employees", "Pesel", c => c.String());
            AddColumn("dbo.Customers", "Pesel", c => c.String());
            CreateIndex("dbo.Employees", "Pesel1");
            CreateIndex("dbo.Customers", "Pesel1");
            AddForeignKey("dbo.Order", "EmployeeId", "dbo.Employees", "Pesel1");
            AddForeignKey("dbo.FuelPriceHistory", "EmployeeId", "dbo.Employees", "Pesel1");
            AddForeignKey("dbo.Order", "CustomerId", "dbo.Customers", "Pesel1");
        }
    }
}
