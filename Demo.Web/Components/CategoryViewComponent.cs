using Demo.Application.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Components
{
    public class CategoryViewComponent : ViewComponent
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryViewComponent(ICategoryRepository categoroyRepository)
        {
            _categoryRepository = categoroyRepository;
        }
        public IViewComponentResult Invoke()
        {
            var model = _categoryRepository.Find(x => x.Deleted == false && x.Status == true).ToList();
            return View(model);
        }
    }
}
