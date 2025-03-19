using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;


using user_management.Databases;
using user_management.Helpers;
using user_management.Models;
using user_management.Services;
using user_management.Validators;



namespace user_management.Extensions
{
    public static class AddDependencyInjection
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy => policy.AllowAnyOrigin()
                                    .AllowAnyMethod()
                                    .AllowAnyHeader());
            });

            services.AddTransient<AuthService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Only_Myanmar", Version = "v1" });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "JWT Authentication",
                    Description = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InRlc3R1c2VyQGV4YW1wbGUuY29tIiwicm9sZSI6InN0cmluZyIsIm5iZiI6MTc0MTU3NjMwMCwiZXhwIjoxNzQxNTc5OTAwLCJpYXQiOjE3NDE1NzYzMDB9.Dmc5c7uoJcfTBi6gwS46DYGo_VA9OY5HgKxwoS5ro8g",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                };

                c.AddSecurityDefinition("Bearer", securityScheme);

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                };

                c.AddSecurityRequirement(securityRequirement);
            });

            services.AddEndpointsApiExplorer();

            services
                .AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(AuthSettings.PrivateKey)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            services.AddAuthorization();

            // Register AutoMapper
            services.AddAutoMapper(cfg => cfg.AddProfile<UserProfile>(), typeof(UserProfile).Assembly);


            // Register the AppDbContext as a Singleton
            services.AddDbContext<AppDbContext>();

            // Register Services as a Scoped service
            services.AddScoped<UserService>();
            services.AddScoped<UserDirectory>();
            services.AddScoped<RoleService>();
            services.AddScoped<AccountStatusService>();
            services.AddScoped<ContentCreatorService>();
            services.AddScoped<ProductionCompanyService>();
            services.AddScoped<ViewerService>();
            services.AddScoped<PlatformAdminService>();

            // Register Validator
            services.AddScoped<RegisterValidator>();
            services.AddScoped<LoginValidator>();
            return services;
        }
    }
}
