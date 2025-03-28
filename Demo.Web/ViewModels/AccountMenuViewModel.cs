namespace Demo.Web.ViewModels
{
    public class AccountMenuViewModel
    {
        public bool IsAuthenticated { get; set; }
        public bool AdminPermission { get; set; }
        public string Username { get; set; }
        public bool IsExternalLogin { get; set; }
    }
}
