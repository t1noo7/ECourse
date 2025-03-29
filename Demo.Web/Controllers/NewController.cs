using Demo.Application.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Controllers
{
    public class NewController : Controller
    {
        private readonly INewRepository _newRepository;
        public NewController(INewRepository newRepository)
        {
            _newRepository = newRepository;
        }

        public IActionResult List()
        {
            var model = _newRepository.Find(x => x.Deleted != true && x.Status == true).OrderByDescending(x => x.Created).ToList();
            return View(model);
        }

        public IActionResult Details(string url)
        {
            var article = _newRepository.Find(x => x.FriendlyUrl == url && x.Deleted == false && x.Status).ToList().FirstOrDefault();
            return View(article);
        }
    }
}
