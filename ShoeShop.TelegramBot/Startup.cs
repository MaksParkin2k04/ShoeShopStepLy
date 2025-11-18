using ShoeShop.TelegramBot.Services;

namespace ShoeShop.TelegramBot;

public class Startup {
    public void ConfigureServices(IServiceCollection services) {
        services.AddSingleton<TelegramBotService>();
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
        if (env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
        }
        
        // Поддержка статических файлов для Mini App
        app.UseStaticFiles();
        
        // Маршрут для Mini App
        app.UseRouting();
        app.UseEndpoints(endpoints => {
            endpoints.MapGet("/miniapp", async context => {
                var html = await File.ReadAllTextAsync("wwwroot/miniapp/index.html");
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(html);
            });
        });
        
        // Запуск Telegram бота
        var botService = app.ApplicationServices.GetRequiredService<TelegramBotService>();
        _ = Task.Run(() => botService.StartAsync());
    }
}