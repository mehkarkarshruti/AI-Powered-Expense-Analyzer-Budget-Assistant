using ExpenseAnalyzer.Core.DTOs.Prediction;

namespace ExpenseAnalyzer.Core.Interfaces;

/// <summary>
/// Machine Learning Prediction Engine contract providing real-time inference,
/// monthly spending forecasting, anomaly detection, and automated model training.
/// </summary>
public interface IPredictionEngine
{
    /// <summary>
    /// Evaluates a single transaction for predicted impact and anomaly flags.
    /// </summary>
    Task<SpendingPredictionDto> PredictExpenseAsync(PredictionRequestDto request);

    /// <summary>
    /// Forecasts monthly spending for a given user and target month.
    /// </summary>
    Task<SpendingPredictionDto> ForecastMonthlySpendingAsync(string userId, DateTime targetMonth);

    /// <summary>
    /// Trains/re-trains the ML.NET model from a CSV training dataset and saves the zip model output.
    /// </summary>
    Task<ModelTrainingResultDto> TrainModelAsync(string trainingDataPath, string outputModelPath);

    /// <summary>
    /// Predicts monthly spending for a specified user integer ID.
    /// </summary>
    Task<SpendingPredictionDto> PredictMonthlySpendingAsync(int userId);
}
