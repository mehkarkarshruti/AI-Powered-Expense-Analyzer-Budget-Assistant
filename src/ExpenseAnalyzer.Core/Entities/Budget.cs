namespace ExpenseAnalyzer.Core.Entities;

public class Budget
{
    public int BudgetId { get; set; }
    public int UserId { get; set; }
    public string Month { get; set; } = string.Empty; // Format: YYYY-MM (e.g. 2026-08)
    public decimal Amount { get; set; }

    // Navigation properties
    public User? User { get; set; }
}
