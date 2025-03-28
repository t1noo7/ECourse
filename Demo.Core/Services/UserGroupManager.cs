using Microsoft.Extensions.Caching.Memory;
using Demo.Core.Repositories;
using Demo.Core.Permission;
using Demo.Core.Models;

namespace Demo.Core.Services
{
    public class UserGroupManager : IUserGroupManager
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IUserRepository _userRepository;
        private readonly IUserGroupRepository _userGroupRepository;
        private const string UserRoleGroupCacheKey = "user-role-group";

        public UserGroupManager(IMemoryCache memoryCache,
            IUserRepository userRepository,
            IUserGroupRepository userGroupRepository)
        {
            _memoryCache = memoryCache;
            _userRepository = userRepository;
            _userGroupRepository = userGroupRepository;
        }
        public bool HasPermission(string username, params string[] parameters)
        {
            var roleGroup = _memoryCache.GetOrCreate($"{UserRoleGroupCacheKey}-{username}", m =>
            {
                var user = _userRepository.GetByUsername(username);
                return new
                {
                    Roles = user?.CustomRoles ?? new List<string>(),
                    Groups = user?.Groups ?? new List<Guid>()
                };
            });
            var hasRole = roleGroup.Roles.Intersect(parameters).Any();
            if (hasRole) return true;

            foreach (var groupID in roleGroup.Groups)
            {
                var group = GetGroup(groupID);
                if (group?.Roles?.Intersect(parameters)?.Any() == true) return true;
            }
            return false;
        }

        public List<string> GetAllRoles(string username)
        {
            var roleGroup = _memoryCache.GetOrCreate($"{UserRoleGroupCacheKey}-{username}", m =>
            {
                var user = _userRepository.GetByUsername(username);
                return new
                {
                    Roles = user?.CustomRoles ?? new List<string>(),
                    Groups = user?.Groups ?? new List<Guid>()
                };
            });

            foreach (var groupID in roleGroup.Groups)
            {
                var group = GetGroup(groupID);
                roleGroup.Roles.AddRange(group.Roles);
            }
            return roleGroup.Roles.Distinct().ToList();
        }

        public UserGroup GetGroup(Guid groupId) => _memoryCache.GetOrCreate($"group-{groupId}", m => _userGroupRepository.Get(groupId));

        public User GetUser(string username) => _memoryCache.GetOrCreate($"user-{username}", m => _userRepository.GetByUsername(username));

        public async Task SetUserRolesAsync(string userId, List<string> roles)
        {
            if (roles?.Any() != true) return;

            var user = await _userRepository.GetByIdAsync(userId);
            await _userRepository.SetAsync(userId, nameof(User.CustomRoles), roles);
            _memoryCache.Remove($"{UserRoleGroupCacheKey}-{user.UserName}");
        }

        public async Task SetGroupRolesAsync(Guid groupId, List<string> roles)
        {
            if (roles?.Any() != true) return;

            var group = await _userGroupRepository.GetAsync(groupId);
            await _userGroupRepository.SetAsync(groupId, nameof(User.Roles), roles);
            _memoryCache.Remove($"group-{groupId}");
        }

        public async Task AddUserToGroupAsync(Guid groupId, string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user.Groups.Contains(groupId)) return;

            user.Groups.Add(groupId);

            await _userRepository.SetAsync(userId, nameof(User.Groups), user.Groups);
            _memoryCache.Remove($"{UserRoleGroupCacheKey}-{user.UserName}");
        }
    }
}
