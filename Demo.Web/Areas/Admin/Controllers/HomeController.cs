using Microsoft.AspNetCore.Mvc;
using Demo.Common.Extensions;
using Demo.Application.Repositories;
using Demo.Web.Areas.Admin.Models;
using Demo.Web.Models;
using System.Diagnostics;
using Demo.Database.Repositories;
using Demo.Core.Enums;
using Demo.Core.ValueObjects;
using Demo.Application.Services;

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
            var (startDate, endDate) = DateTimeExtensions.GetDateRange(filterType);

            // Tổng doanh thu từ khóa học đã bán
            var totalRevenue = _orderRepository.Find(o => o.Status == OrderStatus.Paid && o.Created >= startDate && o.Created < endDate)
                                .Sum(o => o.Price);

            // Tổng số khách hàng đã mua ít nhất một khóa học
            var totalCustomers = _orderRepository.Find(o => o.Status == OrderStatus.Paid && o.Created >= startDate && o.Created < endDate)
                                .Select(o => o.Username).Distinct().Count();

            // Tổng số đơn hàng đã thanh toán
            var totalOrders = _orderRepository.Find(o => o.Deleted != true && o.Created >= startDate && o.Created < endDate).Count();

            // Tỷ lệ thay đổi
            decimal revenueChange = _dashboardService.GetRevenueChangeRate(startDate, endDate);
            decimal orderChange = _dashboardService.GetOrderChangeRate(startDate, endDate);

            // Đơn hàng gần đây
            var recentOrders = _orderRepository.Find(o => o.Deleted != true && o.Created >= startDate && o.Created < endDate)
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
            var popularCourses = _orderRepository.Find(o => o.Course != null && o.Created >= startDate && o.Created < endDate)
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

            // Gán dữ liệu vào ViewModel
            var dashboardViewModel = new DashboardViewModel
            {
                Revenue = totalRevenue,
                TotalStudents = totalCustomers,
                TotalOrders = totalOrders,
                RevenueChange = revenueChange,
                OrderChange = orderChange,
                RecentOrders = recentOrders,
                PopularCourses = popularCourses,
                /*FilterType = filterType*/
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
