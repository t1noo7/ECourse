using Demo.Application.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Components
{
    public class LessonViewComponent : ViewComponent
    {
        private readonly ILessonRepository _lessonRepository;

        public LessonViewComponent(ILessonRepository lessonRepository)
        {
            _lessonRepository = lessonRepository;
        }

        public IViewComponentResult Invoke(Guid? lessonId)
        {
            var model = _lessonRepository.Find(x => x.Deleted == false && x.Id == lessonId).SingleOrDefault();
            return View(model);
        }
    }
}
