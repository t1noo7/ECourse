using Microsoft.AspNetCore.Mvc;
using Demo.Common.Extensions;
using Demo.Application.Extensions;
using Demo.Application.Infrastructures;
using Demo.Core.Permission;
using Demo.Core.Models;
using Demo.Web.Filters;
using Demo.Application.Services.IServices;

namespace Demo.Web.Areas.Admin.Controllers
{
    /*[WebAuthorize(RoleList.Admin)]*/
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly ISystemParameters _systemParameters;
        private readonly IMailService _mailService;

        public AdminController(ILogger<AdminController> logger,
            ISystemParameters systemParameters,
            IMailService mailService
            )
        {
            _logger = logger;
            _systemParameters = systemParameters;
            _mailService = mailService;
        }

        public IActionResult SystemParameters()
        {
            return View(_systemParameters.GetValues());
        }

        public IActionResult EditSystemParameter(string name)
        {
            var value = _systemParameters.GetValue(name);
            var data = PropertyExtensions.GetDataTypes<ISystemParameters>().FirstOrDefault(m => m.DataName == name);
            var model = new SystemParamData { DataName = name, DataValue = value, Type = data?.Type, Description = data?.Description };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSystemParameter(SystemParamData model, string dataValue)
        {
            _systemParameters.SetValue(model.DataName, dataValue);
            return RedirectToAction(nameof(SystemParameters));
        }

        [HttpGet]
        public async Task<IActionResult> SchedulerJobs()
        {
            return View();
        }
    }
}
