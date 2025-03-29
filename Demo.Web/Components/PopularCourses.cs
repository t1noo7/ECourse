using Demo.Application.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Components
{
    public class PopularCoursesViewComponent : ViewComponent
    {
        private readonly ICourseRepository _courseRepository;

        public PopularCoursesViewComponent(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }
        public IViewComponentResult Invoke()
        {
            var model = _courseRepository.Find(x=> x.Deleted != true && x.Status).Take(4).OrderByDescending(x => x.Created).ToList();
            return View(model);
        }
    }
}
