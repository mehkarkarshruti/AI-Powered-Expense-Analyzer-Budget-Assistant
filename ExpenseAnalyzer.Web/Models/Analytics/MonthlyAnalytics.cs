namespace ExpenseAnalyzer.Web.Models.Analytics;

public class MonthlyAnalytics
{
    public string Month { get; set; } = string.Empty;
    public decimal TotalExpenses { get; set; }
    public int TotalTransactions { get; set; }
    public decimal AverageExpense { get; set; }
}
