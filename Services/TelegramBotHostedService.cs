using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace ExpenseBot.Services;

public class TelegramBotHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramBotHostedService> _logger;
    private ITelegramBotClient? _botClient;

    public TelegramBotHostedService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<TelegramBotHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var botToken = _configuration["TelegramBot:Token"];
        if (string.IsNullOrEmpty(botToken))
        {
            _logger.LogError("Telegram bot token not configured. Set TelegramBot:Token in appsettings.json");
            return;
        }

        _botClient = new TelegramBotClient(botToken);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message]
        };

        _logger.LogInformation("Starting Telegram bot...");

        var handler = new DefaultUpdateHandler(
            HandleUpdateAsync,
            HandleErrorAsync);

        await _botClient.ReceiveAsync(
            handler,
            receiverOptions,
            stoppingToken
        );
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var botService = scope.ServiceProvider.GetRequiredService<TelegramBotService>();
        await botService.HandleUpdateAsync(botClient, update, cancellationToken);
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Error while handling Telegram update");
        return Task.CompletedTask;
    }
}
