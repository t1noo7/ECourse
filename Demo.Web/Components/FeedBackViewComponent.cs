using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Components
{
    public class FeedBackViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
