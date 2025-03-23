
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

            app.UseAuthentication();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapPost("/logout", async (UserService userService, LoginRequest user, ILogger<Program> logger) =>
                {
                    try
                    {
                        var userServiceCurrentUser = new User()
                        {
                            Username = user.Username,
                            Email = user.Email,
                            PasswordHash = user.PasswordHash,
                        };
                        userService.CurrentUser = userServiceCurrentUser;
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

            app.MapPut("/users/update", async (UserService userService, AuthService authService, User updatedUser, ILogger<Program> logger) =>
                {
                    try
                    {
                        userService.CurrentUser = updatedUser;
                        var updatedUserInfo = await userService.UpdateAccountAsync(updatedUser);
        
                        logger.LogInformation("User Updated: {User}", updatedUserInfo);
                        
                        return Results.Ok(ApiResponse<User>.Success(updatedUserInfo!, authService.GenerateToken(updatedUserInfo!) ));
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Error updating user: {Error}", ex.Message);
                        return Results.InternalServerError(ApiResponse<string>.Error(ex, "Failed to update user."));
                    }
                })
                .WithName("UpdateUser")
                .WithOpenApi(operation => new(operation)
                {
                    Summary = "Update an existing user",
                    Description = "Updates the user information (username, email, password)."
                });

            // Endpoint for user login
            app.MapPost("/login", async (UserService userService, AuthService authService,LoginValidator validator, LoginRequest user, ILogger<Program> logger) =>
            {
                try
                {
                    var userServiceCurrentUser = new User()
                    {
                        Username = user.Username,
                        Email = user.Email,
                        PasswordHash = user.PasswordHash,
                    };
                    
                    userService.CurrentUser = userServiceCurrentUser;
                    
                    var validationResult = await validator.ValidateAsync(userServiceCurrentUser);
                    
                    if (!validationResult.IsValid)
                    {
                        return Results.BadRequest(ApiResponse<string>.BadRequest(validationResult.ToString()));
                    }
                    
                    bool isLoggedIn = await userService.LoginAsync();
                    
                    logger.LogInformation("User Log In: {IsLoggedIn}", isLoggedIn);
                    
                    return isLoggedIn
                        ? Results.Ok(ApiResponse<User>.Success(userService.CurrentUser, authService.GenerateToken(userService.CurrentUser)))
                        : Results.BadRequest(ApiResponse<string>.BadRequest("Invalid username or password."));
                }
                catch (Exception ex)
                {
                    return Results.InternalServerError(ApiResponse<string>.Error(ex, ex.Message));
                }
            })
            .WithName("LoginUser")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Login a user",
                Description = "Authenticates user and sets login session."
            });
            

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


app.MapPost("/resetpassword", async (ResetPasswordRequest request, AuthService authService, UserDirectory userDirectory,LoginValidator validator,EmailService emailService, UserService userService) =>
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
            var newUser = await userDirectory.GetUserInfoByEmailAsync(request.Email);
            return resetSuccess
                ? Results.Ok(ApiResponse<User>.Success(newUser!, authService.GenerateToken(newUser!)))
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

            return app;
        }
    }
}