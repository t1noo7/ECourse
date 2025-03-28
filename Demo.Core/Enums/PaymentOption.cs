using System.ComponentModel;

namespace Demo.Core.ValueObjects
{
    public enum PaymentOption
    {
        [Description("Đăng ký sớm trước 1 tháng giảm 10%")]
        OneMonth = 1,

        [Description("Đăng ký trước 3 tháng giảm 15%")]
        ThreeMonths = 2
    }
}
