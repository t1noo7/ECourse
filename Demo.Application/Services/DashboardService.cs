using Demo.Application.Repositories;
using Demo.Application.Services.IServices;
using Demo.Core.Enums;
using Demo.Core.ValueObjects;

namespace Demo.Application.Services
{
    public class DashboardService : IDashboardService
    {
        public readonly IOrderRepository _orderRepository;

        public DashboardService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        /// <summary>
        /// % Tăng giảm của Doanh thu
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="filterType"></param>
        /// <returns></returns>
        public (decimal rate, bool isIncrease) GetRevenueChangeRate(DateTime startDate, DateTime endDate, DashboardEnum filterType)
        {
            DateTime previousStartDate, previousEndDate;

            if (filterType.GetHashCode() == 1) 
            {
                previousStartDate = startDate.AddDays(-7);
                previousEndDate = endDate.AddDays(-7);
            } else 
            {
                previousStartDate = startDate.AddMonths(-1);
                previousEndDate = endDate.AddMonths(-1);
            }

            var lastPeriodRevenue = _orderRepository
                .Find(o => o.Created >= previousStartDate && o.Created <= previousEndDate && o.Status == OrderStatus.Paid)
                .Sum(o => (decimal?)o.Price) ?? 0;

            var currentPeriodRevenue = _orderRepository
                .Find(o => o.Created >= startDate && o.Created <= endDate && o.Status == OrderStatus.Paid)
                .Sum(o => (decimal?)o.Price) ?? 0;

            return CalculateChangeRate(lastPeriodRevenue, currentPeriodRevenue);
        }

        /// <summary>
        /// % Tăng giảm của Đơn hàng
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="filterType"></param>
        /// <returns></returns>
        public (decimal rate, bool isIncrease) GetOrderChangeRate(DateTime startDate, DateTime endDate, DashboardEnum filterType)
        {
            DateTime previousStartDate, previousEndDate;

            if (filterType.GetHashCode() == 1) 
            {
                previousStartDate = startDate.AddDays(-7);
                previousEndDate = endDate.AddDays(-7);
            } else 
            {
                previousStartDate = startDate.AddMonths(-1);
                previousEndDate = endDate.AddMonths(-1);
            }

            var lastPeriodOrders = _orderRepository
                .Find(o => o.Created >= previousStartDate && o.Created <= previousEndDate && o.Status == OrderStatus.Paid)
                .Count();

            var currentPeriodOrders = _orderRepository
                .Find(o => o.Created >= startDate && o.Created <= endDate && o.Status == OrderStatus.Paid)
                .Count();

            return CalculateChangeRate(lastPeriodOrders, currentPeriodOrders);
        }

        /// <summary>
        /// % Tăng giảm của Học viên
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="filterType"></param>
        /// <returns></returns>
        public (decimal rate, bool isIncrease) GetCustomerChangeRate(DateTime startDate, DateTime endDate, DashboardEnum filterType)
        {
            DateTime previousStartDate, previousEndDate;

            if (filterType.GetHashCode() == 1) 
            {
                previousStartDate = startDate.AddDays(-7);
                previousEndDate = endDate.AddDays(-7);
            } else 
            {
                previousStartDate = startDate.AddMonths(-1);
                previousEndDate = endDate.AddMonths(-1);
            }

            var lastPeriodCustomers = _orderRepository
                .Find(o => o.Created >= previousStartDate && o.Created <= previousEndDate && o.Status == OrderStatus.Paid)
                .Select(o => o.Username)
                .Distinct()
                .Count();

            var thisPeriodCustomers = _orderRepository
                .Find(o => o.Created >= startDate && o.Created <= endDate && o.Status == OrderStatus.Paid)
                .Select(o => o.Username)
                .Distinct()
                .Count();

            return CalculateChangeRate(lastPeriodCustomers, thisPeriodCustomers);
        }

        /// <summary>
        /// Check giá trị % tăng giảm
        /// </summary>
        /// <param name="previousValue"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        private (decimal rate, bool isIncrease) CalculateChangeRate(decimal previousValue, decimal currentValue)
        {
            if (previousValue == 0) return (0, false); 

            decimal changeRate = ((currentValue - previousValue) / previousValue) * 100;
            bool isIncrease = changeRate > 0; // true nếu tăng, false nếu giảm hoặc không thay đổi

            return (changeRate, isIncrease);
        }
    }
}
