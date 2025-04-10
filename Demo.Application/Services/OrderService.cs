using Demo.Application.Repositories;
using Demo.Application.Services.IServices;
using Demo.Core.Enums;
using Demo.Core.Models;

namespace Demo.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        List<Order> IOrderService.GetOrdersByUsername(string username)
        {
            return _orderRepository.Find(x => x.Username == username).ToList();
        }

        List<Order> IOrderService.GetActiveCourse(string username)
        {
            return _orderRepository.Find(x => x.Username == username && x.Status == OrderStatus.Approved || x.Status == OrderStatus.Pending).ToList();
        }
    }
}
