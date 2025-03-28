using System.ComponentModel;

namespace Demo.Core.ValueObjects
{
    public enum PaymentState
    {
        [Description("Chưa thanh toán")]
        Unpaid = 0,

        [Description("Đã thanh toán")]
        Paid = 10,

        [Description("Hoàn tiền")]
        Refund = 20
    }
}
