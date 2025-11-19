using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Data;
using ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Models;
using ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=(localdb)\\mssqllocaldb;Database=ShoeShopMultiTenant;Trusted_Connection=true;MultipleActiveResultSets=true";
builder.Services.AddDbContext<ShoeShop.MultiTenantAdmin.Data.ApplicationContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ShoeShop.MultiTenantAdmin.Data.ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 5;
}).AddRoles<ShoeShop.MultiTenantAdmin.Data.ApplicationRole>().AddEntityFrameworkStores<ShoeShop.MultiTenantAdmin.Data.ApplicationContext>();

builder.Services.AddScoped<CompanyService>();
builder.Services.AddScoped<ShoeShop.MultiTenantAdmin.Models.IAdminRepository, ShoeShop.MultiTenantAdmin.Data.AdminRepository>();
builder.Services.AddScoped<ShoeShop.MultiTenantAdmin.Models.IProductRepository, ShoeShop.MultiTenantAdmin.Data.ProductRepository>();
builder.Services.AddScoped<ShoeShop.MultiTenantAdmin.Models.IProductStockRepository, ShoeShop.MultiTenantAdmin.Data.ProductStockRepository>();
builder.Services.AddScoped<ShoeShop.MultiTenantAdmin.Infrastructure.IImageManager, ShoeShop.MultiTenantAdmin.Services.ImageManager>();
builder.Services.AddScoped<ShoeShop.MultiTenantAdmin.Infrastructure.IProductManager, ShoeShop.MultiTenantAdmin.Services.ProductManager>();
builder.Services.AddScoped<ShoeShop.MultiTenantAdmin.Services.StockService>();
builder.Services.AddScoped<ShoeShop.MultiTenantAdmin.Services.SalesStatisticsService>();
builder.Services.AddScoped<ShoeShop.MultiTenantAdmin.Services.PromoCodeService>();
builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope()) {
    var context = scope.ServiceProvider.GetRequiredService<ShoeShop.MultiTenantAdmin.Data.ApplicationContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/companies"));

app.Run();
