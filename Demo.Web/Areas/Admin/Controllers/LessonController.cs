using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Demo.Common.Extensions;
using Demo.Application.Repositories;
using Demo.Web.Filters;
using Demo.Core.Permission;
using Demo.Core.Models;
using Demo.Web.Helpers;
using Demo.Database.Repositories;
using Demo.Web.Areas.Admin.Models;
using Demo.Application.Models;
using Demo.Application.Infrastructures;
using Demo.Core.Enums;

namespace Demo.Web.Areas.Admin.Controllers
{
    [WebAuthorize(RoleList.Content, RoleList.Product, RoleList.Admin)]
    [Area("Admin")]
    public class LessonController : Controller
    {
        private readonly ILogger<LessonController> _logger;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IFileService _fileService;

        public LessonController(ILogger<LessonController> logger,
            ILessonRepository lessonRepository, 
            ICourseRepository courseRepository,
            IFileService fileService)
        {
            _logger = logger;
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
            _fileService = fileService;
        }

        public IActionResult Index(LessonFilter model, int page = 1)
        {
            List<LessonViewModel> lessonViewModels = new List<LessonViewModel>();

            var lessons = _lessonRepository.Find(x => !x.Deleted &&
                                                      (model.CourseId == Guid.Empty || x.CourseId == model.CourseId))
                                           .ToList();

            var courseIds = lessons.Select(x => x.CourseId).Distinct().ToList();
            var courses = _courseRepository.Find(x => courseIds.Contains(x.Id)).ToList();

            if (!string.IsNullOrEmpty(model.CourseName))
            {
                var filteredCourseIds = courses.Where(c => c.Title == model.CourseName.Trim())
                                               .Select(c => c.Id)
                                               .ToList();
                lessons = lessons.Where(l => filteredCourseIds.Contains(l.CourseId)).ToList();
            }

            lessonViewModels = lessons.Select(lesson => new LessonViewModel
            {
                Id = lesson.Id,
                Title = lesson.Title,
                CourseId = lesson.CourseId,
                CourseName = courses.FirstOrDefault(c => c.Id == lesson.CourseId)?.Title ?? "Không xác định",
                Created = lesson.Created
            }).ToList();

            var pagedResult = lessonViewModels.GetPaged(page);
            return View(pagedResult);
        }


        public IActionResult Edit(Guid? id)
        {
            Lesson? model = null;
            var lscourse = _courseRepository.Find(x => x.Deleted == false).ToList();

            if (id.HasValue)
            {
                model = _lessonRepository.Get(id.Value);
            }

            if (model == null)
            {
                model = new Lesson
                {
                    Id = Guid.NewGuid()
                };
            }
            ViewBag.Courses = lscourse;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Lesson model, IFormFile videoInput, string returnUrl)
        {
            try
            {
                if (!ModelState.IsValid && (!ModelState.ContainsKey("returnUrl") && !ModelState.ContainsKey("videoInput")))
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

                // sử dụng ảnh thumbnail youtube
                /*if (!string.IsNullOrEmpty(model.YouTubeUrl))
                {
                    var videoId = ExtractYouTubeVideoId(model.YouTubeUrl);
                    model.Image = $"https://img.youtube.com/vi/{videoId}/hqdefault.jpg";
                }*/

                if (videoInput != null)
                {
                    using var videoStream = videoInput.OpenReadStream();
                    var videoFileName = $"{model.Id}{Path.GetExtension(videoInput.FileName)}";
                    model.VideoPath = _fileService.UpsertVideo("lessonVideos", videoFileName, videoStream);
                }

                if (string.IsNullOrEmpty(model.FriendlyUrl))
                {
                    var url = StringHelpers.ToFriendlyUrl(model.Title);
                    if (_lessonRepository.Find(x => x.FriendlyUrl == url && x.Deleted != true).FirstOrDefault() != null)
                    {
                        do
                        {
                            model.FriendlyUrl = url + "-" + new Random().Next(1, 100);
                        }
                        while (_lessonRepository.Find(x => x.FriendlyUrl == model.FriendlyUrl && x.Deleted != true).FirstOrDefault() != null);
                    }
                    else
                    {
                        model.FriendlyUrl = url;
                    }
                }

                await _lessonRepository.UpsertAsync(model);
                TempData[TempDataKey.Success] = TempDataMessage.UpdateSuccess;
                if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
                else return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving");
                TempData[TempDataKey.Error] = TempDataMessage.GeneralError;
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(Guid id, string returnUrl)
        {
            try
            {
                await _lessonRepository.SetAsync(id, nameof(Lesson.Deleted), true);
                TempData[TempDataKey.Success] = TempDataMessage.DeleteSuccess;
                if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
                else return Redirect(returnUrl);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error while saving");
                TempData[TempDataKey.Error] = TempDataMessage.GeneralError;
                return RedirectToAction(nameof(Index));
            }
            
        }

        private string ExtractYouTubeVideoId(string url)
        {
            var uri = new Uri(url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return query["v"];
        }
    }
}
