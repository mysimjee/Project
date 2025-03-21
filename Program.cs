
using user_management.Models;
using user_management.Services;
using user_management.Helpers;
using user_management.Exceptions;
using user_management.Extensions;

using Serilog;
using user_management.Hubs;
using user_management.Validators;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDependencies(); 


//Add support to logging with SERILOG
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));


builder.Logging.ClearProviders();
builder.Logging.AddSerilog(); 



var app = builder.Build();

app.UseCors("AllowLocalhost");
app.MapHub<NotificationHub>("/hubs/NotificationHub");


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

// Change Account Status
app.MapPut("/users/{id:int}/status/{statusId:int}", async (int id, int statusId, UserDirectory userDirectory) =>
{
    try
    {
        var result = await userDirectory.ChangeAccountStatusAsync(id, statusId);
        return result != null
            ? Results.Ok(ApiResponse<AccountStatus?>.Success(result, "Account status updated successfully."))
            : Results.NotFound(ApiResponse<object>.NotFound("User or status not found."));
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));
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



// Account Status
app.MapGet("/account-status/{accountStatusId:int}", async (int accountStatusId, AccountStatusService accountStatusService) =>
{
    try
    {
        var status = await accountStatusService.GetStatusInfoAsync(accountStatusId);
        return status != null
            ? Results.Ok(ApiResponse<AccountStatus>.Success(status, "Account status retrieved successfully."))
            : Results.NotFound(ApiResponse<string>.NotFound("Account status not found."));
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));    }
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
        return Results.Ok(ApiResponse<AccountStatus>.Created(status, "Account status created successfully."));
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));    }
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
            ? Results.Ok(ApiResponse<AccountStatus>.Success(updatedStatus, "Account status updated successfully."))
            : Results.NotFound(ApiResponse<string>.NotFound("Account status not found."));
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));    }
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
            ? Results.Ok(ApiResponse<object>.Success(true, "Account status deleted successfully."))
            : Results.NotFound(ApiResponse<string>.NotFound("Account status not found."));
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));    }
})
.WithName("DeleteAccountStatus")
.WithOpenApi(operation => new(operation)
{
    Summary = "Delete an account status",
    Description = "Deletes an account status using the given ID."
});


//User




app.MapGet("/login-status/{userId:int}", async (UserService userService, int userId) =>
{
    try
    {
        userService.CurrentUser!.UserId = userId;
        var loginStatus = await userService.GetLoginStatusAsync();
        return loginStatus != null
            ? Results.Ok(ApiResponse<LoginHistory>.Success(loginStatus, "Login status retrieved successfully."))
            : Results.NotFound(ApiResponse<string>.NotFound("No login history found."));
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));    }
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
            ? Results.Ok(ApiResponse<object>.Success(true, "Account deactivated successfully."))
            : Results.NotFound(ApiResponse<string>.NotFound("Account not found or already inactive."));
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));    }
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
    try
    {
        var viewer = await service.GetViewerByIdAsync(userId);
        return viewer != null
            ? Results.Ok(ApiResponse<Viewer>.Success(viewer, "Viewer retrieved successfully."))
            : Results.NotFound(ApiResponse<string>.NotFound("Viewer not found."));
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));    
    }
})
.WithName("GetViewerById")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get Viewer by ID",
    Description = "Retrieves a viewer's details by their user ID."
});


app.MapGet("/content-creators/{userId}", async (int userId, ContentCreatorService service) =>
{
    try
    {
        var contentCreator = await service.GetContentCreatorByIdAsync(userId);
        return contentCreator != null
            ? Results.Ok(ApiResponse<ContentCreator>.Success(contentCreator, "Content Creator retrieved successfully."))
            : Results.NotFound(ApiResponse<string>.NotFound("Content Creator not found."));
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));    
    }
})
.WithName("GetContentCreatorById")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get Content Creator by ID",
    Description = "Retrieves a content creator's details by their user ID."
});

app.MapGet("/production-companies/{userId}", async (int userId, ProductionCompanyService service) =>
{
    try
    {
        var productionCompany = await service.GetProductionCompanyByIdAsync(userId);
        return productionCompany != null
            ? Results.Ok(ApiResponse<ProductionCompany>.Success(productionCompany, "Production Company retrieved successfully."))
            : Results.NotFound(ApiResponse<string>.NotFound("Production Company not found."));
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));    

    }
})
.WithName("GetProductionCompanyById")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get Production Company by ID",
    Description = "Retrieves a production company's details by their user ID."
});

app.MapGet("/platform-admins/{userId}", async (int userId, PlatformAdminService service) =>
{
    try
    {
        var platformAdmin = await service.GetPlatformAdminByIdAsync(userId);
        return platformAdmin != null
            ? Results.Ok(ApiResponse<PlatformAdmin>.Success(platformAdmin, "Platform Admin retrieved successfully."))
            : Results.NotFound(ApiResponse<string>.NotFound("Platform Admin not found."));
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));    
    }
})
.WithName("GetPlatformAdminById")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get Platform Admin by ID",
    Description = "Retrieves a platform admin's details by their user ID."
});


app.MapGet("/sendemailcode/{email}", async (string email, EmailService service) =>
    {
        try
        {
            var platformAdmin = await service.SendEmailVerificationCodeAsync(email);
            return platformAdmin
                ? Results.Ok(ApiResponse<bool>.Success(true, "Verification code sent successfully."))
                : Results.NotFound(ApiResponse<string>.NotFound("Email not found."));
        }
        catch (Exception ex)
        {
            return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));    
        }
    })
    .WithName("SendEmailVerificationCode")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Send Email Verification Code",
        Description = "Sends an email verification code."
    });

app.MapGet("/validateemailcode/{email}/{code}", async (string email, string code, EmailService service) =>
    {
        try
        {
            var isValid = await service.ValidateVerificationCodeAsync(email, code);
            return isValid
                ? Results.Ok(ApiResponse<bool>.Success(true, "Verification code is valid."))
                : Results.BadRequest(ApiResponse<string>.Error("Invalid or expired verification code."));
        }
        catch (Exception ex)
        {
            return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));    
        }
    })
    .WithName("ValidateEmailVerificationCode")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Validate Email Verification Code",
        Description = "Checks if the provided email verification code is valid and not expired."
    });


app.MapPost("/resetpassword", async (ResetPasswordRequest request, LoginValidator validator,EmailService emailService, UserService userService) =>
    {
        try
        {
            userService.CurrentUser = new User
            {
                Email = request.Email,
                PasswordHash = request.NewPassword
            };

            var validationResult = await validator.ValidateAsync(userService.CurrentUser);
                    
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(ApiResponse<string>.BadRequest(validationResult.ToString()));
            }

            
            var isValid = await emailService.ValidateVerificationCodeAsync(request.Email, request.Code);
            if (!isValid)
            {
                return Results.BadRequest(ApiResponse<string>.Error("Invalid or expired verification code."));
            }
            
            var resetSuccess = await userService.ResetPasswordAsync(request.Email, request.NewPassword);
            return resetSuccess
                ? Results.Ok(ApiResponse<bool>.Success(true, "Password reset successfully."))
                : Results.NotFound(ApiResponse<string>.NotFound("User not found."));
        }
        catch (Exception ex)
        {
            return Results.InternalServerError(ApiResponse<string>.Error(ex));
        }
    })
    .WithName("ResetPassword")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Reset Password",
        Description = "Allows users to reset their password using a valid email verification code."
    });

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


app.MapPost("/verifyemail", async (VerifyEmailRequest request, EmailService service) =>
    {
        try
        {
            var isVerified = await service.VerifyEmailAsync(request.Email, request.Code);
            return isVerified
                ? Results.Ok(ApiResponse<bool>.Success(true, "Email verified successfully."))
                : Results.BadRequest(ApiResponse<string>.BadRequest("Invalid or expired verification code."));
        }
        catch (Exception ex)
        {
            return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));
        }
    })
    .WithName("VerifyEmail")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Verify Email",
        Description = "Verifies a user's email using a verification code and updates their account status."
    });


app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowAll");

app.Run();
