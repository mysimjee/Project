
using user_management.Models;
using user_management.Services;
using user_management.Helpers;
using user_management.Exceptions;
using user_management.Extensions;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies(); 


//Add support to logging with SERILOG
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));


builder.Logging.ClearProviders();
builder.Logging.AddSerilog(); 



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseExceptionHandler("/Error");

}


app.UseStaticFiles();
app.UseSerilogRequestLogging();



app.MapGet("/", () => "Hello World!");

app.MapPost("/authenticate", (User user, AuthService authService)
    => authService.GenerateToken(user));

app.MapGet("/signin", () => "User Authenticated Successfully!").RequireAuthorization();


// Define Minimal API Endpoints
app.AddAuthenticationExtension();

// Remove User
app.MapDelete("/users/{id:int}", async (int id, UserDirectory userDirectory) =>
{
    try
    {
        bool result = await userDirectory.RemoveUserAsync(id);
        return result
            ? Results.Ok(ApiResponse<object>.Success(null, "User deleted successfully."))
            : Results.NotFound(ApiResponse<object>.NotFound("User not found."));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ApiResponse<object>.Error(ex));
    }
})
.WithName("RemoveUser")
.WithOpenApi(operation => new(operation)
{
    Summary = "Delete a user",
    Description = "Removes a user by ID."
});

// Change Account Status
app.MapPut("/users/{id:int}/status/{statusId:int}", async (int id, int statusId, UserDirectory userDirectory) =>
{
    try
    {
        var result = await userDirectory.ChangeAccountStatusAsync(id, statusId);
        return result != null
            ? Results.Json(ApiResponse<object>.Success(result, "Account status updated successfully."))
            : Results.Json(ApiResponse<object>.NotFound("User or status not found."));
    }
    catch (Exception ex)
    {
        return Results.Json(ApiResponse<object>.Error(ex));
    }
})
.WithName("ChangeAccountStatus")
.WithOpenApi(operation => new(operation)
{
    Summary = "Change account status",
    Description = "Updates the account status of a user."
});

// Change Role
app.MapPut("/users/{id:int}/role/{roleId:int}", async (int id, int roleId, UserDirectory userDirectory) =>
{
    try
    {
        var result = await userDirectory.ChangeRoleAsync(id, roleId);
        return result != null
            ? Results.Json(ApiResponse<object>.Success(result, "Account status updated successfully."))
            : Results.Json(ApiResponse<object>.NotFound("User or status not found."));
    }
    catch (Exception ex)
    {
        return Results.Json(ApiResponse<object>.Error(ex));
    }
})
.WithName("ChangeRole")
.WithOpenApi(operation => new(operation)
{
    Summary = "Change user role",
    Description = "Updates the role of a user."
});

// Get Users (Paginated)
app.MapGet("/users/{limit:int}/{cursor:int}", async (UserDirectory userDirectory, int limit = 10, int cursor = 0) =>
{
    try
    {
        userDirectory.setCursor(cursor);
        var users = await userDirectory.GetUsersAsync(limit);
        return Results.Json(ApiResponse<List<User>>.Success(users, "Users retrieved successfully."));
    }
    catch (Exception ex)
    {
        return Results.Json(ApiResponse<object>.Error(ex));
    }
})
.WithName("GetUsers")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get list of users",
    Description = "Retrieves a paginated list of users."
});

app.MapGet("/users/by-property", async (UserDirectory userDirectory, string property, string value, int limit) =>
{
    try
    {
        object convertedValue;
        
        if (property.ToLower() == "roleid" || property.ToLower() == "userid" || property.ToLower() == "accountstatusid")
        {
            if (!int.TryParse(value, out int intValue))
                throw new FailToMeetCriteriaException("Invalid integer value provided.");
            
            convertedValue = intValue;
        }
        else
        {
            convertedValue = value;
        }

        var response = await userDirectory.GetUsersByPropertyAsync(property, convertedValue, limit);
        return Results.Json(new ApiResponse<object>(200, "Users retrieved successfully.", response));
    }
    catch (FailToMeetCriteriaException ex)
    {
        return Results.BadRequest(new ApiResponse<string>(400, ex.Message, null));
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.WithName("GetUsersByProperty")  // Set endpoint name
.WithOpenApi();  // Include in OpenAPI documentation



app.MapGet("/users/count", (UserDirectory userDirectory) =>
{
    try
    {
        int totalCount = userDirectory.totalUsers;
        return Results.Json(ApiResponse<object>.Success(new { TotalUsers = totalCount }, "Total user count retrieved."));
    }
    catch (Exception ex)
    {
        return Results.Json(ApiResponse<object>.Error(ex));
    }
})
.WithName("GetTotalUserCount")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get total user count",
    Description = "Retrieves the total number of registered users."
});


//role
app.MapPost("/roles", async (RoleService roleService, string roleName, string description, List<RolePermission> rolePermissions) =>
{
    try
    {
        var role = await roleService.CreateRoleAsync(roleName, description, rolePermissions);
        return Results.Json(ApiResponse<Role>.Created(role, "Role created successfully."));
    }
    catch (Exception ex)
    {
        return Results.Json(ApiResponse<object>.Error(ex, "Failed to create role."));
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
            ? Results.Json(ApiResponse<Role>.Success(updatedRole, "Role updated successfully."))
            : Results.Json(ApiResponse<object>.NotFound("Role not found."));
    }
    catch (Exception ex)
    {
        return Results.Json(ApiResponse<object>.Error(ex, "Failed to update role."));
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
        bool isDeleted = await roleService.DeleteRoleAsync(roleId);
        return isDeleted
            ? Results.Json(ApiResponse<object>.Success(null, "Role deleted successfully."))
            : Results.Json(ApiResponse<object>.NotFound("Role not found."));
    }
    catch (Exception ex)
    {
        return Results.Json(ApiResponse<object>.Error(ex, "Failed to delete role."));
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
            ? Results.Json(ApiResponse<RolePermission>.Success(updatedPermission, "Permission updated successfully."))
            : Results.Json(ApiResponse<object>.NotFound("Permission not found."));
    }
    catch (Exception ex)
    {
        return Results.Json(ApiResponse<object>.Error(ex, "Failed to update permission."));
    }
})
.WithName("UpdatePermission")
.WithOpenApi(operation => new(operation)
{
    Summary = "Update a role permission",
    Description = "Updates a specific role permission by ID."
});



// Account Status
app.MapGet("/account-status/{accountStatusId:int}", async (int accountStatusId, AccountStatusService accountStatusService) =>
{
    try
    {
        var status = await accountStatusService.GetStatusInfoAsync(accountStatusId);
        return status != null
            ? Results.Json(ApiResponse<AccountStatus>.Success(status, "Account status retrieved successfully."))
            : Results.Json(ApiResponse<object>.NotFound("Account status not found."));
    }
    catch (Exception ex)
    {
        return Results.Json(ApiResponse<object>.Error(ex, "Failed to retrieve account status."));
    }
})
.WithName("GetAccountStatus")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get account status by ID",
    Description = "Retrieves account status information using the given ID."
});

app.MapPost("/account-status", async (AccountStatusService accountStatusService, string statusName, string description) =>
{
    try
    {
        var status = await accountStatusService.CreateStatusAsync(statusName, description);
        return Results.Json(ApiResponse<AccountStatus>.Created(status, "Account status created successfully."));
    }
    catch (Exception ex)
    {
        return Results.Json(ApiResponse<object>.Error(ex, "Failed to create account status."));
    }
})
.WithName("CreateAccountStatus")
.WithOpenApi(operation => new(operation)
{
    Summary = "Create a new account status",
    Description = "Creates a new account status with a name and description."
});

app.MapPut("/account-status/{accountStatusId:int}", async (int accountStatusId, AccountStatusService accountStatusService, string statusName, string description) =>
{
    try
    {
        var updatedStatus = await accountStatusService.UpdateStatusAsync(accountStatusId, statusName, description);
        return updatedStatus != null
            ? Results.Json(ApiResponse<AccountStatus>.Success(updatedStatus, "Account status updated successfully."))
            : Results.Json(ApiResponse<object>.NotFound("Account status not found."));
    }
    catch (Exception ex)
    {
        return Results.Json(ApiResponse<object>.Error(ex, "Failed to update account status."));
    }
})
.WithName("UpdateAccountStatus")
.WithOpenApi(operation => new(operation)
{
    Summary = "Update an existing account status",
    Description = "Updates the name and description of an existing account status."
});

app.MapDelete("/account-status/{accountStatusId:int}", async (int accountStatusId, AccountStatusService accountStatusService) =>
{
    try
    {
        bool isDeleted = await accountStatusService.DeleteStatusAsync(accountStatusId);
        return isDeleted
            ? Results.Json(ApiResponse<object>.Success(null, "Account status deleted successfully."))
            : Results.Json(ApiResponse<object>.NotFound("Account status not found."));
    }
    catch (Exception ex)
    {
        return Results.Json(ApiResponse<object>.Error(ex, "Failed to delete account status."));
    }
})
.WithName("DeleteAccountStatus")
.WithOpenApi(operation => new(operation)
{
    Summary = "Delete an account status",
    Description = "Deletes an account status using the given ID."
});


//User




app.MapPost("/login-status", async (UserService userService, User user) =>
{
    try
    {
        userService.CurrentUser = user;
        var loginStatus = await userService.GetLoginStatusAsync();
        return loginStatus != null
            ? Results.Json(ApiResponse<LoginHistory>.Success(loginStatus, "Login status retrieved successfully."))
            : Results.Json(ApiResponse<object>.NotFound("No login history found."));
    }
    catch (Exception ex)
    {
        return Results.Json(ApiResponse<object>.Error(ex, "Failed to retrieve login status."));
    }
})
.WithName("GetLoginStatus")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get user login status",
    Description = "Fetches the latest login history entry."
});

app.MapPut("/deactivate-account", async (UserService userService, User user) =>
{
    try
    {
        userService.CurrentUser = user;
        bool isDeactivated = await userService.DeactivateAccountAsync();
        return isDeactivated
            ? Results.Json(ApiResponse<object>.Success(null, "Account deactivated successfully."))
            : Results.Json(ApiResponse<object>.NotFound("Account not found or already inactive."));
    }
    catch (Exception ex)
    {
        return Results.Json(ApiResponse<object>.Error(ex, "Failed to deactivate account."));
    }
})
.WithName("DeactivateAccount")
.WithOpenApi(operation => new(operation)
{
    Summary = "Deactivate a user account",
    Description = "Changes the account status to 'Inactive'."
});

//Add Detail Based on Role

app.MapPut("/content-creators/{userId}", async (int userId, ContentCreator updatedCreator, ContentCreatorService service) =>
{
    var result = await service.UpdateContentCreatorAsync(userId, updatedCreator);
    return Results.Json(result
        ? ApiResponse<string>.Success("Content Creator updated successfully.")
        : ApiResponse<string>.NotFound("Content Creator not found."));
});

app.MapPut("/production-companies/{userId}", async (int userId, ProductionCompany updatedCompany, ProductionCompanyService service) =>
{
    var result = await service.UpdateProductionCompanyAsync(userId, updatedCompany);
    return Results.Json(result
        ? ApiResponse<string>.Success("Production Company updated successfully.")
        : ApiResponse<string>.NotFound("Production Company not found."));
});

app.MapPut("/viewers/{userId}", async (int userId, Viewer updatedViewer, ViewerService service) =>
{
    var result = await service.UpdateViewerAsync(userId, updatedViewer);
    return Results.Json(result
        ? ApiResponse<string>.Success("Viewer updated successfully.")
        : ApiResponse<string>.NotFound("Viewer not found."));
});

app.MapPut("/platform-admins/{userId}", async (int userId, PlatformAdmin updatedAdmin, PlatformAdminService service) =>
{
    var result = await service.UpdatePlatformAdminAsync(userId, updatedAdmin);
    return Results.Json(result
        ? ApiResponse<string>.Success("Platform Admin updated successfully.")
        : ApiResponse<string>.NotFound("Platform Admin not found."));
});

app.MapGet("/viewers/{userId}", async (int userId, ViewerService service) =>
{
    var viewer = await service.GetViewerByIdAsync(userId);
    return viewer != null
        ? Results.Json(ApiResponse<Viewer>.Success(viewer, "Viewer retrieved successfully."))
        : Results.Json(ApiResponse<string>.NotFound("Viewer not found."));
})
.WithName("GetViewerById")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get Viewer by ID",
    Description = "Retrieves a viewer's details by their user ID."
});


app.MapGet("/content-creators/{userId}", async (int userId, ContentCreatorService service) =>
{
    var contentCreator = await service.GetContentCreatorByIdAsync(userId);
    return contentCreator != null
        ? Results.Json(ApiResponse<ContentCreator>.Success(contentCreator, "Content Creator retrieved successfully."))
        : Results.Json(ApiResponse<string>.NotFound("Content Creator not found."));
})
.WithName("GetContentCreatorById")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get Content Creator by ID",
    Description = "Retrieves a content creator's details by their user ID."
});

app.MapGet("/production-companies/{userId}", async (int userId, ProductionCompanyService service) =>
{
    var productionCompany = await service.GetProductionCompanyByIdAsync(userId);
    return productionCompany != null
        ? Results.Json(ApiResponse<ProductionCompany>.Success(productionCompany, "Production Company retrieved successfully."))
        : Results.Json(ApiResponse<string>.NotFound("Production Company not found."));
})
.WithName("GetProductionCompanyById")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get Production Company by ID",
    Description = "Retrieves a production company's details by their user ID."
});

app.MapGet("/platform-admins/{userId}", async (int userId, PlatformAdminService service) =>
{
    var platformAdmin = await service.GetPlatformAdminByIdAsync(userId);
    return platformAdmin != null
        ? Results.Json(ApiResponse<PlatformAdmin>.Success(platformAdmin, "Platform Admin retrieved successfully."))
        : Results.Json(ApiResponse<string>.NotFound("Platform Admin not found."));
})
.WithName("GetPlatformAdminById")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get Platform Admin by ID",
    Description = "Retrieves a platform admin's details by their user ID."
});


app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowAll");

app.Run();
