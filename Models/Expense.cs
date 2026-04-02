namespace ExpenseBot.Models;

public class Expense
{
    public Guid Id { get; set; }
    public long UserId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
