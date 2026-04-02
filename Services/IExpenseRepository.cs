using ExpenseBot.Models;

namespace ExpenseBot.Services;

public interface IExpenseRepository
{
    void Add(Expense expense);
    List<Expense> GetByUserId(long userId);
    List<Expense> GetByDateRange(long userId, DateTime from, DateTime to);
}
