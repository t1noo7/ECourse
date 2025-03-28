using Demo.Core.Models;

namespace Demo.Application.Services
{
    public interface IMailService
    {
        // public void RegisterSuccess(Order order, Course course);
        // public void CourseOpeningUpToDate(Order order, Course course);
        // public void PaymentConfirmation(Order order, Course course);
        // void ScheduleReminderAWeek(Delivery delivery);
        // public void CourseToOpen(Course course);
        void OrderStatusChanged(Order order);

        void Send(string to, string subject, string body);
    }
}
