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

        
                private OrderDTO MapToOrderDto(Order order)
        {
            if (order == null) return null;

            return new OrderDTO
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                PaymentType = order.PaymentType,
                CustomerNip = order.Customer?.Nip,
                CustomerCompanyName = order.Customer?.CompanyName,
                EmployeePesel = order.Employee?.Pesel,
                EmployeeFullName = $"{order.Employee?.Name} {order.Employee?.Surname}",
                TotalAmount = order.TotalAmount,
                OrderSpecifications = order.OrderSpecifications?
                    .Select(MapToOrderSpecificationDto)
                    .ToList()
            };
        }

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
                PriceAtSale = re.PriceAtSale             };
        }

                private OrderSpecificationDTO MapToOrderSpecificationDto(OrderSpecification os)
        {
            if (os == null) return null;

            var dto = new OrderSpecificationDTO
            {
                OrderSpecificationId = os.OrderSpecificationId,
                Quantity = os.Quantity,
                PriceAtSale = os.PriceAtSale,
                OrderId = os.OrderId,
                ItemTotal = os.Quantity * os.PriceAtSale
            };

                        if (os.ProductId != default && os.Product != null)
            {
                dto.ProductId = os.ProductId;
                dto.ProductName = os.Product.Name;
                dto.ProductPrice = os.Product.Price;
                dto.IsFuel = false;
            }

                        if (os.RefuelingEntryId.HasValue && os.RefuelingEntry != null)
            {
                dto.RefuelingEntryId = os.RefuelingEntryId;
                dto.IsFuel = true;
                dto.RefuelingEntryDetails = new RefuelingEntryDTO
                {
                    RefuelingEntryId = os.RefuelingEntry.RefuelingEntryId,
                    Amount = os.RefuelingEntry.Amount,
                    PriceAtSale = os.RefuelingEntry.PriceAtSale,
                    FuelId = os.RefuelingEntry.FuelId,
                    FuelName = os.RefuelingEntry.Fuel?.FuelName
                };
            }

            return dto;
        }

        
                        public OrderDTO CreateOrder(CreateOrderDTO orderDto)
        {
                        if (orderDto == null) throw new ArgumentNullException(nameof(orderDto));
                        if ((orderDto.Items == null || !orderDto.Items.Any()) && (orderDto.RefuelingEntries == null || !orderDto.RefuelingEntries.Any()))
            {
                throw new BusinessLogicException("Order must contain items or refueling entries.");
            }


                        var employee = _context.Employees.Find(orderDto.EmployeePesel);
            if (employee == null) throw new BusinessLogicException($"Employee with PESEL {orderDto.EmployeePesel} not found.");

                        Customer customer = null;
            if (!string.IsNullOrEmpty(orderDto.CustomerNip))             {
                customer = _context.Customers.Find(orderDto.CustomerNip);
                if (customer == null)                 {
                    throw new BusinessLogicException($"Customer with NIP {orderDto.CustomerNip} not found.");
                }
            }
            

                        var order = new Order
            {
                OrderDate = DateTime.Now,
                PaymentType = orderDto.PaymentType,
                CustomerPesel = customer?.Nip,                 EmployeePesel = orderDto.EmployeePesel,
                OrderSpecifications = new List<OrderSpecification>(),
                RefuelingEntries = new List<RefuelingEntry>(),                 TotalAmount = 0             };

            decimal calculatedTotalAmount = 0;

            
                        if (orderDto.Items != null)
            {
                foreach (var itemDto in orderDto.Items)
                {
                                                            
                    if (itemDto.Quantity <= 0) throw new BusinessLogicException($"Product quantity for item ID {itemDto.ItemId} must be positive.");

                                        var product = _context.Products.Find(itemDto.ItemId);
                    if (product == null) throw new BusinessLogicException($"Product with ID {itemDto.ItemId} not found.");

                                        decimal productPriceAtSale = product.Price; 
                                        var productSpec = new OrderSpecification
                    {
                        Quantity = (int)itemDto.Quantity,                         PriceAtSale = productPriceAtSale,                         ProductId = product.ProductId,
                        RefuelingEntryId = null,                         RefuelingEntry = null,
                                            };
                    order.OrderSpecifications.Add(productSpec); 
                                        calculatedTotalAmount += productSpec.Quantity * productSpec.PriceAtSale;
                }
            }


                        if (orderDto.RefuelingEntries != null)
            {
                foreach (var refuelingEntryDto in orderDto.RefuelingEntries)
                {
                    if (refuelingEntryDto.Amount <= 0) throw new BusinessLogicException($"Fuel amount for fuel ID {refuelingEntryDto.FuelId} must be positive.");

                                        var fuel = _context.Fuels.Find(refuelingEntryDto.FuelId);
                    if (fuel == null) throw new BusinessLogicException($"Fuel with ID {refuelingEntryDto.FuelId} not found.");

                                                                                                    decimal fuelPriceAtSale = refuelingEntryDto.PriceAtSale;

                                        var refuelingEntry = new RefuelingEntry
                    {
                        Amount = refuelingEntryDto.Amount,                         FuelId = fuel.FuelId,
                        PriceAtSale = fuelPriceAtSale,                                             };
                    order.RefuelingEntries.Add(refuelingEntry); 
                                                            var fuelSpec = new OrderSpecification
                    {
                        Quantity = 1,                         PriceAtSale = fuelPriceAtSale * refuelingEntryDto.Amount,                         RefuelingEntry = refuelingEntry,                                                 ProductId = null                     };

                    order.OrderSpecifications.Add(fuelSpec); 
                                        calculatedTotalAmount += refuelingEntry.Amount * refuelingEntry.PriceAtSale;
                }
            }


                        order.TotalAmount = calculatedTotalAmount;

                                                Console.WriteLine($"Order will be saved with:");
            Console.WriteLine($"- {order.OrderSpecifications.Count} specifications");
            Console.WriteLine($"- {order.RefuelingEntries.Count} refueling entries");
            Console.WriteLine($"- Total amount: {order.TotalAmount}");
            _context.Orders.Add(order);


                        _context.SaveChanges();

            Console.WriteLine($"Order saved with ID {order.OrderId}");
            Console.WriteLine($"- Specifications count: {order.OrderSpecifications.Count}");
            Console.WriteLine($"- Refueling entries count: {order.RefuelingEntries.Count}");

                                                            var createdOrder = _context.Orders
                                     .Include(o => o.Customer)
                                     .Include(o => o.Employee)
                                                                          .Include(o => o.OrderSpecifications.Select(os => os.Product))                                                                                                                                         .Include(o => o.RefuelingEntries.Select(re => re.Fuel))                                                                                                                                    .Include(o => o.OrderSpecifications.Select(os => os.RefuelingEntry))
                                     .SingleOrDefault(o => o.OrderId == order.OrderId); 
                        return MapToOrderDto(createdOrder);
        }

                public List<OrderDTO> GetAllOrders()
        {
                        var orders = _context.Orders
                                 .Include(o => o.Customer)
                                 .Include(o => o.Employee)
                                                                                                                                                                     .ToList();

            return orders.Select(o => MapToOrderDto(o)).ToList();
        }

                public OrderDTO GetOrderById(int orderId)
        {
                        var order = _context.Orders
                                .Include(o => o.Customer)
                                .Include(o => o.Employee)
                                                                .Include(o => o.OrderSpecifications.Select(os => os.Product))                                 .Include(o => o.OrderSpecifications.Select(os => os.RefuelingEntry.Fuel))                                 .SingleOrDefault(o => o.OrderId == orderId);

            return MapToOrderDto(order);         }

                public void DeleteOrder(int orderId)
        {
                        var order = _context.Orders
                                .Include(o => o.OrderSpecifications)
                                .Include(o => o.RefuelingEntries)                                 .SingleOrDefault(o => o.OrderId == orderId);

            if (order == null)
            {
                                return;
            }

                        
                                                if (order.OrderSpecifications != null)
            {
                _context.OrderSpecifications.RemoveRange(order.OrderSpecifications);
            }
            if (order.RefuelingEntries != null)
            {
                _context.RefuelingEntries.RemoveRange(order.RefuelingEntries);
            }

                        _context.Orders.Remove(order);

                        _context.SaveChanges();
        }
    }
}