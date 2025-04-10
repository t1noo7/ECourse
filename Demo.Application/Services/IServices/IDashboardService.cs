using Demo.Core.Enums;

namespace Demo.Application.Services.IServices
{
    public interface IDashboardService
    {
        (decimal rate, bool isIncrease) GetRevenueChangeRate(DateTime startDate, DateTime endDate, DashboardEnum filterType);
        (decimal rate, bool isIncrease) GetOrderChangeRate(DateTime startDate, DateTime endDate, DashboardEnum filterType);
        (decimal rate, bool isIncrease) GetCustomerChangeRate(DateTime startDate, DateTime endDate, DashboardEnum filterType);
    }
}
