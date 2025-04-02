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
            DateTime now = DateTime.UtcNow;
            DateTime startDate, endDate;

            switch (filterType)
            {
                case DashboardEnum.Week:
                    startDate = now.AddDays(-7).Date;
                    endDate = now;
                    break;
                case DashboardEnum.Month:
                default:
                    startDate = new DateTime(now.Year, now.Month, 1);
                    endDate = now;
                    break;
            }

            return (startDate, endDate);
        }
    }
}