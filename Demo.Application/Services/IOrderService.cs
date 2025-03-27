using Demo.Core.Models;

namespace Demo.Application.Services
{
    public interface IOrderService
    {
        List<Order> GetOrdersByUsername(string username);
        List<Order> GetActiveCourse(string username);
    }
}
