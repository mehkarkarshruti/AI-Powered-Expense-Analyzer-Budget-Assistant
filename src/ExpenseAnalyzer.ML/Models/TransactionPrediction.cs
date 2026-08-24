using Microsoft.ML.Data;

namespace ExpenseAnalyzer.ML.Models;

/// <summary>
/// ML.NET Prediction Output Schema containing predicted score and anomaly flags.
/// </summary>
public class TransactionPrediction
{
    [ColumnName("Score")]
    public float Score { get; set; }

    public float PredictedMonthlySpending => Score;

    public float Confidence { get; set; }

    public bool IsAnomaly { get; set; }

    public float AnomalyScore { get; set; }

    public string AnomalyReason { get; set; } = string.Empty;
}

/// <summary>
/// Alias model for backwards compatibility with existing pipelines.
/// </summary>
public class SpendingModelOutput
{
    [ColumnName("Score")]
    public float PredictedMonthlySpending { get; set; }

    public float Score => PredictedMonthlySpending;
}
