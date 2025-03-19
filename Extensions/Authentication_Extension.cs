using MongoDB.Bson;
using user_management.Helpers;
using user_management.Models;
using user_management.Services;
using user_management.Validators;

namespace user_management.Extensions
{
    public static class AuthenticationExtension
    {
        public static WebApplication AddAuthenticationExtension(this WebApplication app)
        {
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

            app.MapPost("/logout", async (UserService userService, LoginValidator validator, User user, ILogger<Program> logger) =>
                {
                    try
                    {
                        var validationResult = await validator.ValidateAsync(user);
                        if (!validationResult.IsValid)
                        {
                            return Results.BadRequest(ApiResponse<object>.BadRequest(validationResult.ToString()));
                        }
                        userService.CurrentUser = user;
                        bool isLoggedOut = await userService.LogoutAsync();
                        logger.LogInformation("User Logout: {IsLoggedOut}", isLoggedOut);
                        return isLoggedOut
                            ? Results.Ok(ApiResponse<bool>.Success(true, "Logout successful."))
                            : Results.BadRequest(ApiResponse<string>.Error("Already logged out. Please login again to continue."));
                    }
                    catch (Exception ex)
                    {
                        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));
                    }
                }).WithName("LogoutUser")
                .WithOpenApi(operation => new(operation)
                {
                    Summary = "Logout a user",
                    Description = "Clears the session and logs out the user."
                });

            // Endpoint for user login
            app.MapPost("/login", async (UserService userService, AuthService authService,LoginValidator validator, User user, ILogger<Program> logger) =>
            {
                try
                {
                    userService.CurrentUser = user;
                    
                    var validationResult = await validator.ValidateAsync(user);
                    
                    if (!validationResult.IsValid)
                    {
                        return Results.BadRequest(ApiResponse<string>.BadRequest(validationResult.ToString()));
                    }
                    
                 
                    
                    bool isLoggedIn = await userService.LoginAsync();
                    
                    logger.LogInformation("User Log In: {IsLoggedIn}", isLoggedIn);
                    
                    return isLoggedIn
                        ? Results.Ok(ApiResponse<object>.Success(authService.GenerateToken(userService.CurrentUser), "Login successful."))
                        : Results.BadRequest(ApiResponse<object>.BadRequest("Invalid username or password."));
                }
                catch (Exception ex)
                {
                    return Results.InternalServerError(ApiResponse<object>.Error(ex, ex.Message));
                }
            })
            .WithName("LoginUser")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Login a user",
                Description = "Authenticates user and sets login session."
            });

            return app;
        }
    }
}