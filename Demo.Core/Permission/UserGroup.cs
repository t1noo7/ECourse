using Demo.Core.Models;

namespace Demo.Core.Permission
{
    public class UserGroup : BaseEntity
    {
        public string? Name { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}