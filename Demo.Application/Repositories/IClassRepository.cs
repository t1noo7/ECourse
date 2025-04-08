using Demo.Core.Models;
using Demo.Core.Repositories;
using Demo.Application.ViewModels;

namespace Demo.Application.Repositories
{
    public interface IClassRepository : IBaseRepository<Class>
    {
        Task<Class?> GetByIdAsync(Guid id);
        Task<List<Class>> GetClassesByCourseIdAsync(Guid courseId);
        Task<List<ApprovedStudentViewModel>> GetApprovedStudentsWithCourseAsync();
        Task<List<User>> GetStudentsInClassAsync(Guid classId);
        Task AddStudentsToClassAsync(Guid classId, List<Guid> userIds);
    }
}
