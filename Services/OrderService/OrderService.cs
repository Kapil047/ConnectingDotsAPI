using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ConnectingDotsAPI.Services.OrderService
{
    public class OrderService(ConnectingDotsDbContext db) : IOrderService
    {
        private readonly ConnectingDotsDbContext db = db;

        // Create Order
        public async Task<Guid> CreateOrderAsync(OrderModelRequest.Order orderDto)
        {
            var customer = await db.Customers.FirstOrDefaultAsync(x => x.Guid == Guid.Parse(orderDto.CustomerId)) ?? throw new Exception($"Customer {orderDto.CustomerId} is invalid");
            // Map DTO to Database Model
            var order = db.Orders.FirstOrDefault(x => !string.IsNullOrEmpty(orderDto.Id) && x.OrderGuid == Guid.Parse(orderDto.Id)) ?? new Order
            {

                OrderGuid = Guid.NewGuid(),
                OrderStatusId = 1,

                CreatedOnUtc = DateTime.UtcNow,

            };
            order.CustomOrderNumber = orderDto.CustomOrderNumber;
            order.CustomerId = customer.Id;
            order.CustomerCurrencyCode = orderDto.CustomerCurrencyCode;
            order.DeliveryDate = orderDto.DeliveryDate;
            order.Deadline = orderDto.Deadline;
            order.Podate = orderDto.Podate;
            
            // Map OrderItems
            foreach (var itemDto in orderDto.OrderItems)
            {
                var product = await db.Products.FirstOrDefaultAsync(x => x.Guid == Guid.Parse(itemDto.ProductId)) ?? throw new Exception($"Product {itemDto.ProductId} is invalid");
                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    OrderItemGuid = !itemDto.OrderItemGuid.HasValue?Guid.NewGuid():itemDto.OrderItemGuid.Value,
                    Quantity = itemDto.Quantity,
                    Color = itemDto.Color,
                    AttributeDescription = itemDto.AttributeDescription,
                    Style = itemDto.Style,
                    Uom = itemDto.Uom,
                    Weight = itemDto.Weight,
                    OrderId = itemDto.OrderId,
                   
                };
                order.OrderItems.Add(orderItem);
            }

            // Map OrderNotes
            foreach (var noteDto in orderDto.OrderNotes)
            {
                var orderNote = new OrderNote
                {
                    Note = noteDto.Note,
                    DownloadId = noteDto.DownloadId,
                    DisplayToCustomer = noteDto.DisplayToCustomer,
                    CreatedOnUtc = DateTime.UtcNow
                };
                order.OrderNotes.Add(orderNote);
            }
            if(order.Id==0)
            db.Orders.Add(order);
            await db.SaveChangesAsync();
            return order.OrderGuid;
        }


        // Create Order Note
        public async Task<OrderNote> CreateOrderNoteAsync(OrderModelRequest.OrderNote orderNoteDto)
        {
            // Map DTO to Database Model
            var orderNote = new OrderNote
            {
                OrderId = orderNoteDto.OrderId,
                Note = orderNoteDto.Note,
                DownloadId = orderNoteDto.DownloadId,
                DisplayToCustomer = orderNoteDto.DisplayToCustomer,
                CreatedOnUtc = DateTime.UtcNow
            };

            db.OrderNotes.Add(orderNote);
            await db.SaveChangesAsync();
            return orderNote;
        }

        // Fetch all orders with details
        public async Task<List<OrderModel.OrderWithDetails>> GetOrders(Guid? orderId)
        {
            var orders = await db.Orders.Where(x => orderId.HasValue ? orderId == x.OrderGuid : true)
                .Include(o => o.OrderItems) // Include OrderItems
                .Include(o => o.OrderNotes) // Include OrderNotes
                .Include(o => o.Customer) // Include OrderNotes
                .Select(o => new OrderModel.OrderWithDetails
                {
                    Id = o.OrderGuid,
                    CustomOrderNumber = o.CustomOrderNumber,
                    CustomerId = o.Customer.Guid,
                    Customer = $"{o.Customer.FirstName} {o.Customer.LastName}",
                    OrderStatus = o.OrderStatusId.ToString(),
                    OrderStatusId = o.OrderStatusId,
                    CreatedOnUtc = o.CreatedOnUtc,
                    Podate = o.Podate,
                    Deadline = o.Deadline,
                    DeliveryDate = o.DeliveryDate,
                    OrderItems = o.OrderItems.Select(oi => new
                    {
                        productId = oi.Product.Guid,
                        oi.OrderItemGuid,
                        oi.Quantity,
                        oi.Style,
                        oi.AttributeDescription,
                        oi.Weight,
                        oi.Color,
                        Product = oi.Product.Name,
                        oi.Uom,

                    }).ToList(),
                    OrderNotes = o.OrderNotes.Select(on => new
                    {
                        on.Note,
                        on.DownloadId,
                        on.DisplayToCustomer,
                    }).ToList()
                })
                .ToListAsync();

            return orders;
        }

        //public async Task<OrderModel.OrderWithDetails> UpdateOrderId(OrderItem orderItem)
        //{
        //    var item = db.OrderItems.FirstOrDefault(x => x.OrderItemGuid == orderItem.OrderItemGuid)?? throw new Exception("");

        //}
    }
}
