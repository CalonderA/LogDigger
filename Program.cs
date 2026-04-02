using ExpenseBot.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<TelegramBotService>();
builder.Services.AddHostedService<TelegramBotHostedService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

app.MapGet("/", () => "ExpenseBot is running! Use Telegram to interact with the bot.");

app.Run();
