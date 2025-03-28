using Demo.Application.Repositories;
using Demo.Application.Services;
using Demo.Common.Extensions;
using Demo.Core.Models;
using Demo.Core.Permission;
using Demo.Core.Repositories;
using Demo.Database.Repositories;
using Demo.Web.Filters;
using Demo.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Demo.Web.Areas.Admin.Controllers
{
    //[WebAuthorize(RoleList.Content, RoleList.Admin)]
    [Area("Admin")]
    public class BannerController : Controller
    {
        private readonly ILogger<BannerController> _logger;
        private readonly IBannerRepository _bannerRepository;
        private readonly IFileService _fileService;

        public BannerController(ILogger<BannerController> logger,
            IBannerRepository BannerRepository,
            IFileService fileService)
        {
            _logger = logger;
            _bannerRepository = BannerRepository;
            _fileService = fileService;
        }

        public IActionResult Index()
        {
            var banners = _bannerRepository.Find(x => x.Deleted == false).ToList();
            return View(banners);
        }

        public IActionResult Edit(Guid? id)
        {
            Banner? model = null;
            if (id.HasValue)
            {
                model = _bannerRepository.Get(id.Value);
            }
            if (model == null)
            {
                model = new Banner();
                model.Id = Guid.NewGuid();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Banner model, IFormFile fileInput, string returnUrl)
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
                    isExist = _bannerRepository.Find(x => x.Id == model.Id && x.Deleted != true).FirstOrDefault() != null;

                    if (!isExist)
                    {
                        model.CreatedBy = model.ModifiedBy;
                        model.Created = DateTimeExtensions.UTCNowVN;

                        // tăng thứ tự banner lên + 1 nếu thêm mới
                        var lstBanner = _bannerRepository.GetAll().OrderByDescending(b => b.Order);
                        var highestBanner = lstBanner.FirstOrDefault();
                        int maxOrderIndex = highestBanner != null ? highestBanner.Order : 1;
                        model.Order = maxOrderIndex + 1;

                        model.Status = true;
                    }
                }

                if (fileInput != null)
                {
                    model.Image = _fileService.ResizeImageJpeg(fileInput.OpenReadStream(), 1366, 768, "banners", $"{model.Id}.thumb.png");
                }

                if (String.IsNullOrEmpty(model.FriendlyUrl))
                {
                    var url = StringHelpers.ToFriendlyUrl(model.Title);
                    if (_bannerRepository.Find(x => x.FriendlyUrl == url && x.Deleted != true).FirstOrDefault() != null)
                    {
                        do
                        {
                            model.FriendlyUrl = url + "-" + new Random().Next(1, 100);
                        }
                        while (_bannerRepository.Find(x => x.FriendlyUrl == model.FriendlyUrl && x.Deleted != true).FirstOrDefault() != null);
                    }
                    else
                    {
                        model.FriendlyUrl = url;
                    }
                }

                if (isExist)
                {
                    await _bannerRepository.UpdateAsync(model);
                }
                else
                {
                    await _bannerRepository.UpsertAsync(model);
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
                var model = await _bannerRepository.GetAsync(id);
                model.Status = status;
                await _bannerRepository.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrder([FromBody] Dictionary<string, List<string>> data)
        {
            try
            {
                if (data == null || !data.ContainsKey("ids") || data["ids"] == null)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });

                var ids = data["ids"];
                int order = 1;

                foreach (var id in ids)
                {
                    var banner = await _bannerRepository.GetAsync(Guid.Parse(id));
                    if (banner != null)
                    {
                        banner.Order = order++;
                        await _bannerRepository.UpdateAsync(banner);
                    }
                }
                return Json(new { success = true });
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thứ tự banner");
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> Delete(Guid id, string returnUrl)
        {
            await _bannerRepository.SetAsync(id, nameof(Banner.Deleted), true);
            if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
            else return Redirect(returnUrl);
        }
    }
}
