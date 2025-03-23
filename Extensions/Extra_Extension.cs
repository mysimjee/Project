using Serilog;
using user_management.Hubs;

namespace user_management.Extensions
{
    public static class ExtraExtension
    {
        public static WebApplication AddExtraExtension(this WebApplication app)
        {
   
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
            
            return app;
        }

        
    }
}

