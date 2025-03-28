using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Demo.Common.Extensions;
using Demo.Core.Permission;
using Demo.Core.Repositories;
using Demo.Core.Services;
using Demo.Web.Models;
using Demo.Core.Models;
using Demo.Web.Areas.Admin.Models;

namespace Demo.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IUserGroupManager _userGroupManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUserRepository _userRepository;

        [TempData]
        public string ErrorMessage { get; set; }

        public AccountController(ILogger<AccountController> logger,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IUserGroupManager userGroupManager,
            IUserRepository userRepository
            )
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _userGroupManager = userGroupManager;
            _userRepository = userRepository;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var hasAdminPermission = _userGroupManager.HasPermission(User.Identity.Name, new string[]
                {
                    RoleList.Admin, RoleList.Account, RoleList.Product, RoleList.Content
                });
                return hasAdminPermission ? RedirectToAction("Index", "Home", new { Area = "Admin" }) : RedirectToAction("Index", "Home");
            }
            await CreateAdminUserIfNeeded();
            var model = new LoginViewModel();
            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user?.IsLocked == true)
            {
                ModelState.AddModelError("", $"Tài khoản {model.UserName} đã bị khóa.");
                return View(model);
            }
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, true, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                user.LoginTime = DateTimeExtensions.UTCNowVN;
                await _userManager.UpdateAsync(user);
                var hasPermission = _userGroupManager.HasPermission(model.UserName, new string[]
                {
                            RoleList.Admin, RoleList.Account, RoleList.Product, RoleList.Content
                });
                if (hasPermission)
                {
                    return RedirectToAction("Login");
                }
                returnUrl = returnUrl ?? "/";
                return Redirect(returnUrl);
            }
            ModelState.AddModelError("", "Sai mật khẩu hoặc tên đăng nhập.");
            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> PhoneRegister()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var hasAdminPermission = _userGroupManager.HasPermission(User.Identity.Name, new string[]
                {
                    RoleList.Admin, RoleList.Account, RoleList.Product, RoleList.Content
                });
                return hasAdminPermission ? RedirectToAction("Index", "Home", new { Area = "Admin" }) : RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PhoneRegister(PhoneRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByNameAsync(model.Phone);
            if (user != null)
            {
                ModelState.AddModelError("Phone", "Số điện thoại đã được đăng ký trước đó.");
                return View(model);
            }
            var admin = new User
            {
                UserName = model.Phone,
                IsLocked = false,
                PhoneNumber = model.Phone
            };
            await _userManager.CreateAsync(admin, model.Password);

            var result = await _signInManager.PasswordSignInAsync(model.Phone, model.Password, true, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return Redirect("/");
            }

            TempData["success"] = true;
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult UsernameRegister()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UsernameRegister(RegisterViewModel model)
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
                        return Redirect("/");
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

        public ActionResult MyProfile()
        {
            var user = _userRepository.GetByUsername(User.Identity.Name);
            var model = new UserViewModel()
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address ?? "",
                Name = user.FullName
            };
            return View(model);
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ChangePassword(ManageUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null) return null;
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    return Redirect("/");
                }
                AddErrors(result);
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        private async Task CreateAdminUserIfNeeded()
        {
            var username = "admin";
            var admin = await _userManager.FindByNameAsync(username);
            if (admin == null)
            {
                admin = new User
                {
                    UserName = username,
                    Email = $"{username}@email.com",
                    IsLocked = false,
                    CustomRoles = new List<string> { RoleList.Admin }
                };
                await _userManager.CreateAsync(admin, "1");
            }
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
