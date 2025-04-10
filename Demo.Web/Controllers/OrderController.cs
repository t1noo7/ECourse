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
            if (!ModelState.IsValid)
                return Json(new JsonReturn(false, base.GetModalStateErrorMsg()));

            if (!User.Identity?.IsAuthenticated ?? true)
                return Json(new JsonReturn(false, "Vui lòng đăng nhập để tạo đơn hàng."));

            var course = _courseRepository.Find(x => x.Id == courseId).FirstOrDefault();
            if (course == null)
                return Json(new JsonReturn(false, "Có lỗi xảy ra với khoá học đã chọn, vui lòng thử lại."));

            if (!Enum.IsDefined(typeof(PaymentOption), model.PaymentOption))
                return Json(new JsonReturn(false, "Chưa chọn hình thức thanh toán!"));

            long basePrice = model.PaymentOption switch
            {
                (int)PaymentOption.OneMonth => course.Price * 90 / 100,
                (int)PaymentOption.ThreeMonths => course.Price * 85 / 100,
                _ => course.Price
            };

            // Áp dụng voucher
            Voucher? appliedVoucher = null;
            long finalPrice = basePrice;
            if (!string.IsNullOrWhiteSpace(model.VoucherCode))
            {
                var (isValid, message, voucher, newPrice) = ValidateVoucher(model.VoucherCode, basePrice);
                if (!isValid) return Json(new JsonReturn(false, message));
                appliedVoucher = voucher;
                finalPrice = newPrice;
                _voucherRepository.UpdateQuantity(voucher.Id, Math.Max(0, voucher.Quantity - 1));
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
                PaymentOption = (PaymentOption)model.PaymentOption,
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

            var result = _orderRepository.UpsertAsync(order);
            if (result != null)
            {
                _mailService.OrderStatusChanged(order);
                TempData["OrderSuccessMessage"] = "Đặt hàng thành công!";
                return RedirectToAction("MyOrders");
            }
            else
            {
                return Json(new JsonReturn(false, "Có lỗi khi lưu đơn hàng, vui lòng thử lại."));
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

        private (bool isValid, string message, Voucher? voucher, long finalPrice) ValidateVoucher(string? code, long originalPrice)
        {
            if (string.IsNullOrWhiteSpace(code))
                return (false, "Mã giảm giá trống!", null, originalPrice);

            var voucher = _voucherRepository.Find(x => x.Code == code.Trim()).FirstOrDefault();
            if (voucher == null || voucher.StartDate.Date > DateTimeExtensions.UTCNowVN.Date)
                return (false, $"Mã giảm giá {code} không tồn tại!", null, originalPrice);

            if (voucher.Expired.Date < DateTimeExtensions.UTCNowVN.Date)
                return (false, $"Mã giảm giá {code} đã hết hạn!", null, originalPrice);

            if (voucher.Quantity <= 0)
                return (false, $"Mã giảm giá {code} đã hết!", null, originalPrice);

            var discountedPrice = voucher.DiscountRate > 0
                ? originalPrice * (100 - voucher.DiscountRate) / 100
                : originalPrice - voucher.DiscountAmount;

            return (true, "", voucher, Math.Max(discountedPrice, 0));
        }


        private async Task<string> RenderRazorViewToStringAsync(string viewName, object model)
        {
            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var actionContext = new ActionContext(HttpContext, RouteData, ControllerContext.ActionDescriptor, ModelState);

                var viewResult = _razorViewEngine.FindView(actionContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"View {viewName} was not found.");
                }

                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }
    }
}