using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Controllers
{
    public class NewController : Controller
    {
        public IActionResult Details()
        {
            return View();
        }
    }
}
