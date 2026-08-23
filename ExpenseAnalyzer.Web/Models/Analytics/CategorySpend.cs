namespace ExpenseAnalyzer.Web.Models.Analytics;

public class CategorySpend
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
}
