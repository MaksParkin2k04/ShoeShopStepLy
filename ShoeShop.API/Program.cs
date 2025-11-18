var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure pipeline
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
