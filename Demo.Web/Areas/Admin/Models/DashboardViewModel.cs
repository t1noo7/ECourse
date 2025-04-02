using Demo.Common.Extensions;
using Demo.Core.ValueObjects;

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
        public decimal RevenueChange { get; set; } // Tỷ lệ thay đổi doanh số
        public decimal OrderChange { get; set; } // Tỷ lệ thay đổi đơn hàng
        public decimal CustomerChange { get; set; } // Tỷ lệ thay đổi số khách hàng

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

    
}
