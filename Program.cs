
using user_management.Models;
using user_management.Services;
using user_management.Helpers;
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

// Configure the HTTP request pipeline.
app.AddAuthenticationExtension();
app.AddUserManagementExtension();
app.AddExtraExtension();
app.AddRoleExtension();
app.AddAccountStatusExtension();
app.AddUserInfoExtension();

app.MapGet("/signin", () => "User Authenticated Successfully!").RequireAuthorization();




app.Run();
