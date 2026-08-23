namespace ExpenseAnalyzer.Core.DTOs.Analytics;

/// <summary>
/// DTO representing spending breakdown for a specific category.
/// </summary>
public class CategorySpendDto
{
    /// <summary>
    /// The name of the expense category.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// The total amount spent in this category.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// The percentage of total spending representing this category (range: 0.0 to 100.0).
    /// </summary>
    public decimal Percentage { get; set; }
}
