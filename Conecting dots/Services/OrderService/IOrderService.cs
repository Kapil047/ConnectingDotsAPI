using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;

namespace ConnectingDotsAPI.Services.OrderService
{
    public interface IOrderService
    {
        Task<Guid> CreateOrderAsync(OrderModelRequest.Order orderDto);
        Task<List<OrderModel.OrderWithDetails>> GetOrders(Guid? orderId);
    }
}