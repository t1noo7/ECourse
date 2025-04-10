using Microsoft.AspNetCore.Mvc;
using Demo.Application.Models;
using Demo.Application.Repositories;
using Demo.Common.Extensions;
using Demo.Core.Models;
using Demo.Application.Services.IServices;
using Demo.Core.Enums;
using Demo.Core.Repositories;

namespace Demo.Web.Areas.Admin.Controllers
{
    //[WebAuthorize(RoleList.Account, RoleList.Admin, RoleList.OrderManager)]

    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMailService _mailService;
        private readonly IClassRepository _classRepository;
        private readonly IUserRepository _userRepository;

        public OrderController(ILogger<OrderController> logger,
            IOrderRepository orderRepository,
            ICourseRepository courseRepository,
            IMailService mailService,
            IClassRepository classRepository,
            IUserRepository userRepository)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _courseRepository = courseRepository;
            _mailService = mailService;
            _classRepository = classRepository;
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index(OrderFilter filter)
        {
            if (filter == null) filter = new OrderFilter { OrderStatus = OrderStatus.Pending };

            ViewBag.SearchModel = filter;
            var orders = await _orderRepository.FindAsync(filter);
            return View(orders);
        }

        public IActionResult Edit(Guid id)
        {
            ViewBag.Error = TempData["Error"];
            ViewBag.Success = TempData["Success"];
            var model = _orderRepository.Get(id);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Order model, string returnUrl)
        {
            var order = await _orderRepository.GetAsync(model.Id);
            order.ModifiedBy = User?.Identity?.Name;
            order.Modified = DateTimeExtensions.UTCNowVN;
            order.CustomerAddress = model.CustomerAddress;
            order.CustomerNote = model.CustomerNote;

            await _orderRepository.UpsertAsync(order);
            TempData["Success"] = $"{DateTimeExtensions.UTCNowVN.ToString("dd/MM/yyyy hh:mm:ss")}: Cập nhật đơn hàng thành công";
            if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
            else return Redirect(returnUrl);
        }

        public async Task<IActionResult> UpdateStatus(Guid id, OrderStatus status, string returnUrl)
        {
            var order = await _orderRepository.GetAsync(id);
            order.ModifiedBy = User?.Identity?.Name;
            order.Modified = DateTimeExtensions.UTCNowVN;
            order.Status = status;
            order.StatusHistories.Add(new OrderStatusDetails { ActionTime = DateTimeExtensions.UTCNowVN, Status = status, Author = User?.Identity?.Name });

            await _orderRepository.UpsertAsync(order);
            if (status == OrderStatus.Approved)
            {
                TempData["SuccessMessage"] = "Thay đổi trạng thái đơn hàng thành công. Vui lòng chuyển sang chức năng lớp học để thêm học viên vào lớp.";
            }

            _logger.LogDebug($"Status updated to {status}, order Id: {order.Id}");
            _mailService.OrderStatusChanged(order);

            if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
            else return Redirect(returnUrl);
        }

        public async Task<IActionResult> Delete(Guid id, string returnUrl)
        {
            await _orderRepository.SetAsync(id, nameof(Order.Deleted), true);
            if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
            else return Redirect(returnUrl);
        }
    }
}
