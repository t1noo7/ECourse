using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Demo.Core.Repositories;
using Demo.Core.Services;
using Demo.Web.Filters;
using Demo.Core.Permission;
using Demo.Common.Extensions;
using Demo.Core.Models;
using Demo.Web.Areas.Admin.Models;
using Demo.Application.Services;
using Demo.Application.Repositories;
using Demo.Application.Models;
using Demo.Database.Repositories;

namespace Demo.Web.Areas.Admin.Controllers
{
    [WebAuthorize(RoleList.Admin, RoleList.Customer)]
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IUserGroupManager _userGroupManager;
        private readonly IUserGroupRepository _groupRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IClassRepository _classRepository;

        public AccountController(ILogger<AccountController> logger,
            UserManager<User> userManager,
            IUserRepository userRepository,
            IUserGroupManager userGroupManager,
            IUserGroupRepository groupRepository,
            IOrderRepository orderRepository,
            ICourseRepository courseRepository,
            IClassRepository classRepository
            )
        {
            _logger = logger;
            _userManager = userManager;
            _userRepository = userRepository;
            _userGroupManager = userGroupManager;
            _groupRepository = groupRepository;
            _orderRepository = orderRepository;
            _courseRepository = courseRepository;
            _classRepository = classRepository;
        }

        #region User
        public async Task<IActionResult> Users(FilterModel model, int page = 1)
        {
            if (model == null) model = new FilterModel();
            if (!_userGroupManager.HasPermission(User.Identity.Name, RoleList.Admin))
                model.custom = "Khách hàng";
            ViewBag.SearchModel = model;
            var users = await _userRepository.FindAsync(model);

            var pagedResult = users.GetPaged(page);
            return View(pagedResult);
        }

        [HttpGet]
        public async Task<IActionResult> UserDetails(string id)
        {
            ViewBag.Error = TempData["Error"];
            ViewBag.Success = TempData["Success"];
            var user = await _userRepository.GetByIdAsync(id);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserDetails(User model, string returnUrl)
        {
            if (model.Email != null)
                model.Email = model.Email.Trim().ToLower();

            var user = await _userRepository.GetByIdAsync(model.Id.ToString());
            user.Email = model.Email ?? user.Email;
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.Updated = DateTimeExtensions.UTCNowVN;

            var emailExisted = _userRepository.Find(m => m.Email != null && m.Email == model.Email && m.Id != model.Id).Any();
            if (emailExisted)
            {
                ViewBag.Error = $"Email {model.Email} đã tồn tại";
                return View(user);
            }

            await _userRepository.UpdateAsync(user);
            return Redirect(returnUrl);
        }
        #endregion

        #region Student
        public async Task<IActionResult> Students(StudentFilter model, int page = 1)
        {
            if (model == null) model = new StudentFilter();
            if (!_userGroupManager.HasPermission(User.Identity.Name, RoleList.Admin))
                model.custom = "Khách hàng";

            ViewBag.SearchModel = model;

            var orders = new List<Order>();
            if (model.CourseName != null)
            {
                orders = _orderRepository.Find(o => o.Deleted != true && o.Course.Title == model.CourseName).ToList();
            }
            else
            {
                orders = _orderRepository.Find(o => o.Deleted != true).ToList();
            }

            var usersWithOrders = orders.Select(o => o.Username).Distinct().ToHashSet();

            // Lọc danh sách user: chỉ giữ lại những người có đơn hàng
            var users = (await _userRepository.FindAsync(model))
                        .Where(u => usersWithOrders.Contains(u.UserName))
                        .ToList();

            var courseIds = orders.Select(o => o.Course.Id).Distinct().ToList();
            var courses = _courseRepository.Find(c => courseIds.Contains(c.Id)).ToList();

            var userIds = users.Select(u => u.Id).ToList();
            var userClasses = _classRepository.Find(c => c.StudentIds.Any(sid => userIds.Contains(sid))).ToList();

            List<StudentViewModel> studentViewModels = users.Select(user =>
            {
                // Lấy danh sách các khóa học mà User đã đặt hàng
                var userCourseIds = orders
                    .Where(o => o.Username == user.UserName)
                    .Select(o => o.Course.Id)
                    .Distinct()
                    .ToList();

                var userCourses = courses.Where(c => userCourseIds.Contains(c.Id)).ToList();

                var classes = userClasses
                    .Where(c => c.StudentIds.Contains(user.Id))
                    .Distinct()
                    .ToList();

                return new StudentViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    Course = userCourses,
                    Class = classes
                };
            }).ToList();

            var pagedResult = studentViewModels.GetPaged(page);

            return View(pagedResult);
        }


        [HttpGet]
        public async Task<IActionResult> StudentDetails(string id)
        {
            ViewBag.Error = TempData["Error"];
            ViewBag.Success = TempData["Success"];
            var user = await _userRepository.GetByIdAsync(id);

            var orders = _orderRepository.Find(o => o.Username == user.UserName && !o.Deleted).ToList();
            var courseIds = orders.Select(o => o.Course.Id).Distinct().ToList();
            var courses = _courseRepository.Find(c => courseIds.Contains(c.Id)).ToList();

            var studentViewModel = new StudentViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                FullName = user.FullName,
                Course = courses
            };

            return View(studentViewModel);
        }
        #endregion

        #region Customer
        public async Task<IActionResult> Customers(FilterModel model, int page = 1)
        {
            if (model == null) model = new FilterModel();

            model.custom = "Khách hàng";

            ViewBag.SearchModel = model;
            var users = await _userRepository.FindAsync(model);

            var pagedResult = users.GetPaged(page);
            return View(pagedResult);
        }

        public async Task<IActionResult> CustomerDetails(string id)
        {
            ViewBag.Error = TempData["Error"];
            ViewBag.Success = TempData["Success"];
            var user = await _userRepository.GetByIdAsync(id);
            return View(user);
        }
        #endregion

        public ActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddUser(RegisterViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = new User
                    {
                        UserName = model.UserName,
                        Email = model.Email ?? "email@email.com",
                    };
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(Users));
                    }

                    AddErrors(result);
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra, liên hệ nhà phát triển phần mềm để được hỗ trợ.");
            }
            return View(model);
        }

        [WebAuthorize(RoleList.Admin)]
        public async Task<IActionResult> LockUser(string id, bool isLocked)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                user.IsLocked = isLocked;
                await _userRepository.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Users));
            }

            return RedirectToAction(nameof(Users));
        }

        [WebAuthorize(RoleList.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string returnUrl)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                await _userManager.RemovePasswordAsync(user);
                await _userManager.AddPasswordAsync(user, "demo1");
                TempData["Success"] = $"{DateTimeExtensions.UTCNowVN.ToString("dd/MM/yyyy hh:mm:ss")}: Reset mật khẩu thành công";
            }
            catch (Exception e)
            {
                TempData["Error"] = $"{DateTimeExtensions.UTCNowVN.ToString("dd/MM/yyyy hh:mm:ss")}: Lỗi xảy ra khi reset mật khẩu: {e.Message}";
                return Redirect(returnUrl);
            }

            return Redirect(returnUrl);
        }

        [WebAuthorize(RoleList.Admin)]
        [HttpGet]
        public async Task<ActionResult> AssignRoles(string id, bool? isGroup)
        {
            var controllers = System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(type => typeof(Controller).IsAssignableFrom(type));
            var allRoles = new List<string>();
            foreach (var conller in controllers)
            {
                var att = Attribute.GetCustomAttributes(conller);
                foreach (var item in att)
                {
                    var roles = (item as Filters.WebAuthorizeAttribute)?.GetParams();
                    if (roles?.Any() == true) allRoles.AddRange(roles);
                }
            }
            var assignedRoles = new List<string>();
            if (isGroup.HasValue)
            {
                var groupdId = new Guid(id);
                var group = await _groupRepository.GetAsync(groupdId);
                assignedRoles = group.Roles;
            }
            else
            {
                var user = await _userRepository.GetByIdAsync(id);
                assignedRoles = user.CustomRoles;
            }

            (string ObjectId, List<KeyValuePair<string, bool>> Roles) model = (id, new List<KeyValuePair<string, bool>>());
            foreach (var item in allRoles)
            {
                model.Roles.Add(new KeyValuePair<string, bool>(item, assignedRoles.Contains(item)));
            }
            model.Roles = model.Roles.Distinct().ToList();
            return View(model);
        }

        [WebAuthorize(RoleList.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignRoles(string Item1, List<string> roles, bool? isGroup, string returnUrl)
        {
            if (isGroup.HasValue)
            {
                var groupdId = new Guid(Item1);
                await _userGroupManager.SetGroupRolesAsync(groupdId, roles);
            }
            else
            {
                await _userGroupManager.SetUserRolesAsync(Item1, roles);
            }
            if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Users));
            else return Redirect(returnUrl);
        }


        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
    }
}