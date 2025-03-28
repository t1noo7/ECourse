using Demo.Application.Repositories;
using Demo.Database.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Components
{
    public class CourseViewComponent : ViewComponent
    {
        private readonly ICourseRepository _courseRepository;

        public CourseViewComponent(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public IViewComponentResult Invoke()
        {
            var model = _courseRepository.Find(x => x.Deleted == false && x.Status == true).Take(3).ToList();
            return View(model);
        }
    }
}
