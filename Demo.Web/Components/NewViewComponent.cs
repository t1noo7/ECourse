using Demo.Application.Repositories;
using Demo.Database.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Components
{
    public class NewViewComponent : ViewComponent
    {
        private readonly INewRepository _newRepository;

        public NewViewComponent(INewRepository newRepository)
        {
            _newRepository = newRepository;
        }
        public IViewComponentResult Invoke()
        {
            var model = _newRepository.Find(x => x.Deleted != true && x.Status == true).Take(4).ToList();
            return View(model);
        }
    }
}
