using Demo.Application.Repositories;
using Demo.Application.Services;
using Demo.Common.Extensions;
using Demo.Core.Models;
using Demo.Core.Permission;
using Demo.Database.Repositories;
using Demo.Web.Filters;
using Demo.Web.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Areas.Admin.Controllers
{
    //[WebAuthorize(RoleList.Content, RoleList.Admin)]
    [Area("Admin")]
    public class NewController : Controller
    {
        private readonly ILogger<NewController> _logger;
        private readonly INewRepository _newRepository;
        private readonly IFileService _fileService;

        public NewController(ILogger<NewController> logger, INewRepository newRepository, IFileService fileService)
        {
            _logger = logger;
            _newRepository = newRepository;
            _fileService = fileService;
        }
        public IActionResult Index()
        {

            var news = _newRepository.Find(x => x.Deleted == false).ToList();
            return View(news);
        }

        public IActionResult Edit(Guid? id)
        {
            New? model = null;
            if (id.HasValue)
            {
                model = _newRepository.Get(id.Value);
            }
            if (model == null)
            {
                model = new New();
                model.Id = Guid.NewGuid();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(New model, IFormFile fileInput, string returnUrl)
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
                    isExist = _newRepository.Find(x => x.Id == model.Id && x.Deleted != true).FirstOrDefault() != null;

                    if (!isExist)
                    {
                        model.CreatedBy = model.ModifiedBy;
                        model.Created = DateTimeExtensions.UTCNowVN;
                        model.Author = User?.Identity?.Name;
                    }
                }

                if (fileInput != null)
                {
                    model.Image = _fileService.ResizeImageJpeg(fileInput.OpenReadStream(), 360, 230, "news", $"{model.Id}.thumb.png");
                }

                if (String.IsNullOrEmpty(model.FriendlyUrl))
                {
                    var url = StringHelpers.ToFriendlyUrl(model.Title);
                    if (_newRepository.Find(x => x.FriendlyUrl == url && x.Deleted != true).FirstOrDefault() != null)
                    {
                        do
                        {
                            model.FriendlyUrl = url + "-" + new Random().Next(1, 100);
                        }
                        while (_newRepository.Find(x => x.FriendlyUrl == model.FriendlyUrl && x.Deleted != true).FirstOrDefault() != null);
                    }
                    else
                    {
                        model.FriendlyUrl = url;
                    }
                }

                if (isExist)
                {
                    await _newRepository.UpdateAsync(model);
                }
                else
                {
                    await _newRepository.UpsertAsync(model);
                }

                if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
                else return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving");
                return View(model);
            }
        }

        public async Task<IActionResult> ChangeStatus(Guid id, bool status)
        {
            try
            {
                var model = await _newRepository.GetAsync(id);
                model.Status = status;
                await _newRepository.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid id, string returnUrl)
        {
            await _newRepository.SetAsync(id, nameof(Course.Deleted), true);
            if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
            else return Redirect(returnUrl);
        }
    }
}
