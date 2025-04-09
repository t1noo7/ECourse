using Demo.Application.Repositories;
using Demo.Application.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Controllers
{
    public class LessonController : Controller
    {
        private readonly ILogger<LessonController> _logger;
        private readonly ILessonRepository _lessonRepository;
        private readonly ISystemParameters _systemParameters;

        public LessonController(ILogger<LessonController> logger,
            ILessonRepository lessonRepository,
            ISystemParameters systemParameters)
        {
            _logger = logger;
            _lessonRepository = lessonRepository;
            _systemParameters = systemParameters;
        }
        public IActionResult MyLessons(Guid courseId)
        {
            var lessons = _lessonRepository.Find(x => x.CourseId == courseId).ToList();

            // Lấy tham số RewatchTime từ hệ thống để check các bài giảng còn thời gian xem lại
            var rewatchTimeParam = _systemParameters.GetValue(nameof(ISystemParameters.RewatchTime));
            int.TryParse(rewatchTimeParam?.ToString(), out var rewatchMonths);

            var now = DateTime.Now;

            var result = lessons
                .Where(lesson =>
                {
                    if (rewatchMonths == -1)
                        return true; // không giới hạn thì luôn hiển thị

                    var expiryDate = lesson.Created.AddMonths(rewatchMonths);
                    return now <= expiryDate; // chỉ hiển thị nếu chưa hết hạn
                }).ToList();

            return View(result);
        }


        public IActionResult LoadLessonContent(Guid lessonId)
        {
            return ViewComponent("Lesson", new { lessonId });
        }
    }
}
