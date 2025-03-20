using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Components
{
    public class AboutUsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
