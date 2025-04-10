using Demo.Application.Services.IServices;
using Demo.Web.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.IO;

namespace Demo.Web.Components
{
    public class OrderViewComponent : ViewComponent
    {
        private readonly IOrderService _orderService;

        public OrderViewComponent(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IViewComponentResult Invoke()
        {
            var orders = _orderService.GetOrdersByUsername(User.Identity.Name).OrderByDescending(x => x.Created).ToList();
            return View(orders);
        }
    }
}
