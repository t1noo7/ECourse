using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Application.Services
{
    public interface IDashboardService
    {
        decimal GetRevenueChangeRate(DateTime startDate, DateTime endDate);
        decimal GetOrderChangeRate(DateTime startDate, DateTime endDate);
        /*decimal GetCustomerChangeRate();*/
    }
}
