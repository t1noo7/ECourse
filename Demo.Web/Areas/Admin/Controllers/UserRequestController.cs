using Demo.Application.Repositories;
using Demo.Core.Enums;
using Demo.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Areas.Admin.Controllers
{
    //[WebAuthorize(RoleList.Sale, RoleList.Admin)]
    [Area("Admin")]
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

        public IActionResult Index()
        {

            var model = _userRequestRepository.Find(x => x.Deleted == false).OrderByDescending(x => x.Created).ToList();
            return View(model);
        }

        public IActionResult Details(Guid id)
        {
            try
            {
                var model = _userRequestRepository.Find(x => x.Id == id).FirstOrDefault();
                return PartialView("_DetailsPartialView", model);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error");
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> ChangeStatus(Guid id, bool status)
        {
            try
            {
                var model = await _userRequestRepository.GetAsync(id);
                model.Status = status;
                await _userRequestRepository.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error");
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid id, string returnUrl)
        {
            await _userRequestRepository.SetAsync(id, nameof(UserRequest.Deleted), true);
            if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
            else return Redirect(returnUrl);
        }
    }
}
