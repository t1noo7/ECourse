using Demo.Application.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Controllers
{
    public class LessonController : Controller
    {
        private readonly ILessonRepository _lessonRepository;

        public LessonController(ILessonRepository lessonRepository)
        {
            _lessonRepository = lessonRepository;
        }
        public IActionResult MyLessons(Guid courseId)
        {
            var lessons = _lessonRepository.Find(x => x.CourseId == courseId).ToList();
            return View(lessons);
        }

        public IActionResult LoadLessonContent(Guid lessonId)
        {
            return ViewComponent("Lesson", new { lessonId });
        }
    }
}
