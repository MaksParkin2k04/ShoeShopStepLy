using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShoeShop.Data;
using ShoeShop.Data.Initialization;
using ShoeShop.Infrastructure;
using ShoeShop.Models;
using ShoeShop.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ShoeShop {
    public class Program {
        public static async Task Main(string[] args) {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSingleton(HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic }));

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Строка подключения 'DefaultConnection' не найдена.");
            builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false).AddRoles<ApplicationRole>().AddEntityFrameworkStores<ApplicationContext>();
            builder.Services.AddTransient<IEmailSender, EmailSender>();



            builder.Services.AddSession();
            builder.Services.AddRazorPages();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<IImageManager, ImageManager>();
            builder.Services.AddScoped<IProductManager, ProductManager>();
            builder.Services.AddScoped<IAdminRepository, AdminRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductStockRepository, ProductStockRepository>();
            builder.Services.AddScoped<StockService>();
            builder.Services.AddScoped<SalesStatisticsService>();
            builder.Services.AddScoped<IBasketShoppingService, BasketShoppingCookies>();
            builder.Services.AddScoped<PromoCodeService>();
            builder.Services.AddScoped<ReviewService>();
            builder.Services.AddHttpClient<YooKassaService>();
            builder.Services.AddHttpClient<YandexMetrikaService>();
            builder.Services.AddHttpClient<TelegramBotService>();
            builder.Services.AddScoped<TelegramBotHandler>();
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddHostedService<TelegramShopService>();

            builder.Services.Configure<IdentityOptions>(options => {
                // Параметры для требований к паролю..
                options.Password.RequireDigit = false;            // Возвращает или задает флаг, указывающий, должны ли пароли содержать цифру. По умолчанию используется значение 'true'.
                options.Password.RequireLowercase = false;        // Возвращает или задает флаг, указывающий, должны ли пароли содержать символ ASCII в нижнем регистре. По умолчанию используется значение 'true'.
                options.Password.RequireNonAlphanumeric = false;  // Возвращает или задает флаг, указывающий, должны ли пароли содержать не буквенно-цифровой символ. По умолчанию используется значение 'true'.
                options.Password.RequireUppercase = false;        // Возвращает или задает флаг, указывающий, должны ли пароли содержать символ ASCII в верхнем регистре. По умолчанию используется значение 'true'.
                options.Password.RequiredLength = 5;              // Возвращает или задает минимальную длину пароля. Значение по умолчанию — 6.
                options.Password.RequiredUniqueChars = 1;         // Возвращает или задает минимальное количество уникальных символов, которое должен содержать пароль. По умолчанию равен 1.

                // Параметры для блокировки пользователей..
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Возвращает или задает флаг, указывающий, можно ли заблокировать нового пользователя. По умолчанию имеет значение true.
                options.Lockout.MaxFailedAccessAttempts = 5;                      // Возвращает или задает число неудачных попыток доступа, разрешенных до блокировки пользователя при условии, что блокировка включена. Значение по умолчанию — 5.
                options.Lockout.AllowedForNewUsers = true;                        // Возвращает или задает флаг, указывающий, можно ли заблокировать нового пользователя. По умолчанию имеет значение true.

                // Параметры проверки пользователей.
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"; // Возвращает или задает список допустимых символов в имени пользователя, используемом для проверки имен пользователей. По умолчанию — abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+
                options.User.RequireUniqueEmail = true;                                                                         // Возвращает или задает флаг, указывающий, требуется ли приложению уникальные сообщения электронной почты для пользователей. Значение по умолчанию — false.
            });

            WebApplication app = builder.Build();

            // Инициализация базы данных
            using (IServiceScope scope = app.Services.CreateScope())
            {
                ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                StockService stockService = scope.ServiceProvider.GetRequiredService<StockService>();
                
                // Добавляем поля профиля пользователя
                try {
                    context.Database.ExecuteSqlRaw(
                        "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'FirstName') " +
                        "ALTER TABLE AspNetUsers ADD FirstName nvarchar(max) NULL");
                    
                    context.Database.ExecuteSqlRaw(
                        "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'LastName') " +
                        "ALTER TABLE AspNetUsers ADD LastName nvarchar(max) NULL");
                    
                    context.Database.ExecuteSqlRaw(
                        "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'Street') " +
                        "ALTER TABLE AspNetUsers ADD Street nvarchar(max) NULL");
                    
                    context.Database.ExecuteSqlRaw(
                        "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'House') " +
                        "ALTER TABLE AspNetUsers ADD House nvarchar(max) NULL");
                    
                    context.Database.ExecuteSqlRaw(
                        "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'Apartment') " +
                        "ALTER TABLE AspNetUsers ADD Apartment nvarchar(max) NULL");
                    
                    context.Database.ExecuteSqlRaw(
                        "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'City') " +
                        "ALTER TABLE AspNetUsers ADD City nvarchar(max) NULL");
                    
                    context.Database.ExecuteSqlRaw(
                        "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'PostalCode') " +
                        "ALTER TABLE AspNetUsers ADD PostalCode nvarchar(max) NULL");
                } catch {
                    // Колонки уже существуют
                }
                
                // Проверяем, существует ли таблица ProductStocks
                try {
                    var tableExists = context.Database.ExecuteSqlRaw(
                        "IF OBJECT_ID('ProductStocks', 'U') IS NULL " +
                        "CREATE TABLE ProductStocks (" +
                        "Id uniqueidentifier NOT NULL PRIMARY KEY, " +
                        "ProductId uniqueidentifier NOT NULL, " +
                        "Size int NOT NULL, " +
                        "Quantity int NOT NULL, " +
                        "PurchasePrice float NOT NULL DEFAULT 0, " +
                        "FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE, " +
                        "UNIQUE (ProductId, Size))");
                    
                    // Добавляем колонку PurchasePrice если ее нет
                    context.Database.ExecuteSqlRaw(
                        "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductStocks' AND COLUMN_NAME = 'PurchasePrice') " +
                        "ALTER TABLE ProductStocks ADD PurchasePrice float NOT NULL DEFAULT 0");
                } catch {
                    // Таблица уже существует
                }
                
                // Создаем таблицу PromoCodes
                try {
                    context.Database.ExecuteSqlRaw(
                        "IF OBJECT_ID('PromoCodes', 'U') IS NULL " +
                        "CREATE TABLE PromoCodes (" +
                        "Id int IDENTITY(1,1) NOT NULL PRIMARY KEY, " +
                        "Code nvarchar(20) NOT NULL UNIQUE, " +
                        "DiscountPercent decimal(5,2) NOT NULL, " +
                        "MaxDiscountAmount decimal(18,2) NULL, " +
                        "IsActive bit NOT NULL DEFAULT 1, " +
                        "CreatedAt datetime2 NOT NULL, " +
                        "ExpiresAt datetime2 NULL, " +
                        "UsageLimit int NULL, " +
                        "UsageCount int NOT NULL DEFAULT 0)");
                    
                    // Добавляем недостающие колонки если таблица уже существует
                    context.Database.ExecuteSqlRaw(
                        "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromoCodes' AND COLUMN_NAME = 'UsageLimit') " +
                        "ALTER TABLE PromoCodes ADD UsageLimit int NULL");
                    
                    context.Database.ExecuteSqlRaw(
                        "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromoCodes' AND COLUMN_NAME = 'UsageCount') " +
                        "ALTER TABLE PromoCodes ADD UsageCount int NOT NULL DEFAULT 0");
                    
                    // Создаем тестовый промокод
                    context.Database.ExecuteSqlRaw(
                        "IF NOT EXISTS (SELECT * FROM PromoCodes WHERE Code = 'TEST10') " +
                        "INSERT INTO PromoCodes (Code, DiscountPercent, IsActive, CreatedAt, UsageCount) " +
                        "VALUES ('TEST10', 10.00, 1, GETDATE(), 0)");
                } catch {
                    // Таблица уже существует
                }
                
                // Добавляем колонку AdminComments в таблицу Orders
                try {
                    context.Database.ExecuteSqlRaw(
                        "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'AdminComments') " +
                        "ALTER TABLE Orders ADD AdminComments nvarchar(max) NULL");
                } catch {
                    // Колонка уже существует
                }
                
                // Добавляем колонки для Telegram интеграции в таблицу Orders
                try {
                    context.Database.ExecuteSqlRaw(
                        "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'Source') " +
                        "ALTER TABLE Orders ADD Source nvarchar(50) NULL");
                    
                    context.Database.ExecuteSqlRaw(
                        "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'TelegramUserId') " +
                        "ALTER TABLE Orders ADD TelegramUserId bigint NULL");
                    
                    context.Database.ExecuteSqlRaw(
                        "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'OrderNumber') " +
                        "ALTER TABLE Orders ADD OrderNumber nvarchar(50) NULL");
                    
                    context.Database.ExecuteSqlRaw(
                        "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'WebUserId') " +
                        "ALTER TABLE Orders ADD WebUserId uniqueidentifier NULL");
                    
                    // Обновляем старые заказы
                    context.Database.ExecuteSqlRaw(
                        "UPDATE Orders SET Source = 'Сайт' WHERE Source IS NULL AND Coment NOT LIKE '%Telegram%'");
                    context.Database.ExecuteSqlRaw(
                        "UPDATE Orders SET Source = 'Telegram' WHERE Source IS NULL AND Coment LIKE '%Telegram%'");
                    
                    // Генерируем номера для старых заказов
                    context.Database.ExecuteSqlRaw(
                        "UPDATE Orders SET OrderNumber = 'WEB' + FORMAT(CreatedDate, 'yyyyMMdd') + LEFT(CAST(Id AS NVARCHAR(36)), 6) WHERE OrderNumber IS NULL");
                } catch {
                    // Колонки уже существуют
                }
                
                // Создаем таблицу TelegramUsers
                try {
                    context.Database.ExecuteSqlRaw(
                        "IF OBJECT_ID('TelegramUsers', 'U') IS NULL " +
                        "CREATE TABLE TelegramUsers (" +
                        "TelegramId bigint NOT NULL PRIMARY KEY, " +
                        "FirstName nvarchar(100) NULL, " +
                        "LastName nvarchar(100) NULL, " +
                        "Username nvarchar(100) NULL, " +
                        "Phone nvarchar(20) NULL, " +
                        "Address nvarchar(500) NULL, " +
                        "CreatedDate datetime2 NOT NULL, " +
                        "LastActivity datetime2 NOT NULL, " +
                        "WebUserId uniqueidentifier NULL, " +
                        "Email nvarchar(256) NULL, " +
                        "IsLinkedToWebsite bit NOT NULL DEFAULT 0)");
                } catch {
                    // Таблица уже существует
                }
                
                // Создаем таблицу ProductReviews
                try {
                    context.Database.ExecuteSqlRaw(
                        "IF OBJECT_ID('ProductReviews', 'U') IS NULL " +
                        "CREATE TABLE ProductReviews (" +
                        "Id uniqueidentifier NOT NULL PRIMARY KEY, " +
                        "ProductId uniqueidentifier NOT NULL, " +
                        "UserId nvarchar(450) NOT NULL, " +
                        "Rating int NOT NULL CHECK (Rating >= 1 AND Rating <= 5), " +
                        "Comment nvarchar(1000) NULL, " +
                        "CreatedAt datetime2 NOT NULL, " +
                        "FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE)");
                } catch {
                    // Таблица уже существует
                }
                
                // Создаем таблицу ReviewReplies
                try {
                    context.Database.ExecuteSqlRaw(
                        "IF OBJECT_ID('ReviewReplies', 'U') IS NULL " +
                        "CREATE TABLE ReviewReplies (" +
                        "Id uniqueidentifier NOT NULL PRIMARY KEY, " +
                        "ReviewId uniqueidentifier NOT NULL, " +
                        "AdminId nvarchar(450) NOT NULL, " +
                        "Reply nvarchar(1000) NOT NULL, " +
                        "CreatedAt datetime2 NOT NULL, " +
                        "FOREIGN KEY (ReviewId) REFERENCES ProductReviews(Id) ON DELETE CASCADE)");
                } catch {
                    // Таблица уже существует
                }
                
                // Инициализируем тестовые данные
                try {
                    await ShoeShop.Data.Initialization.TestDataInitializer.InitializeAsync(context);
                } catch {
                    // Ошибка инициализации
                }
                
                // Инициализируем тестовые остатки
                try {
                    await ShoeShop.Data.Initialization.StockInitializer.InitializeAsync(context, stockService);
                } catch {
                    // Ошибка инициализации - продолжаем без остатков
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                app.UseMigrationsEndPoint();
            } else {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            // Обработка 404 ошибок
            app.UseStatusCodePagesWithReExecute("/404");

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            
            // Swagger для API
            if (app.Environment.IsDevelopment()) {
                app.UseSwagger();
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShoeShop API v1");
                    c.RoutePrefix = "api-docs";
                });
            }
            
            app.MapRazorPages();
            app.MapControllers();

            app.Run();
        }
    }
}
