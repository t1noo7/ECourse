using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Components
{
    public class ContactViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
