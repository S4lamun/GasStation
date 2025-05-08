using GasStation.Data;
using GasStation.DTO;
using GasStation.Models;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System;


namespace GasStation.Services
{
    public class OrderService
    {
        private readonly GasStationDbContext _context;
        private readonly FuelService _fuelService;

        public OrderService(GasStationDbContext context, FuelService fuelService)
        {
            _context = context;
            _fuelService = fuelService;
        }

        // --- Private Helper Methods for Mapping ---

        // Maps Order entity (with included related data) to OrderDto
        private OrderDTO MapToOrderDto(Order order)
        {
            if (order == null) return null;

            return new OrderDTO
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                PaymentType = order.PaymentType,
                TotalAmount = order.TotalAmount,

                CustomerNip = order.Customer?.Nip,
                CustomerCompanyName = $"{order.Customer?.Nip} {order.Customer?.CompanyName}",

                EmployeePesel = order.Employee?.Pesel,
                EmployeeFullName = $"{order.Employee?.Name} {order.Employee?.Surname}",

                // Map order specifications
                OrderSpecifications = order.OrderSpecifications?
                                          .Select(os => MapToOrderSpecificationDto(os))
                                          .ToList() ?? new List<OrderSpecificationDTO>()
            };
        }

        // Maps RefuelingEntry entity (with included related data) to RefuelingEntryDto
        private RefuelingEntryDTO MapToRefuelingEntryDto(RefuelingEntry re)
        {
            if (re == null) return null;

            return new RefuelingEntryDTO
            {
                RefuelingEntryId = re.RefuelingEntryId,
                Amount = re.Amount,
                OrderId = re.OrderId,

                FuelId = re.FuelId,
                FuelName = re.Fuel?.FuelName,
                PriceAtSale = re.PriceAtSale // Map PriceAtSale from entity
            };
        }

        // Maps OrderSpecification entity (with included related data) to OrderSpecificationDto
        private OrderSpecificationDTO MapToOrderSpecificationDto(OrderSpecification os)
        {
            if (os == null) return null;

            // Calculate the sum for the item based on PriceAtSale
            decimal itemTotal = 0;
            string productName = null;
            decimal? productPriceAtSale = null;
            RefuelingEntryDTO refuelingDetails = null;

            if (os.ProductId != null && os.Product != null)
            {
                // Product item
                productName = os.Product.Name;
                // Product price at time of purchase - get from PriceAtSale from OrderSpecification
                productPriceAtSale = os.PriceAtSale;
                itemTotal = os.PriceAtSale * os.Quantity; // Item total
            }
            else if (os.RefuelingEntryId != null && os.RefuelingEntry != null)
            {
                // Fuel item
                // Map refueling details using helper method
                refuelingDetails = MapToRefuelingEntryDto(os.RefuelingEntry);
                // Fuel price at time of purchase - get from PriceAtSale from RefuelingEntry
                decimal fuelPriceAtSale = os.RefuelingEntry.PriceAtSale;
                itemTotal = os.RefuelingEntry.Amount * fuelPriceAtSale; // Item total
            }

            return new OrderSpecificationDTO
            {
                OrderSpecificationId = os.OrderSpecificationId,
                Quantity = os.Quantity,
                OrderId = os.OrderId,

                // Product details
                ProductId = os.ProductId,
                ProductName = productName,
                ProductPrice = productPriceAtSale, // Map product PriceAtSale here

                // Refueling details
                RefuelingEntryId = os.RefuelingEntryId,
                RefuelingEntryDetails = refuelingDetails, // RefuelingEntryDto goes here

                ItemTotal = itemTotal // Calculated item total
            };
        }

        // --- Public Service Methods ---

        // Creates a new order based on input DTO
        public OrderDTO CreateOrder(CreateOrderDTO orderDto)
        {
            // 1. Validate input DTO
            if (orderDto == null) throw new ArgumentNullException(nameof(orderDto));
            if (orderDto.Items == null || !orderDto.Items.Any())
            {
                throw new BusinessLogicException("Order must contain items.");
            }

            // 2. Find and validate Employee
            var employee = _context.Employees.Find(orderDto.EmployeePesel);
            if (employee == null) throw new BusinessLogicException($"Employee with PESEL {orderDto.EmployeePesel} not found.");

            // 3. Find and validate Customer (now optional)
            Customer customer = null;
            if (!string.IsNullOrEmpty(orderDto.CustomerNip)) // Check if CustomerNip was provided
            {
                customer = _context.Customers.Find(orderDto.CustomerNip);
                if (customer == null) // If NIP was provided but customer not found
                {
                    throw new BusinessLogicException($"Customer with NIP {orderDto.CustomerNip} not found.");
                }
            }
            // If orderDto.CustomerNip is null or empty, 'customer' remains null, which is allowed.


            // 4. Create main Order entity
            var order = new Order
            {
                OrderDate = DateTime.Now,
                PaymentType = orderDto.PaymentType,
                CustomerPesel = customer?.Nip, // Set CustomerPesel to found customer's NIP or null
                EmployeePesel = orderDto.EmployeePesel,
                OrderSpecifications = new List<OrderSpecification>(),
                RefuelingEntries = new List<RefuelingEntry>(), // Initialize refueling entries collection
                TotalAmount = 0 // Will be calculated
            };

            decimal calculatedTotalAmount = 0;

            // 5. Process each item from DTO
            // Potrzebujemy serwisu FuelService do pobrania aktualnej ceny paliwa
            var fuelService = new FuelService(_context); // Tworzymy instancję FuelService

            foreach (var itemDto in orderDto.Items)
            {
                if (itemDto.Quantity <= 0) throw new BusinessLogicException("Item quantity must be positive.");

                if (itemDto.IsFuel)
                {
                    // --- Process fuel item ---
                    var fuel = _context.Fuels.Find(itemDto.ItemId);
                    if (fuel == null) throw new BusinessLogicException($"Fuel with ID {itemDto.ItemId} not found.");

                    // Get CURRENT fuel price at order time using FuelService
                    var fuelPriceHistory = fuelService.GetCurrentFuelPrice(fuel.FuelId, order.OrderDate);
                    if (fuelPriceHistory == null) throw new BusinessLogicException($"No active price found for fuel '{fuel.FuelName}' at {order.OrderDate}.");

                    decimal fuelPriceAtSale = fuelPriceHistory.Price;

                    // Create RefuelingEntry
                    var refuelingEntry = new RefuelingEntry
                    {
                        Amount = itemDto.Quantity, // This is the amount of fuel (liters)
                        FuelId = fuel.FuelId,
                        PriceAtSale = fuelPriceAtSale, // SAVE price in refueling entity
                        // OrderId will be set automatically by EF relationship or SaveChanges
                    };
                    order.RefuelingEntries.Add(refuelingEntry); // Add to the order's collection

                    // Create OrderSpecification for this fuel item
                    // An OrderSpecification for fuel links to the RefuelingEntry
                    var fuelSpec = new OrderSpecification
                    {
                        Quantity = 1, // Quantity of items = 1 (one refueling transaction for this spec line)
                        PriceAtSale = fuelPriceAtSale, // Optionally store unit price here too if needed for the spec line
                        RefuelingEntry = refuelingEntry, // Link between specification and refueling
                        RefuelingEntryId = refuelingEntry.RefuelingEntryId, // Ensure FK is set if needed
                        // OrderId will be set automatically by EF relationship or SaveChanges
                        ProductId = default // Not a product item
                    };
                    // Ensure the RefuelingEntry is added to the context first if not implicitly handled by cascade insert
                    // _context.RefuelingEntries.Add(refuelingEntry); // Usually not needed if added to the navigation property collection

                    order.OrderSpecifications.Add(fuelSpec); // Add to the order's collection


                    // Calculate sum for this item (liters * price per liter)
                    calculatedTotalAmount += refuelingEntry.Amount * refuelingEntry.PriceAtSale;
                }
                else
                {
                    // --- Process product item ---
                    var product = _context.Products.Find(itemDto.ItemId);
                    if (product == null) throw new BusinessLogicException($"Product with ID {itemDto.ItemId} not found.");

                    // Get product price at order time (assuming Product.Price is current selling price)
                    decimal productPriceAtSale = product.Price; // Fetched from Product entity

                    // Create OrderSpecification for this product item
                    var productSpec = new OrderSpecification
                    {
                        Quantity = (int)itemDto.Quantity, // Product quantity (pieces)
                        PriceAtSale = productPriceAtSale, // SAVE price in product specification
                        ProductId = product.ProductId,
                        RefuelingEntryId = null, // Not a fuel item
                        RefuelingEntry = null,
                        // OrderId will be set automatically by EF relationship or SaveChanges
                    };
                    order.OrderSpecifications.Add(productSpec); // Add to the order's collection

                    // Calculate sum for this item (quantity * price per item)
                    calculatedTotalAmount += productSpec.Quantity * productSpec.PriceAtSale;
                }
            }

            // 6. Set total order amount
            order.TotalAmount = calculatedTotalAmount;

            // 7. Add main Order entity to context
            // Adding the order will also add its related OrderSpecifications and RefuelingEntries
            // due to the relationships and cascade behavior (default in EF for dependent entities).
            _context.Orders.Add(order);


            // 8. Save changes (adds Order, OrderSpecifications, RefuelingEntries)
            _context.SaveChanges();

            // 9. Return DTO of created order
            // Fetch order again with loaded relationships for mapping to DTO
            // Use the ID generated by SaveChanges
            var createdOrder = _context.Orders
                                       .Include(o => o.Customer)
                                       .Include(o => o.Employee)
                                       // Include specification items and their related entities (Product, RefuelingEntry, Fuel)
                                       .Include(o => o.OrderSpecifications.Select(os => os.Product))
                                       // Ensure RefuelingEntry is included BEFORE including Fuel through it
                                       .Include(o => o.OrderSpecifications.Select(os => os.RefuelingEntry))
                                       .Include(o => o.OrderSpecifications.Select(os => os.RefuelingEntry.Fuel)) // Include Fuel through RefuelingEntry
                                       .SingleOrDefault(o => o.OrderId == order.OrderId); // Fetch by generated ID

            // Now map complete entity to DTO
            return MapToOrderDto(createdOrder);
        }

        // Get list of all orders
        public List<OrderDTO> GetAllOrders()
        {
            // Fetch orders with needed relationships for display in list
            var orders = _context.Orders
                                 .Include(o => o.Customer)
                                 .Include(o => o.Employee)
                                 // You can load Items only if needed in list view (e.g., summary)
                                 // If not, skip Include for performance
                                 // .Include(o => o.OrderSpecifications.Select(os => os.Product))
                                 // .Include(o => o.OrderSpecifications.Select(os => os.RefuelingEntry.Fuel))
                                 .ToList();

            return orders.Select(o => MapToOrderDto(o)).ToList();
        }

        // Get order details by ID
        public OrderDTO GetOrderById(int orderId)
        {
            // Fetch order with ALL needed relationships for detail view
            var order = _context.Orders
                                .Include(o => o.Customer)
                                .Include(o => o.Employee)
                                // Include specification items and their related entities
                                .Include(o => o.OrderSpecifications.Select(os => os.Product)) // For product items
                                .Include(o => o.OrderSpecifications.Select(os => os.RefuelingEntry.Fuel)) // For fuel items (via RefuelingEntry)
                                .SingleOrDefault(o => o.OrderId == orderId);

            return MapToOrderDto(order); // Map to DTO
        }

        // Delete order by ID
        public void DeleteOrder(int orderId)
        {
            // Find order with related items (for manual deletion if EF doesn't do cascade)
            var order = _context.Orders
                                .Include(o => o.OrderSpecifications)
                                .Include(o => o.RefuelingEntries) // Include refuelings
                                .SingleOrDefault(o => o.OrderId == orderId);

            if (order == null)
            {
                // Not found, do nothing
                return;
            }

            // Business validation: Check if order can be deleted (e.g., not paid)
            // In this example, we assume it can be deleted

            // Manual deletion of child items if cascade deletion in database is not configured OR you want to be sure
            // By default in EF6, one-to-many relationships from Order to OrderSpecification/RefuelingEntry usually have cascade deletion ENABLED at DB level.
            // This code is for cases where it's not or you prefer to be explicit.
            if (order.OrderSpecifications != null)
            {
                _context.OrderSpecifications.RemoveRange(order.OrderSpecifications);
            }
            if (order.RefuelingEntries != null)
            {
                _context.RefuelingEntries.RemoveRange(order.RefuelingEntries);
            }

            // Delete main Order entity
            _context.Orders.Remove(order);

            // Save changes
            _context.SaveChanges();
        }
    }
}