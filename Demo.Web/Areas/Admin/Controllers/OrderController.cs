using Microsoft.AspNetCore.Mvc;
using Demo.Application.Models;
using Demo.Application.Repositories;
using Demo.Core.Permission;
using Demo.Core.ValueObjects;
using Demo.Web.Filters;
using Demo.Application.Services;
using Demo.Common.Extensions;
using Demo.Core.Models;

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

        public OrderController(ILogger<OrderController> logger,
            IOrderRepository orderRepository,
            ICourseRepository courseRepository,
            IMailService mailService)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _courseRepository = courseRepository;
            _mailService = mailService;
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

            // var course = _courseRepository.Get(model.course.Id);
            // if (course == null) ViewBag.CourseItems = new List<string>();
            // else
            // {
            //     var courseItems = course.GetGardenItems().Where(m => m.Area == model.Garden.Area);
            //     var gardenCodesUsed = _orderRepository.Find(m => m.Id != model.Id && m.Garden.Id == garden.Id).Select(m => m.GardenCode).Distinct().ToList();
            //     ViewBag.GardenItems = gardenItems.Where(m => !gardenCodesUsed.Contains(m.Code)).Select(m => m.Code).OrderBy(m => m).ToList();
            // }

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

            // if (model.FarmerId.HasValue)
            // {
            //     order.FarmerId = model.FarmerId;
            //     var farmer = await _generalItemRepository.GetAsync(model.FarmerId.Value);
            //     order.FarmerName = farmer?.Title;
            // }
            // foreach (var item in order.Combo.Vegetables)
            // {
            //     var vege = model.Combo.Vegetables.FirstOrDefault(m => m.Id == item.Id);
            //     item.Area = vege == null ? item.Area : vege.Area;
            // }

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
