namespace GasStation.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveDuplicatePeselFk : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.People", "Customer_Pesel", "dbo.Customers");
            DropForeignKey("dbo.People", "Employee_Pesel", "dbo.Employees");
            DropIndex("dbo.People", new[] { "Customer_Pesel" });
            DropIndex("dbo.People", new[] { "Employee_Pesel" });
            DropColumn("dbo.People", "Customer_Pesel");
            DropColumn("dbo.People", "Employee_Pesel");
        }
        
        public override void Down()
        {
            AddColumn("dbo.People", "Employee_Pesel", c => c.String(maxLength: 128));
            AddColumn("dbo.People", "Customer_Pesel", c => c.String(maxLength: 128));
            CreateIndex("dbo.People", "Employee_Pesel");
            CreateIndex("dbo.People", "Customer_Pesel");
            AddForeignKey("dbo.People", "Employee_Pesel", "dbo.Employees", "Pesel1");
            AddForeignKey("dbo.People", "Customer_Pesel", "dbo.Customers", "Pesel1");
        }
    }
}
