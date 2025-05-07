namespace GasStation.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Changes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Order", "TotalAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.RefuelingEntry", "PriceAtSale", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.OrderSpecification", "PriceAtSale", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.OrderSpecification", "Quantity", c => c.Int(nullable: false));
            AlterColumn("dbo.FuelPriceHistory", "DateTo", c => c.DateTime());
            DropColumn("dbo.OrderSpecification", "Ilosc");
        }
        
        public override void Down()
        {
            AddColumn("dbo.OrderSpecification", "Ilosc", c => c.Int(nullable: false));
            AlterColumn("dbo.FuelPriceHistory", "DateTo", c => c.DateTime(nullable: false));
            DropColumn("dbo.OrderSpecification", "Quantity");
            DropColumn("dbo.OrderSpecification", "PriceAtSale");
            DropColumn("dbo.RefuelingEntry", "PriceAtSale");
            DropColumn("dbo.Order", "TotalAmount");
        }
    }
}
