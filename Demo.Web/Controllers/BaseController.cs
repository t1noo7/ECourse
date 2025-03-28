using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Controllers
{
    public class Demo : Controller
    {
        protected string GetModalStateErrorMsg()
        {
            if (!ModelState.IsValid)
            {
                return string.Join("<br />", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
            }
            return string.Empty;
        }
    }
}
