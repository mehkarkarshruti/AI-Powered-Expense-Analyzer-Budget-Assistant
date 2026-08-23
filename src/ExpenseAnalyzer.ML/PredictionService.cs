using ExpenseAnalyzer.Core.DTOs.Prediction;
using ExpenseAnalyzer.Core.Interfaces;
using ExpenseAnalyzer.Infrastructure.Data;
using ExpenseAnalyzer.ML.Models;
using ExpenseAnalyzer.ML.Pipelines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace ExpenseAnalyzer.ML;

/// <summary>
/// Senior Architect Implementation of IPredictionEngine.
/// Manages thread-safe model caching, real-time transaction inference, category-wise monthly forecasting,
/// anomaly threshold evaluations, and model training orchestration.
/// </summary>
public class PredictionService : IPredictionEngine
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<PredictionService> _logger;
    private readonly ModelTrainingPipeline _trainingPipeline;

    private static ITransformer? _cachedModel;
    private static readonly object _modelLock = new();

    private readonly string _defaultDataPath;
    private readonly string _defaultModelPath;

    public PredictionService(AppDbContext dbContext, ILogger<PredictionService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _trainingPipeline = new ModelTrainingPipeline();

        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        _defaultDataPath = Path.Combine(baseDir, "Data", "upi_data_enhanced.csv");
        if (!File.Exists(_defaultDataPath))
        {
            string devPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "ExpenseAnalyzer.ML", "Data", "upi_data_enhanced.csv");
            if (File.Exists(devPath))
            {
                _defaultDataPath = devPath;
            }
        }

        _defaultModelPath = Path.Combine(baseDir, "Models", "spending-model.zip");
    }

    /// <summary>
    /// Predicts impact and evaluates anomaly status for an incoming transaction request.
    /// </summary>
    public async Task<SpendingPredictionDto> PredictExpenseAsync(PredictionRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        int parsedUserId = int.TryParse(request.UserId, out int uid) ? uid : 1;
        var monthlyForecast = await PredictMonthlySpendingAsync(parsedUserId);

        // Fetch user expense history for anomaly threshold detection
        var recentExpenses = await _dbContext.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == parsedUserId)
            .ToListAsync();

        bool isAnomaly = false;
        double anomalyScore = 0.0;
        string anomalyReason = "Transaction amount is within normal historical range.";

        if (recentExpenses.Count > 0)
        {
            decimal avgAmount = recentExpenses.Average(e => e.Amount);
            double stdDev = Math.Sqrt((double)recentExpenses.Sum(e => (e.Amount - avgAmount) * (e.Amount - avgAmount)) / recentExpenses.Count);

            if (stdDev > 0)
            {
                anomalyScore = (double)(request.Amount - avgAmount) / stdDev;
            }
            else if (avgAmount > 0)
            {
                anomalyScore = (double)(request.Amount / avgAmount);
            }

            if (anomalyScore > 3.0 || (avgAmount > 0 && request.Amount > avgAmount * 4m))
            {
                isAnomaly = true;
                anomalyReason = $"High spending anomaly detected! Transaction amount ({request.Amount:C}) is significantly higher than historical average ({avgAmount:C}).";
            }
        }

        monthlyForecast.IsAnomaly = isAnomaly;
        monthlyForecast.AnomalyScore = Math.Round(anomalyScore, 2);
        monthlyForecast.AnomalyReason = anomalyReason;

        return monthlyForecast;
    }

    /// <summary>
    /// Forecasts monthly spending for a specified user and target month.
    /// </summary>
    public async Task<SpendingPredictionDto> ForecastMonthlySpendingAsync(string userId, DateTime targetMonth)
    {
        int parsedUserId = int.TryParse(userId, out int uid) ? uid : 1;
        var prediction = await PredictMonthlySpendingAsync(parsedUserId);

        string monthStr = targetMonth.ToString("yyyy-MM");
        prediction.CurrentMonth = monthStr;
        return prediction;
    }

    /// <summary>
    /// Executes automated ML model training pipeline and saves the zip model output.
    /// </summary>
    public Task<ModelTrainingResultDto> TrainModelAsync(string trainingDataPath, string outputModelPath)
    {
        string dataPath = string.IsNullOrWhiteSpace(trainingDataPath) ? _defaultDataPath : trainingDataPath;
        string modelPath = string.IsNullOrWhiteSpace(outputModelPath) ? _defaultModelPath : outputModelPath;

        _logger.LogInformation("Starting ML model training from {DataPath} -> {ModelPath}", dataPath, modelPath);

        try
        {
            var result = _trainingPipeline.TrainAndSaveModel(dataPath, modelPath);

            lock (_modelLock)
            {
                _cachedModel = _trainingPipeline.LoadModel(modelPath);
            }

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete ML model training pipeline.");
            return Task.FromResult(new ModelTrainingResultDto
            {
                Success = false,
                Message = $"Training failed: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Main prediction engine algorithm computing monthly projections, category-wise breakdowns, and budget evaluations.
    /// </summary>
    public async Task<SpendingPredictionDto> PredictMonthlySpendingAsync(int userId)
    {
        DateTime now = DateTime.UtcNow;
        string currentMonthStr = now.ToString("yyyy-MM");
        int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
        int daysElapsed = Math.Max(now.Day, 1);

        // 1. Fetch user budget for target month
        var budgetEntity = await _dbContext.Budgets
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.UserId == userId && b.Month == currentMonthStr);

        decimal? monthlyBudget = budgetEntity?.Amount;

        // 2. Fetch user expenses safely without rigid EF inner join on Category
        var userExpenses = await _dbContext.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .ToListAsync();

        // Edge Case: No user expenses available
        if (userExpenses.Count == 0)
        {
            return new SpendingPredictionDto
            {
                UserId = userId,
                UserIdentifier = userId.ToString(),
                CurrentMonth = currentMonthStr,
                HistoricalAverage = 0m,
                CurrentMonthSpending = 0m,
                PredictedMonthlySpending = 0m,
                MonthlyBudget = monthlyBudget,
                RemainingBudget = monthlyBudget ?? 0m,
                PredictionStatus = "InsufficientData",
                ConfidenceScore = 0.0,
                IsBudgetLikelyToBeExceeded = false,
                Message = "No historical expense records available to generate spending predictions.",
                IsFallback = true,
                CategoryForecasts = new List<CategoryForecastDto>()
            };
        }

        // Current Month Expenses
        var currentMonthExpenses = userExpenses
            .Where(e => e.Date.Year == now.Year && e.Date.Month == now.Month)
            .ToList();

        decimal currentSpentSoFar = currentMonthExpenses.Sum(e => e.Amount);
        int currentTxCount = currentMonthExpenses.Count;

        // Historical Expenses (Prior to current month)
        var historicalExpenses = userExpenses
            .Where(e => e.Date.Year < now.Year || (e.Date.Year == now.Year && e.Date.Month < now.Month))
            .ToList();

        decimal historicalAvg = 0m;
        decimal prevMonthSpending = 0m;

        if (historicalExpenses.Count > 0)
        {
            var monthlySums = historicalExpenses
                .GroupBy(e => new { e.Date.Year, e.Date.Month })
                .Select(g => g.Sum(e => e.Amount))
                .ToList();

            historicalAvg = monthlySums.Average();

            var prevMonthDate = now.AddMonths(-1);
            var prevMonthGroup = historicalExpenses
                .Where(e => e.Date.Year == prevMonthDate.Year && e.Date.Month == prevMonthDate.Month)
                .ToList();

            prevMonthSpending = prevMonthGroup.Count > 0 ? prevMonthGroup.Sum(e => e.Amount) : historicalAvg;
        }
        else
        {
            historicalAvg = currentSpentSoFar;
            prevMonthSpending = currentSpentSoFar;
        }

        // ML Inference Execution
        float predictedAmountFloat = -1f;
        bool isFallback = false;

        try
        {
            ITransformer? model = GetOrLoadModel();
            if (model != null)
            {
                var predEngine = _trainingPipeline.CreatePredictionEngine(model);
                var input = new TransactionData
                {
                    UserId = userId,
                    MonthIndex = now.Month,
                    DaysElapsed = daysElapsed,
                    DaysInMonth = daysInMonth,
                    HistoricalAverage = (float)historicalAvg,
                    PrevMonthSpending = (float)prevMonthSpending,
                    CurrentSpentSoFar = (float)currentSpentSoFar,
                    TransactionCountSoFar = currentTxCount
                };

                var output = predEngine.Predict(input);
                predictedAmountFloat = output.PredictedMonthlySpending;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ML.NET model prediction failed. Executing fallback estimation strategy.");
        }

        decimal predictedMonthlySpending;

        if (predictedAmountFloat <= 0f || userExpenses.Count < 3)
        {
            isFallback = true;
            if (daysElapsed > 0 && currentSpentSoFar > 0)
            {
                // Linear velocity extrapolation
                predictedMonthlySpending = (currentSpentSoFar / daysElapsed) * daysInMonth;
            }
            else if (historicalAvg > 0)
            {
                predictedMonthlySpending = historicalAvg;
            }
            else
            {
                predictedMonthlySpending = currentSpentSoFar;
            }
        }
        else
        {
            predictedMonthlySpending = Math.Max((decimal)predictedAmountFloat, currentSpentSoFar);
        }

        // Clamp prediction to at least current spend so far
        predictedMonthlySpending = Math.Max(predictedMonthlySpending, currentSpentSoFar);
        predictedMonthlySpending = Math.Round(predictedMonthlySpending, 2);

        // Fetch category dictionary for category forecast
        var categoryIds = userExpenses.Select(e => e.CategoryId).Distinct().ToList();
        var categoryMap = await _dbContext.Categories
            .AsNoTracking()
            .Where(c => categoryIds.Contains(c.CategoryId))
            .ToDictionaryAsync(c => c.CategoryId, c => c.Name);

        // Category-wise Breakdown Forecast
        var categoryForecasts = userExpenses
            .GroupBy(e => categoryMap.TryGetValue(e.CategoryId, out var catName) ? catName : (e.Category?.Name ?? "General"))
            .Select(g =>
            {
                decimal catHistoricalAvg = g.Sum(e => e.Amount);
                decimal catRatio = userExpenses.Sum(e => e.Amount) > 0 ? catHistoricalAvg / userExpenses.Sum(e => e.Amount) : 0m;
                decimal catForecast = Math.Round(predictedMonthlySpending * catRatio, 2);
                return new CategoryForecastDto
                {
                    Category = g.Key,
                    ForecastAmount = catForecast,
                    ConfidenceScore = isFallback ? 0.60 : 0.85
                };
            })
            .ToList();

        // Budget comparison logic
        bool isBudgetLikelyToBeExceeded = false;
        decimal remainingBudget = 0m;
        string predictionStatus = "Normal";
        string message;
        double confidenceScore;

        if (monthlyBudget.HasValue && monthlyBudget.Value > 0)
        {
            remainingBudget = monthlyBudget.Value - predictedMonthlySpending;

            if (predictedMonthlySpending > monthlyBudget.Value)
            {
                isBudgetLikelyToBeExceeded = true;
                predictionStatus = "LikelyToExceed";
                message = $"Warning: Your predicted monthly spending ({predictedMonthlySpending:C}) is projected to exceed your budget ({monthlyBudget.Value:C}).";
            }
            else if (predictedMonthlySpending >= monthlyBudget.Value * 0.85m)
            {
                predictionStatus = "NearBudgetLimit";
                message = $"Caution: Your predicted monthly spending ({predictedMonthlySpending:C}) is approaching your budget limit ({monthlyBudget.Value:C}).";
            }
            else
            {
                predictionStatus = "UnderBudget";
                message = $"Good job! Your predicted spending ({predictedMonthlySpending:C}) is well within your budget ({monthlyBudget.Value:C}).";
            }
        }
        else
        {
            message = $"Predicted monthly spending is {predictedMonthlySpending:C}. No monthly budget is set for this month.";
            predictionStatus = "NoBudgetSet";
        }

        // Confidence calculation
        if (isFallback)
        {
            confidenceScore = userExpenses.Count < 3 ? 0.45 : 0.65;
        }
        else
        {
            confidenceScore = Math.Min(0.85 + (daysElapsed / (double)daysInMonth) * 0.10, 0.95);
        }

        return new SpendingPredictionDto
        {
            UserId = userId,
            UserIdentifier = userId.ToString(),
            CurrentMonth = currentMonthStr,
            HistoricalAverage = Math.Round(historicalAvg, 2),
            CurrentMonthSpending = Math.Round(currentSpentSoFar, 2),
            PredictedMonthlySpending = predictedMonthlySpending,
            MonthlyBudget = monthlyBudget,
            RemainingBudget = Math.Round(remainingBudget, 2),
            PredictionStatus = predictionStatus,
            ConfidenceScore = Math.Round(confidenceScore, 2),
            IsBudgetLikelyToBeExceeded = isBudgetLikelyToBeExceeded,
            Message = message,
            IsFallback = isFallback,
            CategoryForecasts = categoryForecasts,
            IsAnomaly = false,
            AnomalyScore = 0.0,
            AnomalyReason = "No transaction anomaly evaluated."
        };
    }

    /// <summary>
    /// Thread-safe model cache loader.
    /// </summary>
    private ITransformer? GetOrLoadModel()
    {
        if (_cachedModel != null)
            return _cachedModel;

        lock (_modelLock)
        {
            if (_cachedModel != null)
                return _cachedModel;

            _cachedModel = _trainingPipeline.LoadModel(_defaultModelPath);

            if (_cachedModel == null && File.Exists(_defaultDataPath))
            {
                _logger.LogInformation("No saved ML model found at {ModelPath}. Training initial model from dataset.", _defaultModelPath);
                try
                {
                    var result = _trainingPipeline.TrainAndSaveModel(_defaultDataPath, _defaultModelPath);
                    _cachedModel = _trainingPipeline.LoadModel(_defaultModelPath);
                    _logger.LogInformation("Initial ML model trained successfully. R2: {R2}, RMSE: {RMSE}", result.RSquared, result.RMSE);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to train initial ML model during startup.");
                }
            }
        }

        return _cachedModel;
    }
}
