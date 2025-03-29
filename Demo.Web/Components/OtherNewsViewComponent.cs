using Demo.Application.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Components
{
    public class OtherNewsViewComponent : ViewComponent
    {
        private readonly INewRepository _newRepository;

        public OtherNewsViewComponent(INewRepository newRepository)
        {
            _newRepository = newRepository;
        }
        public IViewComponentResult Invoke(Guid currentNewId, DateTime currentNewCreatedDate)
        {
            var numberOfNews = 4;
            var news = _newRepository.Find(x => x.Id != currentNewId && x.Created >= currentNewCreatedDate && x.Status && x.Deleted == false).Take(numberOfNews).ToList();
            if (news.Count < numberOfNews)
            {
                var remainNews = _newRepository.Find(x => x.Id != currentNewId && x.Created < currentNewCreatedDate && x.Status && x.Deleted == false)
                    .OrderByDescending(y => y.Created).Take(numberOfNews - news.Count).ToList();
                news.AddRange(remainNews);
            }

            return View(news);
        }
    }
}
