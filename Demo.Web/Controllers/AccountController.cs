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
using Demo.Application.Services.IServices;
using Demo.Web.ViewModels;
using MongoDB.Bson.IO;
using Demo.Web.Extensions;
using Demo.Core.Enums;

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
        private readonly IMailService _mailService;

        [TempData]
        public string ErrorMessage { get; set; }

        public AccountController(ILogger<AccountController> logger,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IUserGroupManager userGroupManager,
            IUserRepository userRepository,
            IMailService mailService
            )
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _userGroupManager = userGroupManager;
            _userRepository = userRepository;
            _mailService = mailService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
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
            ViewBag.ReturnUrl = returnUrl;
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
                    TempData[TempDataKey.Success] = TempDataMessage.LoginSuccess;
                    return RedirectToAction("Login");
                }
                TempData[TempDataKey.Success] = TempDataMessage.LoginSuccess;
                returnUrl = returnUrl ?? "/";
                return Redirect(returnUrl);
            }
            ModelState.AddModelError("", "Sai mật khẩu hoặc tên đăng nhập.");
            TempData[TempDataKey.Error] = TempDataMessage.GeneralError;
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        #region Đăng ký
        [AllowAnonymous]
        public async Task<IActionResult> PhoneRegister(string returnUrl)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var hasAdminPermission = _userGroupManager.HasPermission(User.Identity.Name, new string[]
                {
                    RoleList.Admin, RoleList.Account, RoleList.Product, RoleList.Content
                });
                return hasAdminPermission ? RedirectToAction("Index", "Home", new { Area = "Admin" }) : RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PhoneRegister(PhoneRegisterViewModel model, string returnUrl)
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
                PhoneNumber = model.Phone,
                Created = DateTimeExtensions.UTCNowVN
            };
            await _userManager.CreateAsync(admin, model.Password);

            var result = await _signInManager.PasswordSignInAsync(model.Phone, model.Password, true, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                returnUrl = returnUrl ?? "/";
                return Redirect(returnUrl);
            }
            TempData["success"] = true;
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult UsernameRegister(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> UsernameRegister(RegisterViewModel model, string? returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existedUser = _userRepository.Find(x => x.Email == model.Email).ToList();
                    if(existedUser != null)
                    {
                        TempData[TempDataKey.Warning] = TempDataMessage.DuplicateEmail;
                        return View(model);
                    }
                    
                    HttpContext.Session.SetObject("RegisterData", model); // đẩy data vào session và lấy lại khi xác thực mã thành công

                    // tạo mã xác thực và gửi đến mail đăng ký
                    var code = GenerateVerificationCode();
                    HttpContext.Session.SetString("VerificationCode", code);
                    HttpContext.Session.SetString("EmailToVerify", model.Email);
                    _mailService.RegisterVerification(model.Email, code);
                    ViewBag.ReturnUrl = returnUrl;

                    TempData[TempDataKey.Success] = TempDataMessage.RegisterSuccess;
                    return RedirectToAction("RegisterVerify", new { returnUrl, model.Email });
                }
            }
            catch (Exception)
            {
                TempData[TempDataKey.Error] = TempDataMessage.GeneralError;
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra, liên hệ nhà phát triển phần mềm để được hỗ trợ.");
            }
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult RegisterVerify(string email, string returnUrl)
        {
            var model = new RegisterVerifyViewModel { Email = email };
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterVerify(RegisterVerifyViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra mã xác minh từ session/tempdata/db
            var expectedCode = HttpContext.Session.GetString("VerificationCode");
            if (model.Code == expectedCode)
            {
                TempData[TempDataKey.Success] = TempDataMessage.VerifySuccess;

                // tạo người dùng
                var registerData = HttpContext.Session.GetObject<RegisterViewModel>("RegisterData");

                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    IsLocked = false
                };
                var result = await _userManager.CreateAsync(user, registerData.Password);
                if (result.Succeeded)
                {
                    returnUrl = returnUrl ?? "/";
                    return Redirect(returnUrl);
                }
                AddErrors(result);
                TempData[TempDataKey.Error] = TempDataMessage.GeneralError;
                return View();
            }
            else
            {
                TempData[TempDataKey.Error] = TempDataMessage.VerifyCodeNotMatched;
                ModelState.AddModelError("", "Mã xác minh không đúng.");
                return View(model);
            }
        }

        #endregion

        #region User
        public ActionResult MyProfile()
        {
            var user = _userRepository.GetByUsername(User.Identity.Name);
            var model = new UserViewModel()
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Name = user.FullName
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult MyProfile(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _userRepository.GetByUsername(User.Identity.Name);
                user.PhoneNumber = model.PhoneNumber;
                user.FullName = model.Name;
                _userRepository.UpdateAsync(user);
                return RedirectToAction("Profile");
            }
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
        #endregion

        #region Private
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

        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
        #endregion


    }
}
