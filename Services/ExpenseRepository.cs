using System.Text.Json;
using ExpenseBot.Models;

namespace ExpenseBot.Services;

public class ExpenseRepository : IExpenseRepository
{
    private readonly string _filePath;
    private readonly object _lock = new();
    private List<Expense> _expenses;

    public ExpenseRepository(string filePath = "expenses.json")
    {
        _filePath = filePath;
        _expenses = LoadFromFile();
    }

    public void Add(Expense expense)
    {
        lock (_lock)
        {
            _expenses.Add(expense);
            SaveToFile();
        }
    }

    public List<Expense> GetByUserId(long userId)
    {
        lock (_lock)
        {
            return _expenses
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.CreatedAt)
                .ToList();
        }
    }

    public List<Expense> GetByDateRange(long userId, DateTime from, DateTime to)
    {
        lock (_lock)
        {
            return _expenses
                .Where(e => e.UserId == userId && e.CreatedAt >= from && e.CreatedAt <= to)
                .OrderByDescending(e => e.CreatedAt)
                .ToList();
        }
    }

    private List<Expense> LoadFromFile()
    {
        if (!File.Exists(_filePath))
        {
            return new List<Expense>();
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<Expense>>(json) ?? new List<Expense>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading expenses: {ex.Message}");
            return new List<Expense>();
        }
    }

    private void SaveToFile()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(_expenses, options);
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving expenses: {ex.Message}");
        }
    }
}
