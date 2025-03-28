using Microsoft.AspNetCore.Mvc;
using Demo.Core.Services;
using Demo.Web.ViewModels;

namespace Demo.Web.Components
{
    public class AccountMenuListViewComponent : ViewComponent
    {
        private readonly IUserGroupManager _userGroupManager;

        public AccountMenuListViewComponent(IUserGroupManager userGroupManager)
        {
            _userGroupManager = userGroupManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var accountMenu = new AccountMenuViewModel()
            {
                IsAuthenticated = User.Identity?.IsAuthenticated == true,
                AdminPermission = User.Identity?.IsAuthenticated == true && _userGroupManager.GetAllRoles(User.Identity.Name).Any(),
                Username = User.Identity?.Name,
                IsExternalLogin = User.Identity?.IsAuthenticated == true && _userGroupManager.GetUser(User.Identity.Name)?.Logins?.Any() == true
            };
            return View(accountMenu);
        }
    }
}
