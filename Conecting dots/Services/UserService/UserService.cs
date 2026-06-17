using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Services.CacheService;
using Microsoft.EntityFrameworkCore;
using System;
using static ConnectingDotsAPI.Models.UserModel;

namespace ConnectingDotsAPI.Services.UserService
{
    public class UserService(ConnectingDotsDbContext db) : IUserService
    {

        private readonly ConnectingDotsDbContext db = db;
        public async Task<List<UserDetails>> GetAll(int userId, string? role)
        {
            User user = db.Users
               .Include(u => u.Roles).ThenInclude(x => x.PagesInRoles).ThenInclude(x => x.Page)
               .Include(u => u.Roles).ThenInclude(x => x.Roles).ThenInclude(x => x.PagesInRoles).ThenInclude(x => x.Page)
               .FirstOrDefault(u => u.Id == userId) ?? throw new Exception("USER NOT FOUND");

            // Check if the user is a super admin
            bool isSuperAdmin = user != null && user.Roles.Any(r => r.SystemName.Equals("superadmin", StringComparison.OrdinalIgnoreCase));

            return await db.Users.Where(x => !x.Deleted && isSuperAdmin ? true : x.ParentUserId == userId).Include(x => x.Roles)
                .Where(x => x.Roles.Any(x => (!string.IsNullOrEmpty(role) && x.SystemName == role.ToLower() )|| true)).Select(_user =>
            new UserDetails
            {
                Active = _user.Active,
                Roles = string.Join(',', _user.Roles.Select(x => x.Name)),
                FirstName = _user.FirstName,
                Guid = _user.Guid,
                LastName = _user.LastName,
                Username = _user.Username,
                ParentUser = $"{db.Users.Where(y => y.Id == _user.ParentUserId).Select(y => y.FirstName).FirstOrDefault()} {db.Users.Where(y => y.Id == _user.ParentUserId).Select(y => y.LastName).FirstOrDefault()}"
            }).Where(x => x.Username != "sa").ToListAsync();
        }
        public async Task<UserDetails?> GetDetails(Guid? guid, int? id)
        {
            return await db.Users.Where(x => (id.HasValue && x.Id == id) || (guid.HasValue && x.Guid == guid))
                .Include(x => x.Roles)
                .Include(x => x.UserPasswords)
                .Include(x => x.ActivityLogs)
                .Select(_user =>
                new UserDetails
                {
                    ActivityLogs = _user.ActivityLogs,
                    Active = _user.Active,
                    Roles = _user.Roles,
                    FirstName = _user.FirstName,
                    Guid = guid,
                    LastName = _user.LastName,
                    UserAuthTokens = _user.UserAuthTokens,
                    Username = _user.Username,
                    Passwords = _user.UserPasswords.OrderByDescending(x => x.CreatedOnUtc).Take(1),
                    ParentUserGuid = db.Users.Where(y => y.Id == _user.ParentUserId).Any() ? db.Users.Where(y => y.Id == _user.ParentUserId).Select(y => y.Guid).FirstOrDefault() : null,

                })
                .FirstOrDefaultAsync();
        }
        public async Task<User> Save(IUserSaveRequest request)
        {
            if (string.IsNullOrEmpty(request.Guid) && !string.IsNullOrEmpty(request.UserName))
            {
                if (db.Users.Any(x => !x.Deleted && x.Username.ToLower() == request.UserName.ToLower().Trim()))
                    throw new Exception("DUPLICATE");
            }

            var user = new User() { Guid = Guid.NewGuid(), Active = true };
            if (!string.IsNullOrEmpty(request.Guid))
                user = db.Users.FirstOrDefault(x => x.Guid == Guid.Parse(request.Guid));
            if (user == null) throw new Exception("NO_USER");
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Username = request.UserName;
            if (!string.IsNullOrEmpty(request.ParentUserId))
            {
                user.ParentUserId = db.Users.Where(y => y.Guid == Guid.Parse(request.ParentUserId)).Select(y => y.Id).FirstOrDefault();
            }
            else
            {
                user.ParentUserId = null;
            }

            if (string.IsNullOrEmpty(request.Guid))
            {
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }
            if (string.IsNullOrEmpty(request.Guid))
            {
                db.UserPasswords.Add(new UserPassword
                {
                    UserId = user.Id,
                    CreatedOnUtc = DateTime.UtcNow,
                    Password = request.Password,
                }); ;
                await db.SaveChangesAsync();
            }
            return new User { Guid = user.Guid, Id = user.Id };
        }
        public async Task ChangePassword(AuthModel.UpdatePasswordRequest request)
        {
            var user = db.Users.FirstOrDefault(x => x.Guid == Guid.Parse(request.Id))
                ?? throw new Exception("NOT_FOUND");
            db.UserPasswords.Add(new UserPassword
            {
                CreatedOnUtc = DateTime.UtcNow,
                Password = request.Password,
                UserId = user.Id
            });
            db.UserAuthTokens.RemoveRange(db.UserAuthTokens.Where(x => x.UserId == user.Id));
            await db.SaveChangesAsync();
        }
        public async Task<int> Delete(Guid guid)
        {
            var c = db.Users.FirstOrDefault(x => x.Guid == guid) ?? throw new Exception("NOT_FOUND");
            c.Deleted = true;
            await db.SaveChangesAsync();
            return c.Id;
        }

        #region User Roles
        public async Task<UserRole> SaveRole(SaveRoleRequest request)
        {
            var role = db.UserRoles.Where(x => x.Id == request.Id).FirstOrDefault() ?? new
                UserRole()
            { Active = true };
            role.IsSystemRole = request.IsSystemRole;
            role.SystemName = request.SystemName;
            role.Name = request.Name;
            role.EnablePasswordLifetime = false;
            if (role.Id == 0)
            {
                db.UserRoles.Add(role);
            }
            await db.SaveChangesAsync();
            return new UserRole() { Id = role.Id };
        }
        public RoleDetails GetRoleDetails(int roleId)
        {
            return db.UserRoles
                   .Where(x => x.Id == roleId)
                   .Include(x => x.Roles)
                   .Include(u => u.PagesInRoles)
                   .Select(u => new RoleDetails
                   {
                       Id = u.Id,
                       Name = u.Name,
                       SystemName = u.SystemName,
                       Pages = u.PagesInRoles.Select
                       (x => new { x.PageId, x.Page.Name, x.Page.SystemName }).ToList(),
                       Roles = u.Roles.Select(x => new { x.Name, x.Id, u.SystemName })
                   }).FirstOrDefault() ?? throw new Exception("ROLE NOT FOUND");
        }
        public async Task<List<RoleDetails>> GetRoles(int userId)
        {
            // Retrieve the user with roles
            User user = db.Users
                .Include(u => u.Roles).ThenInclude(x => x.PagesInRoles).ThenInclude(x => x.Page)
                .Include(u => u.Roles).ThenInclude(x => x.Roles).ThenInclude(x => x.PagesInRoles).ThenInclude(x => x.Page)
                .FirstOrDefault(u => u.Id == userId) ?? throw new Exception("USER NOT FOUND");

            // Check if the user is a super admin
            bool isSuperAdmin = user != null && user.Roles.Any(r => r.SystemName.Equals("superadmin", StringComparison.OrdinalIgnoreCase));
            if (isSuperAdmin)
            {
                return await db.UserRoles
                    .Where(x => isSuperAdmin || !x.IsSystemRole)
                    //.Include(u => u.PagesInRoles)
                    .Select(u => new RoleDetails
                    {
                        Id = u.Id,
                        Name = u.Name,
                        SystemName = u.SystemName,
                        Pages = u.PagesInRoles.Select
                        (x => new { x.PageId, x.Page.Name, x.Page.SystemName }).ToList(),
                        //    Roles = u.ParentRoles.Select(x => new { x.Name, x.Id, u.SystemName })
                    }).ToListAsync();
            }
            var roles = new List<RoleDetails>();
            foreach (var role in user.Roles)
            {

                if (!roles.Any(x => x.Id == role.Id))
                {
                    roles.Add(new RoleDetails
                    {
                        Id = role.Id,
                        Name = role.Name,
                        SystemName = role.SystemName,
                        Active = role.Active,
                        Pages = role.PagesInRoles.Select
                        (x => new { x.PageId, x.Page?.Name, x.Page?.SystemName }).ToList(),
                    });
                }
                foreach (var item in role.Roles)
                {
                    if (!roles.Any(x => x.Id == item.Id))
                    {
                        roles.Add(new RoleDetails
                        {
                            Id = item.Id,
                            Name = item.Name,
                            SystemName = item.SystemName,
                            Active = item.Active,
                            Pages = item.PagesInRoles.Select
                        (x => new { x.PageId, x.Page.Name, x.Page.SystemName }).ToList(),

                        });
                    }
                }
            }
            return roles;
        }
        public void UpdateRole(UpdateRoleRequest request)
        {
            var User = db.Users.Where(u => u.Guid == Guid.Parse(request.UserId)).Include(u => u.Roles).FirstOrDefault() ?? throw new Exception("User_NOT_FOUND");
            var role = db.UserRoles.FirstOrDefault(r => r.Id == request.RoleId) ?? throw new Exception("ROLE_NOT_FOUND");
            if (request.Action == MethodType.delete)
            {
                User.Roles.Remove(role);
            }
            else
            {
                if (!User.Roles.Any(u => u.Id == role.Id))
                    User.Roles.Add(role);
                db.SaveChanges();
            }
        }
        public async Task<UserRole> UpdateRoleAssignment(UserModel.UpdateRoleAssigment request)
        {
            var Role = db.UserRoles.Where(u => u.Id == request.RoleId).Include(x => x.ParentRoles).FirstOrDefault() ?? throw new Exception("Role_NOT_FOUND");
            var ParentRole = db.UserRoles.Where(u => u.Id == request.ParentRoleId).FirstOrDefault() ?? throw new Exception("ParentRole_NOT_FOUND");
            if (request.Action == MethodType.delete)
            {
                if (Role.ParentRoles.Contains(ParentRole))
                {
                    Role.ParentRoles.Remove(ParentRole);
                }
                else
                {
                    throw new Exception("ParentRole_NOT_ASSIGNED_TO_ROLE");
                }
            }
            else if (request.Action == MethodType.insert)
            {
                if (!Role.ParentRoles.Contains(ParentRole))
                {
                    Role.ParentRoles.Add(ParentRole);
                }
                else
                {
                    throw new Exception("ParentRole_ALREADY_ASSIGNED_TO_ROLE");
                }
            }
            await db.SaveChangesAsync();
            return new UserRole { Id = Role.Id };
        }
        #endregion
    }
}
