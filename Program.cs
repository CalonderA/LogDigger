using CacheApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<CacheService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
