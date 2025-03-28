
namespace Demo.Core.ValueObjects
{
    public class PaymentStatus
    {
        public PaymentState PaymentState { get; set; }
        public string Message { get; set; }
        public DateTime UpdatedTime { get; set; }
    }
}
