// using Microsoft.AspNetCore.Mvc;
// using Demo.Application.Infrastructures;
// using Demo.Application.Repositories;
// using Demo.Application.Services;
// using Demo.Core.Permission;
// using Demo.Core.Models;
// using Demo.Web.Filters;
// using Demo.Web.Models;

// namespace Demo.Web.Areas.Admin.Controllers
// {
//     [Area("Admin")]
//     [WebAuthorize("Quản lý Sale", RoleList.Sale, RoleList.Admin)]
//     public class GeneralItemController : Controller
//     {
//         private readonly ILogger<GeneralItemController> _logger;
//         private readonly IFileService _fileService;
//         private readonly IOrderService _orderService;

//         public GeneralItemController(ILogger<GeneralItemController> logger,
//             IFileService fileService,
//             IOrderService orderService)
//         {
//             _logger = logger;
//             _fileService = fileService;
//             _orderService = orderService;
//         }

//         public IActionResult Sales()
//         {
//             var items = _generalItemRepository.GetAll().Where(m => !m.Deleted && m.Type == "Sale").OrderByDescending(m => m.Created).ToList();
//             return View(items);
//         }

//         public IActionResult EditSale(Guid? id)
//         {
//             GeneralItem? model = null;
//             if (id.HasValue)
//             {
//                 model = _generalItemRepository.Get(id.Value);
//             }
//             if (model == null)
//             {
//                 model = new GeneralItem { Created = DateTime.UtcNow.AddHours(7), Type = "Sale" };
//             }
//             return View(model);
//         }

//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public Task<IActionResult> EditSale(GeneralItem model, IFormFile fileInput, string returnUrl) => Submit(model, fileInput, "sales", returnUrl);

//         public async Task<IActionResult> Delete(Guid id, string returnUrl)
//         {
//             await _generalItemRepository.SetAsync(id, m => m.Deleted, true);
//             if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Farmers));
//             else return Redirect(returnUrl);
//         }
//     }
// }
