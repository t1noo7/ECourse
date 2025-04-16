using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Demo.Common.Extensions;
using Demo.Application.Repositories;
using Demo.Web.Filters;
using Demo.Core.Permission;
using Demo.Core.Models;
using Demo.Web.Helpers;
using Microsoft.Extensions.Logging;
using Demo.Database.Repositories;
using Demo.Application.Services.IServices;

namespace Demo.Web.Areas.Admin.Controllers
{
    [WebAuthorize(RoleList.Content, RoleList.Admin)]
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ILogger<CategoryController> _logger;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFileService _fileService;

        public CategoryController(ILogger<CategoryController> logger,
            ICategoryRepository categoriesRepository,
            IFileService fileService)
        {
            _logger = logger;
            _categoryRepository = categoriesRepository;
            _fileService = fileService;
        }

        public IActionResult Index(int page = 1)
        {
            var categories = _categoryRepository.Find(x => x.Deleted == false).ToList();

            var pagedResult = categories.GetPaged(page);
            return View(pagedResult);
        }

        public IActionResult Edit(Guid? id)
        {
            Category? model = null;
            var Categories = _categoryRepository.Find(x => x.Deleted == false).ToList();
            // var Lesson =
            if (id.HasValue)
            {
                model = _categoryRepository.Get(id.Value);
            }
            if (model == null)
            {
                model = new Category();
            }
            ViewBag.Categories = Categories;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category model, IFormFile fileInput, string returnUrl)
        {
            if (!ModelState.IsValid && (!ModelState.ContainsKey("returnUrl") && !ModelState.ContainsKey("fileInput")))
            {
                return View(model);
            }

            model.ModifiedBy = User?.Identity?.Name;
            model.Modified = DateTimeExtensions.UTCNowVN;

            if (model.Id == Guid.Empty)
            {
                model.CreatedBy = model.ModifiedBy;
                model.Created = DateTimeExtensions.UTCNowVN;
            }

            if (fileInput != null)
            {
                model.Image = _fileService.ResizeImageJpeg(fileInput.OpenReadStream(), 628, 300, "categories", $"{model.Id}.thumb.png");
            }

            await (model.Id == Guid.Empty ? _categoryRepository.UpsertAsync(model) : _categoryRepository.UpdateAsync(model));

            if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
            else return Redirect(returnUrl);
        }

        public async Task<IActionResult> ChangeStatus(Guid id, bool status)
        {
            try
            {
                var model = await _categoryRepository.GetAsync(id);
                model.Status = status;
                await _categoryRepository.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid id, string returnUrl)
        {
            await _categoryRepository.SetAsync(id, nameof(Category.Deleted), true);
            if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
            else return Redirect(returnUrl);
        }
    }
}
