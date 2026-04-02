using ExpenseBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ExpenseBot.Services;

public class TelegramBotService
{
    private readonly IExpenseRepository _repository;
    private readonly TimeZoneInfo _localTimeZone;

    public TelegramBotService(IExpenseRepository repository)
    {
        _repository = repository;
        _localTimeZone = TimeZoneInfo.Local;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message || update.Message?.Type != MessageType.Text)
        {
            if (update.Message != null && update.Message.Type != MessageType.Text)
            {
                await SendMessage(botClient, update.Message.Chat.Id, "Я понимаю только текст и команды", cancellationToken);
            }
            return;
        }

        var message = update.Message;
        var chatId = message.Chat.Id;
        var text = message.Text?.Trim() ?? string.Empty;

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Message from {chatId}: {text}");

        try
        {
            var response = await ProcessMessageAsync(chatId, text);
            await SendMessage(botClient, chatId, response, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message: {ex.Message}");
            await SendMessage(botClient, chatId, "Произошла ошибка при обработке сообщения", cancellationToken);
        }
    }

    private async Task<string> ProcessMessageAsync(long userId, string text)
    {
        var command = text.ToLowerInvariant();

        return command switch
        {
            "/start" or "/help" => GetHelpText(),
            "/today" => GetTodayExpenses(userId),
            "/week" => GetWeekExpenses(userId),
            "/month" => GetMonthExpenses(userId),
            "/last" => GetLastExpenses(userId),
            _ => TryAddExpense(userId, text)
        };
    }

    private static string GetHelpText()
    {
        return """
            💰 Бот для учета личных расходов

            📌 Как добавить расход:
            Просто отправьте: <сумма> <причина>
            Пример: 500 Обед

            📋 Доступные команды:
            /today - расходы за сегодня
            /week - расходы за последние 7 дней
            /month - расходы за текущий месяц
            /last - последние 10 трат
            /help - эта справка
            """;
    }

    private string GetTodayExpenses(long userId)
    {
        var now = DateTime.Now;
        var startOfDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Local);
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        var fromUtc = TimeZoneInfo.ConvertTimeToUtc(startOfDay, _localTimeZone);
        var toUtc = TimeZoneInfo.ConvertTimeToUtc(endOfDay, _localTimeZone);

        var expenses = _repository.GetByDateRange(userId, fromUtc, toUtc);
        var total = expenses.Sum(e => e.Amount);

        return $"За сегодня потрачено: {total:F2} руб. (записей: {expenses.Count})";
    }

    private string GetWeekExpenses(long userId)
    {
        var now = DateTime.Now;
        var startOfWeek = now.Date.AddDays(-6);
        var endOfWeek = now.Date.AddDays(1).AddTicks(-1);

        var fromUtc = TimeZoneInfo.ConvertTimeToUtc(startOfWeek, _localTimeZone);
        var toUtc = TimeZoneInfo.ConvertTimeToUtc(endOfWeek, _localTimeZone);

        var expenses = _repository.GetByDateRange(userId, fromUtc, toUtc);
        var total = expenses.Sum(e => e.Amount);

        return $"За последние 7 дней: {total:F2} руб.";
    }

    private string GetMonthExpenses(long userId)
    {
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Local);
        var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

        var fromUtc = TimeZoneInfo.ConvertTimeToUtc(startOfMonth, _localTimeZone);
        var toUtc = TimeZoneInfo.ConvertTimeToUtc(endOfMonth, _localTimeZone);

        var expenses = _repository.GetByDateRange(userId, fromUtc, toUtc);
        var total = expenses.Sum(e => e.Amount);

        return $"За этот месяц: {total:F2} руб.";
    }

    private string GetLastExpenses(long userId)
    {
        var expenses = _repository.GetByUserId(userId).Take(10).ToList();

        if (expenses.Count == 0)
        {
            return "У вас пока нет записей о расходах";
        }

        var lines = new List<string>();
        for (int i = 0; i < expenses.Count; i++)
        {
            var e = expenses[i];
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(e.CreatedAt, _localTimeZone);
            lines.Add($"{i + 1}. {e.Amount:F0} руб. | {e.Reason} ({localTime:dd.MM HH:mm})");
        }

        return string.Join("\n", lines);
    }

    private string TryAddExpense(long userId, string text)
    {
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return "Ошибка: Начните сообщение с суммы (числа). Пример: 500 Обед";
        }

        if (!decimal.TryParse(parts[0], out var amount) || amount <= 0)
        {
            return "Ошибка: Начните сообщение с суммы (числа). Пример: 500 Обед";
        }

        var reason = parts.Length > 1
            ? string.Join(' ', parts.Skip(1))
            : "Без описания";

        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };

        _repository.Add(expense);

        return $"Записано: {amount:F2} руб. ({reason})";
    }

    private static async Task SendMessage(ITelegramBotClient botClient, long chatId, string text, CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(chatId, text, cancellationToken: cancellationToken);
    }
}
