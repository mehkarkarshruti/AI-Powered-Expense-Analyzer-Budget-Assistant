namespace ExpenseAnalyzer.API.DTOs;

public class AlertResponse
{
    public int AlertId { get; set; }
    public int? BudgetId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
