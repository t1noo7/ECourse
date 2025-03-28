using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Demo.Core.Models;
using Demo.Application.Repositories;
using Demo.Web.Helpers;
using Demo.Web.Filters;
using Demo.Core.Permission;
using Demo.Common.Extensions;
using Demo.Application.Services;

namespace Demo.Web.Areas.Admin.Controllers
{
    [WebAuthorize(RoleList.Admin, RoleList.Customer, RoleList.Sale)]
    [Area("Admin")]
    public class ChapterController : Controller
    {
        private readonly ILogger<ChapterController> _logger;
        private readonly IChapterRepository _chapterRepository;
        private readonly ICourseRepository _courseRepository;

        public ChapterController(ILogger<ChapterController> logger,
            IChapterRepository chapterRepository, ICourseRepository courseRepository)
        {
            _logger = logger;
            _chapterRepository = chapterRepository;
            _courseRepository = courseRepository;

        }

        public IActionResult Index(Guid courseId)
        {
            if(courseId != Guid.Empty)
            {
                var chapters = _chapterRepository.Find(x => x.Deleted == false && x.CourseId == courseId).ToList();
                return View(chapters);
            }
            else
            {
                var chapters = _chapterRepository.Find(x => x.Deleted == false).ToList();
                return View(chapters);
            }
        }

        public IActionResult Edit(Guid? id)
        {
            Chapter? model = null;
            var course = _courseRepository.Find(x => x.Deleted == false).ToList();
            if (id.HasValue)
            {
                model = _chapterRepository.Get(id.Value);
            }
            if (model == null)
            {
                model = new Chapter();
                model.Id = Guid.NewGuid();
            }
            ViewBag.Courses = course;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Chapter model, string returnUrl)
        {
            try
            {
                if (!ModelState.IsValid && (!ModelState.ContainsKey("returnUrl") && !ModelState.ContainsKey("fileInput")))
                {
                    return View(model);
                }
                model.ModifiedBy = User?.Identity?.Name;
                model.Modified = DateTimeExtensions.UTCNowVN;
                if (model.Id != Guid.Empty && model.Id != null)
                {
                    model.CreatedBy = model.ModifiedBy;
                    model.Created = DateTimeExtensions.UTCNowVN;
                }

                if (String.IsNullOrEmpty(model.FriendlyUrl))
                {
                    var url = StringHelpers.ToFriendlyUrl(model.Title);
                    if (_chapterRepository.Find(x => x.FriendlyUrl == url && x.Deleted != true).FirstOrDefault() != null)
                    {
                        do
                        {
                            model.FriendlyUrl = url + "-" + new Random().Next(1, 100);
                        }
                        while (_chapterRepository.Find(x => x.FriendlyUrl == model.FriendlyUrl && x.Deleted != true).FirstOrDefault() != null);
                    }
                    else
                    {
                        model.FriendlyUrl = url;
                    }
                }
                await _chapterRepository.UpsertAsync(model);
                if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
                else return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving");
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(Guid id, string returnUrl)
        {
            await _chapterRepository.SetAsync(id, nameof(Chapter.Deleted), true);
            if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
            else return Redirect(returnUrl);
        }
    }
}