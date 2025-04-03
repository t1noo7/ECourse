using Demo.Core.Enums;

namespace Demo.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime UTCNowVN
        {
            get { return DateTime.UtcNow.AddHours(7); }
        }

        public static (DateTime startDate, DateTime endDate) GetDateRange(DashboardEnum filterType)
        {
            // Lấy múi giờ UTC+7
            TimeZoneInfo utcPlus7 = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            // Chuyển đổi từ UTC sang UTC+7
            DateTime nowUtc7 = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, utcPlus7);

            DateTime startDate, endDate;

            switch (filterType)
            {
                case DashboardEnum.Week:
                    startDate = nowUtc7.AddDays(-7).Date;
                    endDate = nowUtc7; // Giữ nguyên giờ phút giây hiện tại
                    break;
                case DashboardEnum.Month:
                default:
                    startDate = new DateTime(nowUtc7.Year, nowUtc7.Month, 1);
                    endDate = nowUtc7; // Giữ nguyên giờ phút giây hiện tại
                    break;
            }

            return (startDate, endDate);
        }

    }
}