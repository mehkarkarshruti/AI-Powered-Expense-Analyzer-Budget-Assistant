using System.ComponentModel.DataAnnotations;

namespace ExpenseAnalyzer.Core.DTOs.Prediction;

/// <summary>
/// DTO representing an incoming real-time transaction prediction request.
/// </summary>
public class PredictionRequestDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Range(0.01, 1_000_000, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Category-wise breakdown forecast DTO.
/// </summary>
public class CategoryForecastDto
{
    public string Category { get; set; } = string.Empty;
    public decimal ForecastAmount { get; set; }
    public double ConfidenceScore { get; set; }
}

/// <summary>
/// Request DTO for triggering model re-training.
/// </summary>
public class ModelTrainingRequestDto
{
    public string? TrainingDataPath { get; set; }
    public string? OutputModelPath { get; set; }
}

/// <summary>
/// Response DTO containing evaluation metrics and status of ML model training.
/// </summary>
public class ModelTrainingResultDto
{
    public bool Success { get; set; }
    public double RSquared { get; set; }
    public double RMSE { get; set; }
    public double MAE { get; set; }
    public string ModelPath { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime TrainedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Response DTO containing full prediction analysis, budget comparison, category forecast, and anomaly flags.
/// </summary>
public class SpendingPredictionDto
{
    public int UserId { get; set; }
    public string UserIdentifier { get; set; } = string.Empty;
    public string CurrentMonth { get; set; } = string.Empty;
    public string PredictionPeriod => CurrentMonth;
    public decimal HistoricalAverage { get; set; }
    public decimal CurrentMonthSpending { get; set; }
    public decimal PredictedMonthlySpending { get; set; }
    public decimal PredictedAmount => PredictedMonthlySpending;
    public decimal? MonthlyBudget { get; set; }
    public decimal RemainingBudget { get; set; }
    public string PredictionStatus { get; set; } = "Normal";
    public double ConfidenceScore { get; set; }
    public double Confidence => ConfidenceScore;
    public string Currency { get; set; } = "INR";
    public bool IsBudgetLikelyToBeExceeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsFallback { get; set; }

    // Category-wise Breakdown & Anomaly Detection Flags
    public List<CategoryForecastDto> CategoryForecasts { get; set; } = new();
    public bool IsAnomaly { get; set; }
    public double AnomalyScore { get; set; }
    public string AnomalyReason { get; set; } = string.Empty;
}
