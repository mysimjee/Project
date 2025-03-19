
using Microsoft.EntityFrameworkCore;
using user_management.Databases;
using user_management.Models;


namespace user_management.Services
{
    public class RoleService(AppDbContext context)
    {
        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await context.Roles
                .Include(r => r.RolePermissions)
                .ToListAsync();
        }

        // Create a new role with permissions
        public async Task<Role> CreateRoleAsync(string roleName, string description, List<RolePermission> rolePermissions)
        {
            var role = new Role
            {
                RoleName = roleName,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RolePermissions = rolePermissions
            };

            context.Roles.Add(role);
            await context.SaveChangesAsync();

            return role;
        }

        // Update role and its permissions
        public async Task<Role?> UpdateRoleAsync(int roleId, string roleName, string description, List<RolePermission> rolePermissions)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
            if (role != null)
            {
                role.RoleName = roleName;
                role.Description = description;
                role.UpdatedAt = DateTime.UtcNow;
                role.RolePermissions = rolePermissions;
            }
            await context.SaveChangesAsync();
            return role;
        }

        // Delete a role
        public async Task<bool?> DeleteRoleAsync(int roleId)
        {
            var role = await context.Roles.FindAsync(roleId);
            if (role != null)
            {
                context.Roles.Remove(role);
                await context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        // Update a role permission
        public async Task<RolePermission?> UpdatePermissionAsync(int permissionId, string permissionName, string description)
        {
            var permission = await context.RolePermissions.FindAsync(permissionId);
            if (permission != null)
            {
                permission.PermissionName = permissionName;
                permission.Description = description;
                permission.UpdatedAt = DateTime.UtcNow;
            }
            await context.SaveChangesAsync();
            return permission;
        }
    }
}
