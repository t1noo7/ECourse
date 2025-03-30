using Demo.Core.Models;

namespace RauSaDemoch.Application.Infrastructures
{
    public interface IMailService
    {
        /// <summary>
        /// Gửi thông báo khi thay đổi trạng thái đơn hàng
        /// </summary>
        /// <param name="order"></param>
        void OrderStatusChanged(Order order);

        void Send(string to, string subject, string body);
    }
}
