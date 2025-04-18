using Microsoft.AspNetCore.Mvc;
using Demo.Common.Extensions;
using Demo.Application.Repositories;
using Demo.Core.Models;
using Demo.Core.Repositories;
using Demo.Web.Models;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;
using Demo.Web.ViewModels;
using Demo.Application.Services.IServices;
using Demo.Core.Enums;
using Demo.Application.Infrastructures;
using Microsoft.Extensions.Logging;

namespace Demo.Web.Controllers
{
    public class OrderController : Demo
    {
        private readonly ILogger<OrderController> _logger;
        private readonly ICourseRepository _courseRepository;
        private readonly IOrderService _orderService;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IVoucherRepository _voucherRepository;
        private readonly IFileService _fileService;
        private readonly ISystemParameters _systemParameters;
        private readonly IPaymentService _paymentService;
        private readonly IMailService _mailService;
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly ILessonRepository _lessonRepository;

        public OrderController(ILogger<OrderController> logger,
            ICourseRepository courseRepository,
            IOrderService orderService,
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            IVoucherRepository voucherRepository,
            IFileService fileService,
            ISystemParameters systemParameters,
            IPaymentService paymentService,
            IMailService mailService,
            IRazorViewEngine razorViewEngine,
            IServiceProvider serviceProvider,
            ITempDataProvider tempDataProvider,
            ILessonRepository lessonRepository)
        {
            _logger = logger;
            _courseRepository = courseRepository;
            _orderService = orderService;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _fileService = fileService;
            _systemParameters = systemParameters;
            _paymentService = paymentService;
            _voucherRepository = voucherRepository;
            _mailService = mailService;
            _razorViewEngine = razorViewEngine;
            _serviceProvider = serviceProvider;
            _tempDataProvider = tempDataProvider;
            _lessonRepository = lessonRepository;
        }

        [HttpGet]
        public IActionResult Checkout(Guid courseId)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = $"/Order/Checkout?courseId={courseId}" });
            }

            var course = _courseRepository.Find(x => x.Id == courseId).FirstOrDefault();

            var model = new OrderViewModel();
            var currentUser = _userRepository.GetByUsername(User.Identity.Name);

            if (currentUser != null)
            {
                model.CustomerName = currentUser.FullName;
                model.CustomerPhone = currentUser.PhoneNumber;
                model.CustomerEmail = currentUser.Email;
                model.CourseId = course.Id;
            }
            ViewBag.Course = course;
            var bankInfo = _systemParameters.BankInfo;
            if (!string.IsNullOrEmpty(bankInfo))
            {
                bankInfo = bankInfo.Replace("\r\n", "<br/>");
            }
            ViewData["BankInfo"] = bankInfo ?? "";
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Checkout(OrderViewModel model, Guid courseId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new JsonReturn(false, base.GetModalStateErrorMsg()));

                if (!User.Identity?.IsAuthenticated ?? true)
                    return Json(new JsonReturn(false, "Vui lòng đăng nhập để tạo đơn hàng."));

                var course = _courseRepository.Find(x => x.Id == courseId).FirstOrDefault();
                if (course == null)
                    return Json(new JsonReturn(false, "Không tìm thấy khóa học đã chọn, vui lòng thử lại."));

                long basePrice = course.Price;

                // Áp dụng voucher
                Voucher? appliedVoucher = null;
                long finalPrice = basePrice;
                if (!string.IsNullOrWhiteSpace(model.VoucherCode))
                {
                    var (isValid, message, voucher, newPrice) = await ValidateVoucher(model.VoucherCode, basePrice);
                    if (!isValid) return Json(new JsonReturn(false, message));
                    appliedVoucher = voucher;
                    finalPrice = newPrice;
                    await UpdateQuantity(voucher.Id, Math.Max(0, voucher.Quantity - 1));
                }

                var order = new Order
                {
                    Created = DateTimeExtensions.UTCNowVN,
                    Modified = DateTimeExtensions.UTCNowVN,
                    CreatedBy = User.Identity.Name,
                    ModifiedBy = User.Identity.Name,
                    Username = User.Identity.Name,
                    CustomerName = model.CustomerName,
                    CustomerPhone = model.CustomerPhone,
                    CustomerEmail = model.CustomerEmail,
                    CustomerNote = model.CustomerNote,
                    VerifyImageUrl = model.VerifyImageUrl,
                    Course = course,
                    CourseIds = new List<Guid> { courseId },
                    Status = OrderStatus.Pending,
                    StatusHistories = new List<OrderStatusDetails>
                    {
                         new OrderStatusDetails
                         {
                            ActionTime = DateTimeExtensions.UTCNowVN,
                            Status = OrderStatus.Pending,
                            Author = User.Identity.Name
                         }
                    },
                    Code = $"{User.Identity.Name[..Math.Min(4, User.Identity.Name.Length)]}{DateTimeExtensions.UTCNowVN:yyMMddHHmmss}",
                    Price = finalPrice,
                    Voucher = appliedVoucher
                };

                var result = await _orderRepository.UpsertAsync(order);
                if (result != null)
                {
                    _mailService.OrderStatusChanged(order);
                    TempData[TempDataKey.Success] = TempDataMessage.CheckOutSuccess;
                    return RedirectToAction("MyOrders");
                }

                TempData[TempDataKey.Error] = TempDataMessage.CheckOutError;
                return View("CheckOut", model);
            }
            catch (Exception ex)
            {
                TempData[TempDataKey.Error] = TempDataMessage.CheckOutError;
                _logger.LogError("Error", ex);
                return View("CheckOut", model);
            }
        }

        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
            if (file != null)
            {
                var date = DateTimeExtensions.UTCNowVN;
                string ext = Path.GetExtension(file.FileName);
                var url = _fileService.UpsertImage("orders", $"{date.Year}/{date.Month}/{Guid.NewGuid()}.{date.ToString("yyyyMMdd")}.{ext ?? "png"}", file.OpenReadStream());
                return Json(new JsonReturn(true, url));
            }
            return Json(new JsonReturn(false));
        }

        public IActionResult MyOrders()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        public IActionResult MyCourses()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account");
            }
            var myOrders = _orderService.GetActiveCourse(User.Identity.Name);
            var myCourses = myOrders.Select(o => o.Course).GroupBy(c => c.Id).Select(g => g.First()).ToList();
            var suggestCourses = _courseRepository.GetAll();
            var model = (myCourses, suggestCourses);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> TryValidateVoucher(string code, long originalPrice)
        {
            var (isValid, message, voucher, newPrice) = await ValidateVoucher(code, originalPrice);

            if (!isValid)
                return Json(new { success = false, message });

            return Json(new
            {
                success = true,
                finalPrice = newPrice,
                voucherCode = voucher?.Code,
                discountRate = voucher?.DiscountRate,
                discountAmount = voucher?.DiscountAmount
            });
        }

        #region Private
        private async Task UpdateQuantity(Guid id, int quantity)
        {
            var voucher = await _voucherRepository.GetAsync(id);
            if (voucher == null) return;

            voucher.Quantity = quantity;
            await _voucherRepository.UpdateAsync(voucher);
        }

        private async Task<(bool isValid, string message, Voucher? voucher, long finalPrice)> ValidateVoucher(string code, long originalPrice)
        {
            if (string.IsNullOrWhiteSpace(code))
                return (false, "Mã giảm giá trống!", null, originalPrice);

            var voucher = await _voucherRepository.Find(x => x.Code == code.Trim()).FirstOrDefaultAsync();

            if (voucher == null || voucher.StartDate.Date > DateTimeExtensions.UTCNowVN.Date)
                return (false, $"Mã giảm giá {code} không tồn tại!", null, originalPrice);

            if (voucher.Expired.Date < DateTimeExtensions.UTCNowVN.Date)
                return (false, $"Mã giảm giá {code} đã hết hạn!", null, originalPrice);

            if (voucher.Quantity <= 0)
                return (false, $"Mã giảm giá {code} đã hết lượt sử dụng!", null, originalPrice);

            long discountedPrice = originalPrice;

            if (voucher.DiscountRate > 0)
                discountedPrice = originalPrice * (100 - voucher.DiscountRate) / 100;
            else if (voucher.DiscountAmount > 0)
                discountedPrice = originalPrice - voucher.DiscountAmount;

            discountedPrice = Math.Max(discountedPrice, 0);

            return (true, "", voucher, discountedPrice);
        }

        private async Task AddUsedOrderId(Guid voucherId, Guid orderId)
        {
            var voucher = await _voucherRepository.GetAsync(x => x.Id == voucherId);
            if (voucher == null) return;

            if (voucher.UsedOrderIds == null)
                voucher.UsedOrderIds = new List<Guid>();

            if (!voucher.UsedOrderIds.Contains(orderId))
                voucher.UsedOrderIds.Add(orderId);

            await _voucherRepository.UpdateAsync(voucher);
        }
        #endregion
    }
}