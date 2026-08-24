namespace ExpenseAnalyzer.Core.Entities;

public class Expense
{
    public int ExpenseId { get; set; }
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;

    // Navigation properties
    public User? User { get; set; }
    public Category? Category { get; set; }
}
