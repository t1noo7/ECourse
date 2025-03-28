using Microsoft.Extensions.Logging;
using Demo.Common.Extensions;
using Demo.Application.Repositories;
using Demo.Core.Models;
using Demo.Core.ValueObjects;

namespace Demo.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ILogger<PaymentService> _logger;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;
        //private readonly IMailService _mailService;
        private readonly ISystemParameters _systemParameters;

        public PaymentService(ILogger<PaymentService> logger,
            IPaymentRepository paymentRepository,
            IOrderRepository orderRepository,
            //IMailService mailService,
            ISystemParameters systemParameters)
        {
            _logger = logger;
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            //_mailService = mailService;
            _systemParameters = systemParameters;
        }

        public async Task ChangeStatusAsync(Guid paymentId, PaymentStatus state)
        {
            var payment = await _paymentRepository.GetAsync(paymentId);
            payment.Status = state;
            payment.Modified = DateTimeExtensions.UTCNowVN;
            await _paymentRepository.UpdateAsync(payment);
        }

        public List<Payment> GetPaid(Guid orderId, PaymentState paymentState)
        {
            var payments = _paymentRepository.Find(x => x.OrderId == orderId && x.Status.PaymentState == paymentState && x.Deleted != true).ToList();
            return payments;
        }
    }
}
