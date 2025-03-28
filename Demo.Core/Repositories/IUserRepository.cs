using Demo.Core.Models;
using Demo.Core.Permission;

namespace Demo.Core.Repositories
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User> GetByIdAsync(string id);
        User GetByUsername(string username);
        Task<List<User>> FindAsync(FilterModel filter);
    }
}