namespace ExpenseAnalyzer.Core.Entities;

public class User
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime SignupDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
