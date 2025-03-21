using user_management.Helpers;
using user_management.Models;
using user_management.Services;
namespace user_management.Extensions;

public static class AccountStatusExtension
{
    public static WebApplication AddAccountStatusExtension(this WebApplication app)
    {
         // Account Status
         
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
        
        return app;
    }
}