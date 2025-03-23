using user_management.Extensions;

using Serilog;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDependencies(); 


//Add support to logging with SERILOG
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(); 


//Enable Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});



var app = builder.Build();
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
app.AddAuthenticationExtension();
app.AddUserManagementExtension();
app.AddExtraExtension();
app.AddRoleExtension();
app.AddAccountStatusExtension();
app.AddUserInfoExtension();

app.MapGet("/signin", () => "User Authenticated Successfully!").RequireAuthorization();




app.Run();
