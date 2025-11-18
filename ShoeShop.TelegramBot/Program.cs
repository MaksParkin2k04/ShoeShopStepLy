using ShoeShop.TelegramBot;

var builder = WebApplication.CreateBuilder(args);

// Настройка сервисов
var startup = new Startup();
startup.ConfigureServices(builder.Services);

var app = builder.Build();

// Настройка pipeline
startup.Configure(app, app.Environment);

Console.WriteLine("🤖 Telegram Bot + Mini App запущен на https://localhost:7003");

app.Run("https://localhost:7003");