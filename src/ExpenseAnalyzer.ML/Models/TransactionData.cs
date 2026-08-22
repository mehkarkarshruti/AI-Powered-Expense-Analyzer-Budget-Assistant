using Microsoft.ML.Data;

namespace ExpenseAnalyzer.ML.Models;

/// <summary>
/// ML.NET Input Data Schema mapping directly to upi_data_enhanced.csv columns and real-time transaction features.
/// </summary>
public class TransactionData
{
    [LoadColumn(0)]
    public float UserId { get; set; }

    [LoadColumn(1)]
    public float MonthIndex { get; set; }

    [LoadColumn(2)]
    public float DaysElapsed { get; set; }

    [LoadColumn(3)]
    public float DaysInMonth { get; set; }

    [LoadColumn(4)]
    public float HistoricalAverage { get; set; }

    [LoadColumn(5)]
    public float PrevMonthSpending { get; set; }

    [LoadColumn(6)]
    public float CurrentSpentSoFar { get; set; }

    [LoadColumn(7)]
    public float TransactionCountSoFar { get; set; }

    [LoadColumn(8), ColumnName("Label")]
    public float TargetTotalMonthlySpending { get; set; }

    // Real-time transactional properties (used for text featurization / anomaly inference)
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public float Amount { get; set; }
}
