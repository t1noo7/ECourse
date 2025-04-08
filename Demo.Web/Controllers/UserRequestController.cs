using Demo.Application.Repositories;
using Demo.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Controllers
{
    public class UserRequestController : Controller
    {
        private readonly ILogger<UserRequestController> _logger;
        private readonly IUserRequestRepository _userRequestRepository;

        public UserRequestController(ILogger<UserRequestController> logger,
            IUserRequestRepository userRequestRepository)
        {
            _logger = logger;
            _userRequestRepository = userRequestRepository;
        }

        [HttpPost]
        public IActionResult Index(UserRequest model)
        {
            try
            {
                model.Id = Guid.NewGuid();
                model.Created = DateTime.Now;
                model.Modified = DateTime.Now;

                _userRequestRepository.AddAsync(model);
                TempData["Success"] = "Bạn đã gửi yêu cầu thành công!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error");
                TempData["Error"] = "Có lỗi xảy ra, vui lòng thử lại.";
                return RedirectToAction("Index", "Home");

            }
        }
    }
}
