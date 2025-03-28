using Demo.Core.Models;
using Demo.Core.ValueObjects;

namespace Demo.Core.Models
{
    public class Payment : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }
        public string? OrderCode { get; set; }
        public string? UserName { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public string? Note { get; set; }
        public DateTime ScheduleDate { get; set; }
        public List<Course> Courses { get; set; } = new List<Course>();
        public string? ImageUrl { get; set; }
        public PaymentStatus Status { get; set; } = new PaymentStatus();
    }
}
