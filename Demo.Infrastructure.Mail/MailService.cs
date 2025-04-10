using Microsoft.Extensions.Logging;
using Demo.Application.Repositories;
using Demo.Core.Models;
using Demo.Core.Repositories;
using System.Net;
using System.Net.Mail;
using Demo.Core.Services;
using Demo.Application.Services.IServices;
using Demo.Core.Enums;

namespace Demo.Infrastructure.Mail
{
    public class MailService : IMailService
    {
        private readonly ILogger<MailService> _logger;
        private readonly ISystemParameters _systemParameters;
        private readonly IEmailTemplate _emailTemplate;
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;
        public MailService(ILogger<MailService> logger, ISystemParameters systemParameters, IEmailTemplate emailTemplate, IUserRepository userRepository, IOrderRepository orderRepository)
        {
            _logger = logger;
            _systemParameters = systemParameters;
            _emailTemplate = emailTemplate;
            _userRepository = userRepository;
            _orderRepository = orderRepository;
        }

        public void OrderStatusChanged(Order order)
        {
            switch (order.Status)
            {
                case OrderStatus.Pending:
                    SendCustomerOrder(order, "CustomerOrderPendding", $"{_systemParameters.Domain}/don-hang-cua-toi", "Đơn hàng của bạn đang chờ duyệt");

                    // Gửi mail cho kế toán
                    AccountantOrderPending(order);
                    break;
                case OrderStatus.Approved:
                    SendCustomerOrder(order, "CustomerOrderApproved", $"{_systemParameters.Domain}/khoa-hoc-cua-toi", "Đơn hàng của bạn đã được duyệt");
                    break;
                case OrderStatus.Canceled:
                    SendCustomerOrder(order, "CustomerOrderCanceled", $"{_systemParameters.Domain}/don-hang-cua-toi", "Đơn hàng của bạn đã bị hủy");
                    break;
                default:
                    break;

            }
        }

        public void RegisterVerification(string email, string code)
        {
            if (!string.IsNullOrWhiteSpace(_systemParameters.AccountingEmails))
            {
                var body = _emailTemplate.GetTemplate("RegisterVerify.html");
                body = body.Replace("__VerificationCode__", code);
                Send(email, $"Mã xác thực của email {email}", body);
                _logger.LogDebug($"Email sent accounting notified, email: {email}");
            }
        }

        private void AccountantOrderPending(Order order)
        {
            if (!string.IsNullOrWhiteSpace(_systemParameters.AccountingEmails))
            {
                var body = _emailTemplate.GetTemplate("AccountantOrderPending.html");
                body = body.Replace("__Link__", $"{_systemParameters.Domain}/admin/order/edit/{order.Id}");
                body = body.Replace("__OrderCode__", order.Code);
                Send(_systemParameters.AccountingEmails, $"Đơn hàng cần duyệt {order.Code}", body);
                _logger.LogDebug($"Email sent accounting notified, order Id: {order.Id}");
            }
        }

        private void SendCustomerOrder(Order order, string templateName, string link, string title)
        {
            var emails = GetUserEmails(order);
            if (emails != null)
            {
                var body = _emailTemplate.GetTemplate($"{templateName}.html");
                body = body.Replace("__Link__", link);
                body = body.Replace("__OrderCode__", order.Code);
                Send(emails, title, body);
                _logger.LogDebug($"Email sent SendCustomerOrder, order Id: {order.Id}, status: {order.Status}");
            }
        }

        public void Send(string to, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(to))
            {
                _logger.LogDebug($"Recipients is empty, subject: {subject}");
                return;
            }
            var fromAddress = new MailAddress(_systemParameters.SmtpEmail, "Website Bán Khóa Học");

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = _systemParameters.SmtpPort,// Gmail = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_systemParameters.SmtpEmail, _systemParameters.SmtpPassword)
            };
            using (var message = new MailMessage()
            {
                Subject = $"Ecourse Education - {subject}",
                Body = body
            })
            {
                message.From = fromAddress;
                foreach (var address in to.Split(new[] { ';', '\n', '\r', ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    message.To.Add(address);
                }
                message.IsBodyHtml = true;
                smtp.Send(message);
            }

            _logger.LogDebug($"Email sent to: {to}");
        }

        private string? GetUserEmails(Order order)
        {
            if (order == null) return null;
            var user = _userRepository.GetByUsername(order.Username);
            var emails = $"{user.Email};";
            return emails.Length > 1 ? emails : null;
        }
    }
}
