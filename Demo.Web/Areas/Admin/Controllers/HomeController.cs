using Microsoft.AspNetCore.Mvc;
using Demo.Common.Extensions;
using Demo.Application.Repositories;
using Demo.Web.Areas.Admin.Models;
using Demo.Web.Models;
using System.Diagnostics;
using Demo.Database.Repositories;
using Demo.Core.Enums;
using Demo.Core.ValueObjects;
using Demo.Application.Services.IServices;

namespace Demo.Web.Areas.Admin.Controllers
{
    //[WebAuthorize(RoleList.Admin)]
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IDashboardService _dashboardService;

        public HomeController(ILogger<HomeController> logger,
            IOrderRepository orderRepository,
            ICourseRepository courseRepository,
            IDashboardService dashboardService)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _courseRepository = courseRepository;
            _dashboardService = dashboardService;
        }

        public IActionResult Index(DashboardEnum filterType)
        {
            if (filterType == null || filterType == 0)
            {
                filterType = DashboardEnum.Week;
            }
            var (startDate, endDate) = DateTimeExtensions.GetDateRange(filterType);

            // Tổng doanh thu từ khóa học đã bán
            var totalRevenue = _orderRepository.Find(o => o.Status == OrderStatus.Paid && o.Created >= startDate && o.Created <= endDate)
                                .Sum(o => o.Price);

            // Tổng số khách hàng đã mua ít nhất một khóa học
            var totalCustomers = _orderRepository.Find(o => o.Status == OrderStatus.Paid && o.Created >= startDate && o.Created <= endDate)
                                .Select(o => o.Username).Distinct().Count();

            // Tổng số đơn hàng đã thanh toán
            var totalOrders = _orderRepository.Find(o => o.Deleted != true && o.Created >= startDate && o.Created <= endDate).Count();

            // Tỷ lệ thay đổi
            var (revenueChange, isRevenueIncrease) = _dashboardService.GetRevenueChangeRate(startDate, endDate, filterType);
            var (orderChange, isOrderIncrease) = _dashboardService.GetOrderChangeRate(startDate, endDate, filterType);
            var (customerChange, isCustomerIncrease) = _dashboardService.GetCustomerChangeRate(startDate, endDate, filterType);

            // Đơn hàng gần đây
            var recentOrders = _orderRepository.Find(o => o.Deleted != true && o.Created >= startDate && o.Created <= endDate)
                    .OrderByDescending(o => o.Created)
                    .Take(3)
                    .Select(o => new RecentOrderViewModel
                    {
                        OrderCode = o.Code,
                        PhoneNumber = o.CustomerPhone,
                        Email = o.CustomerEmail,
                        OrderDate = o.Created,
                        Status = o.Status,
                        CourseName = o.Course.Title
                    }).ToList();

            // Khóa học phổ biến
            var popularCourses = _orderRepository.Find(o => o.Course != null && o.Status == OrderStatus.Paid && o.Created >= startDate && o.Created <= endDate)
                    .GroupBy(o => o.Course)
                    .Select(g => new
                    {
                        Course = g.Key,
                        EnrolledStudents = g.Count()
                    })
                    .OrderByDescending(g => g.EnrolledStudents)
                    .Take(5)
                    .Select(g => new PopularCourseViewModel
                    {
                        CourseName = g.Course.Title,
                        EnrolledStudents = g.EnrolledStudents,
                        Image = g.Course.Image
                    })
                    .ToList();

            // Nhóm đơn hàng theo ngày
            var orders = _orderRepository.Find(o => o.Deleted != true && o.Status == OrderStatus.Paid && o.Created >= startDate && o.Created <= endDate)
                             .ToList(); // Fetch trước, xử lý sau

            var ordersByDay = orders.GroupBy(o => o.Created.Date)
                                    .Select(g => new OrderByDayViewModel
                                    {
                                        OrderDate = g.Key,
                                        OrderCount = g.Count()
                                    })
                                    .OrderBy(g => g.OrderDate)
                                    .ToList();


            // Gán dữ liệu vào ViewModel
            var dashboardViewModel = new DashboardViewModel
            {
                Revenue = totalRevenue,
                TotalStudents = totalCustomers,
                TotalOrders = totalOrders,
                RecentOrders = recentOrders,
                PopularCourses = popularCourses,
                OrdersByDay = ordersByDay,
                RevenueChange = revenueChange,
                IsRevenueIncrease = isRevenueIncrease,
                OrderChange = orderChange,
                IsOrderIncrease = isOrderIncrease,
                CustomerChange = customerChange,
                IsCustomerIncrease = isCustomerIncrease,
            };

            return View(dashboardViewModel);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
