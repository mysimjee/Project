

using user_management.Helpers;
using user_management.Models;
using user_management.Services;




namespace user_management.Extensions;

public static class RoleExtension
{
    public static WebApplication AddRoleExtension(this WebApplication app)
    {
        //role
app.MapPost("/roles", async (RoleService roleService, string roleName, string description, List<RolePermission> rolePermissions) =>
{
    try
    {
        var role = await roleService.CreateRoleAsync(roleName, description, rolePermissions);
        return Results.Created(string.Empty, ApiResponse<Role>.Created(role, "Role created successfully."));
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));
    }
})
.WithName("CreateRole")
.WithOpenApi(operation => new(operation)
{
    Summary = "Create a new role",
    Description = "Creates a new role along with its permissions."
});

app.MapPut("/roles/{roleId:int}", async (int roleId, RoleService roleService, string roleName, string description, List<RolePermission> rolePermissions) =>
{
    try
    {
        var updatedRole = await roleService.UpdateRoleAsync(roleId, roleName, description, rolePermissions);
        return updatedRole != null
            ? Results.Ok(ApiResponse<Role>.Success(updatedRole, "Role updated successfully."))
            : Results.NotFound(ApiResponse<string>.NotFound("Role not found."));
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));
    }
})
.WithName("UpdateRole")
.WithOpenApi(operation => new(operation)
{
    Summary = "Update an existing role",
    Description = "Updates the role name, description, and permissions."
});

app.MapDelete("/roles/{roleId:int}", async (int roleId, RoleService roleService) =>
{
    try
    {
        bool? isDeleted = await roleService.DeleteRoleAsync(roleId);
        return isDeleted != null
            ? Results.Ok(ApiResponse<bool>.Success(true, "Role deleted successfully."))
            : Results.NotFound(ApiResponse<string>.NotFound("Role not found."));
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));
    }
})
.WithName("DeleteRole")
.WithOpenApi(operation => new(operation)
{
    Summary = "Delete a role",
    Description = "Deletes a role by its ID."
});

app.MapPut("/permissions/{permissionId:int}", async (int permissionId, RoleService roleService, string permissionName, string description) =>
{
    try
    {
        var updatedPermission = await roleService.UpdatePermissionAsync(permissionId, permissionName, description);
        return updatedPermission != null
            ? Results.Ok(ApiResponse<RolePermission>.Success(updatedPermission, "Permission updated successfully."))
            : Results.NotFound(ApiResponse<string>.NotFound("Permission not found."));
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));    }
})
.WithName("UpdatePermission")
.WithOpenApi(operation => new(operation)
{
    Summary = "Update a role permission",
    Description = "Updates a specific role permission by ID."
});



        return app;
    }
}