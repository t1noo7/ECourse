using System.ComponentModel;

namespace Demo.Core.Enums
{
    public enum PaymentOption
    {
        [Description("Chọn hình thức thanh toán")]
        None = 0,

        [Description("Đăng ký sớm trước 1 tháng giảm 10%")]
        OneMonth = 1,

        [Description("Đăng ký trước 3 tháng giảm 15%")]
        ThreeMonths = 2
    }
}
