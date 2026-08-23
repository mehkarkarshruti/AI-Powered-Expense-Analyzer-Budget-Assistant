using Microsoft.ML.Data;

namespace ExpenseAnalyzer.ML.Models;

// Forward definition maintained for compatibility
public class ModelOutputForward
{
    [ColumnName("Score")]
    public float PredictedMonthlySpending { get; set; }
}
