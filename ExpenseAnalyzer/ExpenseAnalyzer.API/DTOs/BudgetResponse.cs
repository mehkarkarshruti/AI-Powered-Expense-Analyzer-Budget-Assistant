namespace ExpenseAnalyzer.API.DTOs;

public class BudgetResponse
{
    public int BudgetId { get; set; }
    public int UserId { get; set; }
    public int? CategoryId { get; set; }
    public byte Month { get; set; }
    public short Year { get; set; }
    public decimal BudgetAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
