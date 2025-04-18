using Microsoft.AspNetCore.Mvc;
using Demo.Application.Repositories;
using Demo.Core.Permission;
using Demo.Core.Models;
using Demo.Web.Filters;
using Demo.Common.Extensions;
using Demo.Core.Enums;

namespace Demo.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [WebAuthorize("Quản lý Voucher", RoleList.Admin)]
    public class VoucherController : Controller
    {
        private readonly ILogger<VoucherController> _logger;
        private readonly IVoucherRepository _voucherRepository;

        public VoucherController(ILogger<VoucherController> logger,
            IVoucherRepository voucherRepository)
        {
            _logger = logger;
            _voucherRepository = voucherRepository;
        }

        public IActionResult Index(int page = 1)
        {
            var vouchers = _voucherRepository.GetAll().Where(m => !m.Deleted).OrderByDescending(m => m.Created).ToList();

            var pagedResult = vouchers.GetPaged(page);
            return View(pagedResult);
        }

        public IActionResult Edit(Guid? id)
        {
            Voucher? model = null;
            if (id.HasValue)
            {
                model = _voucherRepository.Get(id.Value);
            }
            if (model == null)
            {
                model = new Voucher { Expired = DateTime.UtcNow.AddHours(7), StartDate = DateTime.UtcNow.AddHours(7) };
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Voucher model, string returnUrl)
        {
            try
            {
                model.ModifiedBy = User?.Identity?.Name;
                model.Modified = DateTime.UtcNow.AddHours(7);
                model.Expired = new DateTime(model.Expired.Year, model.Expired.Month, model.Expired.Day).AddHours(7);
                model.StartDate = new DateTime(model.StartDate.Year, model.StartDate.Month, model.StartDate.Day).AddHours(7);
                if (model.Id == Guid.Empty)
                {
                    model.CreatedBy = model.ModifiedBy;
                    model.Created = DateTime.UtcNow.AddHours(7);
                }
                if (model.StartDate > model.Expired)
                {
                    ModelState.AddModelError("StartDate", "Ngày bắt đầu phải nhỏ hơn ngày hết hạn.");
                    return View(model);
                }
                if (model.Quantity <= 0)
                {
                    ModelState.AddModelError("Quantity", "Số lượng ban đầu phải lớn hơn 0.");
                    return View(model);
                }
                if (model.DiscountRate > 0 && model.DiscountAmount > 0)
                {
                    ModelState.AddModelError("DiscountRate", "Chỉ được chọn giảm giá theo % hoặc giảm giá theo tiền.");
                    return View(model);
                }

                await _voucherRepository.UpsertAsync(model);
                TempData[TempDataKey.Success] = TempDataMessage.UpdateSuccess;
                if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
                else return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving");
                TempData[TempDataKey.Error] = TempDataMessage.GeneralError;
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(Guid id, string returnUrl)
        {
            try
            {
                await _voucherRepository.SetAsync(id, nameof(Voucher.Deleted), true);
                TempData[TempDataKey.Success] = TempDataMessage.DeleteSuccess;
                if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Edit));
                else return Redirect(returnUrl);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error while saving");
                TempData[TempDataKey.Error] = TempDataMessage.GeneralError;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
