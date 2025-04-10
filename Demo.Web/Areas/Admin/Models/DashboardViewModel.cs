using Demo.Common.Extensions;
using Demo.Core.Enums;

namespace Demo.Web.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        /// <summary>
        /// Doanh số
        /// </summary>
        public decimal Revenue { get; set; } 
        /// <summary>
        /// Tổng số học viên
        /// </summary>
        public int TotalStudents { get; set; } 
        public int TotalOrders { get; set; }
        // Thay đổi doanh số
        public decimal RevenueChange { get; set; }
        public bool IsRevenueIncrease { get; set; }

        // Thay đổi đơn hàng
        public decimal OrderChange { get; set; }
        public bool IsOrderIncrease { get; set; }

        // Thay đổi số học viên
        public decimal CustomerChange { get; set; }
        public bool IsCustomerIncrease { get; set; }
        public List<OrderByDayViewModel> OrdersByDay { get; set; }
        public List<RecentOrderViewModel> RecentOrders { get; set; } 
        public List<PopularCourseViewModel> PopularCourses { get; set; } 
    }

    /// <summary>
    /// Đơn hàng gấn đây
    /// </summary>
    public class RecentOrderViewModel
    {
        public string OrderCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string CourseName { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
    }

    /// <summary>
    /// Khóa học phổ biến
    /// </summary>
    public class PopularCourseViewModel
    {
        public string CourseName { get; set; }
        public string Image {  get; set; }
        /// <summary>
        /// Số lượng học viên đã đăng ký
        /// </summary>
        public int EnrolledStudents { get; set; } 
    }

    public class OrderByDayViewModel
    {
        public DateTime OrderDate { get; set; }
        public int OrderCount { get; set; }
    }
}
