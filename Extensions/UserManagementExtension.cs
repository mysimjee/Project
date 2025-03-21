
using user_management.Exceptions;
using user_management.Helpers;
using user_management.Models;
using user_management.Services;
using user_management.Validators;


namespace user_management.Extensions
{
    public static class UserManagementExtension
    {
        public static WebApplication AddUserManagementExtension(this WebApplication app)
        {
            
            app.MapGet("/loginhistory/{userId}/{skip}/{limit}", async (int userId, UserDirectory service, int skip = 0, int limit = 10) =>
                {
                    try
                    {
                        var history = await service.GetLoginHistoryAsync(userId, skip, limit);
                        return history.Any()
                            ? Results.Ok(ApiResponse<List<LoginHistory>>.Success(history, "Login history retrieved successfully."))
                            : Results.NotFound(ApiResponse<string>.NotFound("No login history found for this user."));
                    }
                    catch (Exception ex)
                    {
                        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));
                    }
                })
                .WithName("GetLoginHistory")
                .WithOpenApi(operation => new(operation)
                {
                    Summary = "Get Login History",
                    Description = "Retrieves paginated login history of a user, sorted by most recent login. Supports 'skip' and 'limit'."
                });




// Change Role
app.MapPut("/users/{id:int}/role/{roleId:int}", async (int id, int roleId, UserDirectory userDirectory) =>
{
    try
    {
        var result = await userDirectory.ChangeRoleAsync(id, roleId);
        return result != null
            ? Results.Ok(ApiResponse<Role?>.Success(result, "Account status updated successfully."))
            : Results.NotFound(ApiResponse<string>.NotFound("User or status not found."));
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));
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
        userDirectory.SetCursor(cursor);
        List<User> users = await userDirectory.GetUsersAsync(limit);
        return Results.Ok(ApiResponse<List<User>>.Success(users, "Users retrieved successfully."));
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));
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
        return Results.Ok(new ApiResponse<object>(200, "Users retrieved successfully.", response));
    }
    catch (FailToMeetCriteriaException ex)
    {
        return Results.BadRequest( ApiResponse<string>.BadRequest(ex.Message));
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));
    }
})
.WithName("GetUsersByProperty") 
.WithOpenApi();  



app.MapGet("/users/count", (UserDirectory userDirectory) =>
{
    try
    {
        int totalCount = userDirectory.TotalUsers;
        return Results.Ok(ApiResponse<int>.Success(totalCount, "Total user count retrieved."));
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));
    }
})
.WithName("GetTotalUserCount")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get total user count",
    Description = "Retrieves the total number of registered users."
});


            app.MapDelete("/users/{id:int}", async (int id, UserDirectory userDirectory) =>
                {
                    try
                    {
                        bool result = await userDirectory.RemoveUserAsync(id);
                        return result
                            ? Results.Ok(ApiResponse<object>.Success(true, "User deleted successfully."))
                            : Results.NotFound(ApiResponse<object>.NotFound("User not found."));
                    }
                    catch (Exception ex)
                    {
                        return Results.BadRequest(ApiResponse<object>.Error(ex,ex.Message));
                    }
                })
                .WithName("RemoveUser")
                .WithOpenApi(operation => new(operation)
                {
                    Summary = "Delete a user",
                    Description = "Removes a user by ID."
                });
            
            // Endpoint to fetch all roles
            app.MapGet("/roles", async (RoleService roleService, ILogger<Program> logger) =>
            {
                try
                {
                    // Get all roles from the RoleService
                    var roles = await roleService.GetAllRolesAsync();
                    logger.LogInformation("Fetched Roles: {Roles}", roles.ToString());

                    // Return a success response with the roles
                    return Results.Ok(ApiResponse<List<Role>>.Success(roles, "Fetched roles successfully."));
                }
                catch (Exception ex)
                {
                    // Handle any exceptions and return an error response
                    return Results.InternalServerError(ApiResponse<object>.Error(ex, "Failed to fetch roles."));
                }
            });

            // Endpoint to add a user
            app.MapPost("/users", async (User user, UserDirectory userDirectory, RegisterValidator validator, ILogger<Program> logger) =>
            {
                try
                {
                    var validationResult = await validator.ValidateAsync(user);
                    if (!validationResult.IsValid)
                    {
                        return Results.BadRequest(ApiResponse<object>.BadRequest(validationResult.ToString()));
                    }

                    bool result = await userDirectory.AddUserAsync(user);
                    user.UserId = await userDirectory.GetUserIdByUsernameAsync(user.Username);
                    logger.LogInformation("User Added: {Result}", result);
                    return result
                        ? Results.Created(string.Empty, ApiResponse<User>.Created(user, "User added successfully"))
                        : Results.BadRequest(ApiResponse<string>.BadRequest("Failed to add user"));
                }
                catch (Exception ex)
                {
                    return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));
                }
            })
            .WithName("AddUser")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Add a new user",
                Description = "Creates a new user if email and username are unique."
            });
            




            
            return app;
        }
        
    }
    
}

