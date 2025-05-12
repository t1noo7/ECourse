using System.ComponentModel;

namespace Demo.Core.Enums
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
        Approved = 30
    }
}
