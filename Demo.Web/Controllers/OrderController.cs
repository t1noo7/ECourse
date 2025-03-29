using Microsoft.AspNetCore.Mvc;
using Demo.Common.Extensions;
using Demo.Application.Repositories;
using Demo.Application.Services;
using Demo.Core.Models;
using Demo.Core.Repositories;
using Demo.Core.ValueObjects;
using Demo.Web.Models;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using Demo.Web.ViewModels;

namespace Demo.Web.Controllers
{
    public class OrderController : Demo
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICourseRepository _courseRepository;
        private readonly IOrderService _orderService;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        //private readonly IVoucherRepository _voucherRepository;
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
            //IVoucherRepository voucherRepository,
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
            _systemParameters = systemParameters;
            _paymentService = paymentService;
            //_voucherRepository = voucherRepository;
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

            // Fetch the selected courses from the repository
            var courses = _courseRepository.Find(x => x.Id == courseId).FirstOrDefault();
            if (courses == null)
            {
                return Json(new JsonReturn(false, "Có lỗi xảy ra với khoá học đã chọn, vui lòng thử lại."));
            }

            var order = new Order();
            // Gán các thuộc tính của Order
            order.Created = DateTimeExtensions.UTCNowVN;
            order.CreatedBy = User?.Identity?.Name;
            order.ModifiedBy = User?.Identity?.Name;
            order.Modified = DateTimeExtensions.UTCNowVN;
            order.Price = courses.Price;
            order.Status = OrderStatus.Pending;
            order.Username = User.Identity.Name;
            order.CustomerName = model.CustomerName;
            order.CustomerPhone = model.CustomerPhone;
            order.Course = courses;
            //order.CustomerNote = model.CustomerNote;
            //order.VerifyImageUrl = model.VerifyImageUrl;
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
            order.Price = courses.Price;

            // Save the order
            _orderRepository.UpsertAsync(order);

            // Send email notification
            //_mailService.OrderStatusChanged(order);

            // return Json(new JsonReturn(true, "Đặt hàng thành công!"));
            TempData["OrderSuccessMessage"] = "Đặt hàng thành công!";
            return RedirectToAction("MyOrders");
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