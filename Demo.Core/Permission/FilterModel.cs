using Demo.Core.Enums;

namespace Demo.Core.Permission
{
    public class FilterModel
    {
        public string? query { get; set; }
        public string? custom { get; set; }
        public string? status { get; set; }
        public string? orderby { get; set; }
        public bool isAdmin { get; set; }
        public StatusEnum Status { get; set; }
    }
}