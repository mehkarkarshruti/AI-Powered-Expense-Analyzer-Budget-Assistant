namespace ExpenseAnalyzer.Core.Entities;

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
