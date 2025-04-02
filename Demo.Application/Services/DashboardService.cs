using Demo.Application.Repositories;
using Demo.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Application.Services
{
    public class DashboardService : IDashboardService
    {
        public IOrderRepository _orderReposiory;
        public IUserRepository _userReposiory;

        public DashboardService(IOrderRepository orderRepository, 
            IUserRepository userRepository)
        {
            _orderReposiory = orderRepository;
            _userReposiory = userRepository;
        }
        public decimal GetRevenueChangeRate(DateTime startDate, DateTime endDate)
        {
            // Xác định khoảng thời gian của kỳ trước
            var previousStartDate = startDate.AddMonths(-1);
            var previousEndDate = endDate.AddMonths(-1);

            // Doanh thu kỳ trước
            var lastPeriodRevenue = _orderReposiory
                .Find(o => o.Created >= previousStartDate && o.Created <= previousEndDate)
                .Sum(o => (decimal?)o.Price) ?? 0;

            // Doanh thu kỳ hiện tại
            var currentPeriodRevenue = _orderReposiory
                .Find(o => o.Created >= startDate && o.Created <= endDate)
                .Sum(o => (decimal?)o.Price) ?? 0;

            // Tính tỷ lệ thay đổi doanh thu
            return lastPeriodRevenue == 0 ? 0 : ((currentPeriodRevenue - lastPeriodRevenue) / lastPeriodRevenue) * 100;
        }

        public decimal GetOrderChangeRate(DateTime startDate, DateTime endDate)
        {
            // Xác định khoảng thời gian của kỳ trước
            var previousStartDate = startDate.AddMonths(-1);
            var previousEndDate = endDate.AddMonths(-1);

            // Số lượng đơn hàng kỳ trước
            var lastPeriodOrders = _orderReposiory
                .Find(o => o.Created >= previousStartDate && o.Created <= previousEndDate)
                .Count();

            // Số lượng đơn hàng kỳ hiện tại
            var currentPeriodOrders = _orderReposiory
                .Find(o => o.Created >= startDate && o.Created <= endDate)
                .Count();

            // Tính tỷ lệ thay đổi số lượng đơn hàng
            return lastPeriodOrders == 0 ? 0 : ((currentPeriodOrders - lastPeriodOrders) / (decimal)lastPeriodOrders) * 100;
        }


        /*        public decimal GetCustomerChangeRate()
                {
                    var lastMonthCustomers = _userReposiory
                        .Find(c => c.Created.Month == DateTime.Now.AddMonths(-1).Month)
                        .Count();

                    var thisMonthCustomers = _userReposiory
                        .Find(c => c.Created.Month == DateTime.Now.Month)
                        .Count();

                    return lastMonthCustomers == 0 ? 100 : ((thisMonthCustomers - lastMonthCustomers) / (decimal)lastMonthCustomers) * 100;
                }*/
    }
}
