using System.ComponentModel;

namespace Demo.Core.ValueObjects
{
    public enum ClassState
    {
        [Description("Lớp chưa mở")]
        Pendding = 0,

        [Description("Lớp đang chạy")]
        Delivering = 10,

        [Description("Lớp tạm dừng")]
        Succeeded = 20,

        [Description("Lớp đã kết thúc chương trình")]
        Failed = 30
    }
}