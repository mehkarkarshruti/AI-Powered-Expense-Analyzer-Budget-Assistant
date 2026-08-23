using System;

namespace ExpenseAnalyzer.API.DTOs
{
    public class BudgetResponseDto
    {
        public int BudgetId { get; set; }
        public byte Month { get; set; }
        public short Year { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal CurrentSpending { get; set; }
        public decimal RemainingBudget { get; set; }
    }

    public class SpendingPredictionDto
    {
        public decimal PredictedMonthlySpending { get; set; }
        public decimal? MonthlyBudget { get; set; }
        public bool IsBudgetLikelyToBeExceeded { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class BudgetStatusDto
    {
        public BudgetResponseDto? Budget { get; set; }
        public List<string> ActiveAlerts { get; set; } = new List<string>();
        public SpendingPredictionDto? Prediction { get; set; }
    }
}
