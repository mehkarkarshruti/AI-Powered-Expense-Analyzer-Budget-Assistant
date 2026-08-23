namespace ExpenseAnalyzer.API.DTOs;

public class SpendingPredictionDto
{
    public decimal PredictedMonthlySpending { get; set; }
    public decimal? MonthlyBudget { get; set; }
    public bool IsBudgetLikelyToBeExceeded { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class BudgetStatusDto
{
    public BudgetResponse? Budget { get; set; }
    public List<string> ActiveAlerts { get; set; } = [];
    public SpendingPredictionDto? Prediction { get; set; }
}
