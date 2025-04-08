using Microsoft.AspNetCore.Mvc;
using Demo.Common.Extensions;
using Demo.Application.Repositories;
using Demo.Application.Services;
using Demo.Core.Models;
using Demo.Core.Repositories;
using Demo.Web.Models;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;
using Demo.Core.Enums;
using Demo.Application.Repositories;

namespace Demo.Web.Controllers
{
    public class OrderController : Demo
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICourseRepository _courseRepository;
        private readonly IOrderService _orderService;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IVoucherRepository _voucherRepository;
        private readonly IFileService _fileService;
        private readonly ISystemParameters _systemParameters;
        private readonly IPaymentService _paymentService;
        //private readonly IMailService _mailService;
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly ILessonRepository _lessonRepository;

        public OrderController(ILogger<HomeController> logger,
            ICourseRepository courseRepository,
            IOrderService orderService,
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            IVoucherRepository voucherRepository,
            IFileService fileService,
            ISystemParameters systemParameters,
            IPaymentService paymentService,
            //IMailService mailService,
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
            //_mailService = mailService;
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
                return RedirectToAction("Login", "Account", new { returnUrl = "/Checkout" });
            }

            var course = _courseRepository.Find(x => x.Id == courseId).FirstOrDefault();

            if (course == null)
            {
                return RedirectToAction("Cart");
            }

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
        public IActionResult Checkout(OrderViewModel model, Guid courseId)
        {
            if (!ModelState.IsValid)
            {
                string messages = base.GetModalStateErrorMsg();
                return Json(new JsonReturn(false, messages));
            }

            if (User.Identity?.IsAuthenticated != true)
            {
                return Json(new JsonReturn(false, "Vui lòng đăng nhập để tạo đơn hàng."));
            }

            // Ensure the product IDs are not empty
            if (courseId == null)
            {
                return Json(new JsonReturn(false, "Vui lòng chọn ít nhất một sản phẩm để thanh toán."));
            }

            if (model.PaymentOption != PaymentOption.OneMonth.GetHashCode()
                && model.PaymentOption != PaymentOption.ThreeMonths.GetHashCode())
            {
                return Json(new JsonReturn(false, $"Chưa chọn hình thức thanh toán!"));
            }
            // Fetch the selected courses from the repository
            var course = _courseRepository.Find(x => x.Id == courseId).FirstOrDefault();
            long price = 0;
            if (model.PaymentOption == PaymentOption.OneMonth.GetHashCode())
            {
                price = course.Price * (100 - 10) / 100;
            }
            else if (model.PaymentOption == PaymentOption.ThreeMonths.GetHashCode())
            {
                price = course.Price * (100 - 15) / 100;
            }

            if (course == null)
            {
                return Json(new JsonReturn(false, "Có lỗi xảy ra với khoá học đã chọn, vui lòng thử lại."));
            }

            var order = new Order();
            order.PaymentOption = (PaymentOption)model.PaymentOption;
            if (!String.IsNullOrEmpty(model.VoucherCode))
            {
                var voucher = _voucherRepository.Find(x => x.Code == model.VoucherCode).FirstOrDefault();
                if (voucher == null || voucher.StartDate.Date > DateTimeExtensions.UTCNowVN.Date)
                {
                    return Json(new JsonReturn(false, $"Mã giảm giá {model.VoucherCode} không tồn tại!"));
                }
                if (voucher.Expired.Date < DateTimeExtensions.UTCNowVN.Date)
                {
                    return Json(new JsonReturn(false, $"Mã giảm giá {model.VoucherCode} đã hết hạn!"));
                }
                if (voucher.Quantity <= 0)
                {
                    return Json(new JsonReturn(false, $"Mã giảm giá {model.VoucherCode} đã hết!"));
                }
                if (voucher.DiscountRate > 0)
                {
                    price = price * (100 - voucher.DiscountRate) / 100;
                }
                else if (voucher.DiscountAmount > 0)
                {
                    price -= voucher.DiscountAmount;
                }
                order.Voucher = voucher;
                var quantity = voucher.Quantity - 1 > 0 ? voucher.Quantity - 1 : 0;
                _voucherRepository.UpdateQuantity(voucher.Id, quantity);
            }

            order.Created = DateTimeExtensions.UTCNowVN;
            order.CreatedBy = User?.Identity?.Name;
            order.ModifiedBy = User?.Identity?.Name;
            order.Modified = DateTimeExtensions.UTCNowVN;
            order.Price = course.Price;
            order.Status = OrderStatus.Pending;
            order.Username = User.Identity.Name;
            order.CustomerName = model.CustomerName;
            order.CustomerPhone = model.CustomerPhone;
            order.CustomerEmail = model.CustomerEmail;
            order.Course = course;
            order.CourseIds = new List<Guid> { courseId };
            order.CustomerNote = model.CustomerNote;
            order.VerifyImageUrl = model.VerifyImageUrl;
            order.StatusHistories = new List<OrderStatusDetails>
            {
                new OrderStatusDetails
                {
                    ActionTime = DateTimeExtensions.UTCNowVN,
                    Status = OrderStatus.Pending,
                    Author = User?.Identity?.Name
                }
            };
            order.Code = User.Identity.Name.Length > 4
                ? User.Identity.Name.Substring(0, 4) + DateTimeExtensions.UTCNowVN.ToString("yyMMddHHmmss")
                : User.Identity.Name.Length + DateTimeExtensions.UTCNowVN.ToString("yyMMddHHmmss");
            order.Price = course.Price;

            // Save the order
            _orderRepository.UpsertAsync(order);

            // Send email notification
            //_mailService.OrderStatusChanged(order);

            // return Json(new JsonReturn(true, "Đặt hàng thành công!"));
            TempData["OrderSuccessMessage"] = "Đặt hàng thành công!";
            return RedirectToAction("MyOrders");
        }

        public IActionResult Voucher(string code)
        {
            if (String.IsNullOrEmpty(code))
            {
                return Json(new JsonReturn(false, $"Mã giảm giá trống!"));
            }
            code = code.Trim();
            var voucher = _voucherRepository.Find(x => x.Code == code).FirstOrDefault();
            if (voucher == null
                || new DateTime(voucher.StartDate.Year, voucher.StartDate.Month, voucher.StartDate.Day) > new DateTime(DateTimeExtensions.UTCNowVN.Year, DateTimeExtensions.UTCNowVN.Month, DateTimeExtensions.UTCNowVN.Day))
            {
                return Json(new JsonReturn(false, $"Mã giảm giá {code} không tồn tại!"));
            }
            if (voucher.Expired.Date < DateTimeExtensions.UTCNowVN.Date)
            {
                return Json(new JsonReturn(false, $"Mã giảm giá {code} đã hết hạn!"));
            }
            if (voucher.Quantity <= 0)
            {
                return Json(new JsonReturn(false, $"Mã giảm giá {code} đã hết!"));
            }
            return Json(new JsonReturn(true, voucher.DiscountRate > 0 ? $"{voucher.DiscountRate}%" : $"{voucher.DiscountAmount}vnd"));
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
            var myCourses = myOrders.Select(o => o.Course).ToList();
            var suggestCourses = _courseRepository.GetAll();
            var model = (myCourses, suggestCourses);
            return View(model);
        }

        public IActionResult MyLessons(Guid courseId)
        {
            var lessons = _lessonRepository.Find(x => x.Course.Id == courseId).ToList();
            return View(lessons);
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