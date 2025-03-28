using System.ComponentModel;

namespace Demo.Core.ValueObjects
{
    public enum OrderStatus
    {
        [Description("Khởi tạo")]
        Initial = 0,

        [Description("Chờ duyệt")]
        Pending = 10,

        [Description("Đã hủy")]
        Canceled = 20,

        [Description("Đã duyệt")]
        Paid = 30,

        [Description("Đã hết hạn")]
        Expired = 40
    }
}
