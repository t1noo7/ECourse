
using Demo.Application.Repositories;
using Demo.Application.Services.IServices;
using Demo.Common.Extensions;
using Demo.Core.Models;
using Demo.Core.Permission;
using Demo.Database.Repositories;
using Demo.Web.Filters;
using Demo.Web.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Demo.Web.Areas.Admin.Controllers
{
    //[WebAuthorize(RoleList.Admin, RoleList.Product)]
    [Area("Admin")]
    public class CourseController : Controller
    {
        private readonly ILogger<CourseController> _logger;
        private readonly ICourseRepository _courseRepository;
        private readonly IFileService _fileService;

        public CourseController(ILogger<CourseController> logger, ICourseRepository courseRepository, IFileService fileService)
        {
            _logger = logger;
            _courseRepository = courseRepository;
            _fileService = fileService;
        }
        public IActionResult Index(int page = 1)
        {
            var courses = _courseRepository.Find(x => x.Deleted == false).ToList();

            var pagedResult = courses.GetPaged(page);
            return View(pagedResult);
        }

        public IActionResult Edit(Guid? id)
        {
            Course? model = null;
            if (id.HasValue)
            {
                model = _courseRepository.Get(id.Value);
            }
            if (model == null)
            {
                model = new Course();
                model.Id = Guid.NewGuid();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Course model, IFormFile fileInput, string returnUrl)
        {
            try
            {
                if (!ModelState.IsValid && (!ModelState.ContainsKey("returnUrl") && !ModelState.ContainsKey("fileInput")))
                {
                    return View(model);
                }

                model.ModifiedBy = User?.Identity?.Name;
                model.Modified = DateTimeExtensions.UTCNowVN;

                bool isExist = false;
                if (model.Id != Guid.Empty && model.Id != null)
                {
                    isExist = _courseRepository.Find(x => x.Id == model.Id && x.Deleted != true).FirstOrDefault() != null;

                    if (!isExist)
                    {
                        model.CreatedBy = model.ModifiedBy;
                        model.Created = DateTimeExtensions.UTCNowVN;
                    }
                }

                if (fileInput != null)
                {
                    model.Image = _fileService.ResizeImageJpeg(fileInput.OpenReadStream(), 360, 230, "courses", $"{model.Id}.thumb.png");
                }

                if (String.IsNullOrEmpty(model.FriendlyUrl))
                {
                    var url = StringHelpers.ToFriendlyUrl(model.Title);
                    if (_courseRepository.Find(x => x.FriendlyUrl == url && x.Deleted != true).FirstOrDefault() != null)
                    {
                        do
                        {
                            model.FriendlyUrl = url + "-" + new Random().Next(1, 100);
                        }
                        while (_courseRepository.Find(x => x.FriendlyUrl == model.FriendlyUrl && x.Deleted != true).FirstOrDefault() != null);
                    }
                    else
                    {
                        model.FriendlyUrl = url;
                    }
                }

                if (isExist)
                {
                    await _courseRepository.UpdateAsync(model);
                }
                else
                {
                    await _courseRepository.UpsertAsync(model);
                }

                if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
                else return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving course");
                return View(model);
            }
        }

        public async Task<IActionResult> ChangeStatus(Guid id, bool status)
        {
            try
            {
                var model = await _courseRepository.GetAsync(id);
                model.Status = status;
                await _courseRepository.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid id, string returnUrl)
        {
            await _courseRepository.SetAsync(id, nameof(Course.Deleted), true);
            if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
            else return Redirect(returnUrl);
        }
    }
}