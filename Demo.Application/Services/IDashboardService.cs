using Demo.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Application.Services
{
    public interface IDashboardService
    {
        (decimal rate, bool isIncrease) GetRevenueChangeRate(DateTime startDate, DateTime endDate, DashboardEnum filterType);
        (decimal rate, bool isIncrease) GetOrderChangeRate(DateTime startDate, DateTime endDate, DashboardEnum filterType);
        (decimal rate, bool isIncrease) GetCustomerChangeRate(DateTime startDate, DateTime endDate, DashboardEnum filterType);
    }
}
