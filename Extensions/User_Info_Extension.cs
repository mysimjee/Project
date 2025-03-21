
using user_management.Helpers;
using user_management.Models;
using user_management.Services;


namespace user_management.Extensions;

public static class UserInfoExtension
{

    public static WebApplication AddUserInfoExtension(this WebApplication app)
    {
        
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

app.MapPut("/production-companies/{userId}", async (int userId, ProductionCompany updatedCompany, ProductionCompanyService service) =>
{
    try
    {
        var result = await service.UpdateProductionCompanyAsync(userId, updatedCompany);
        return Results.Json(result
            ? ApiResponse<string>.Success("Production Company updated successfully.")
            : ApiResponse<string>.NotFound("Production Company not found."));
    } catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex));
    }
});

app.MapPut("/viewers/{userId}", async (int userId, Viewer updatedViewer, ViewerService service) =>
{
    try
    {
        var result = await service.UpdateViewerAsync(userId, updatedViewer);
        return Results.Json(result
            ? ApiResponse<string>.Success("Viewer updated successfully.")
            : ApiResponse<string>.NotFound("Viewer not found."));
    } catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex));
    }
});

app.MapPut("/platform-admins/{userId}", async (int userId, PlatformAdmin updatedAdmin, PlatformAdminService service) =>
{
    try
    {
        var result = await service.UpdatePlatformAdminAsync(userId, updatedAdmin);
        return Results.Json(result
            ? ApiResponse<string>.Success("Platform Admin updated successfully.")
            : ApiResponse<string>.NotFound("Platform Admin not found."));
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex));
    }
});

app.MapPut("/content-creators/{userId}", async (int userId, ContentCreator updatedCreator, ContentCreatorService service) =>
{
    try
    {
        var result = await service.UpdateContentCreatorAsync(userId, updatedCreator);
        return Results.Json(result
            ? ApiResponse<string>.Success("Content Creator updated successfully.")
            : ApiResponse<string>.NotFound("Content Creator not found."));
    } catch (Exception ex)
    {
        return Results.InternalServerError(ApiResponse<string>.Error(ex));
    }
});

        return app;
    }
    
}