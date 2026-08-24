using ExpenseAnalyzer.Core.DTOs.Prediction;
using ExpenseAnalyzer.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExpenseAnalyzer.API.Controllers;

/// <summary>
/// Web API Controller providing HTTP endpoints for ML spending predictions, transaction anomaly checks,
/// monthly spending forecasts, and automated model training operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PredictionController : ControllerBase
{
    private readonly IPredictionEngine _predictionEngine;
    private readonly ILogger<PredictionController> _logger;

    public PredictionController(IPredictionEngine predictionEngine, ILogger<PredictionController> logger)
    {
        _predictionEngine = predictionEngine ?? throw new ArgumentNullException(nameof(predictionEngine));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Evaluates a single transaction in real-time for spending predictions and anomaly alerts.
    /// </summary>
    /// <param name="request">Transaction prediction payload containing Amount, Description, Category, Date, UserId.</param>
    /// <returns>SpendingPredictionDto with prediction details, anomaly flags, and budget status.</returns>
    [HttpPost("predict")]
    [ProducesResponseType(typeof(SpendingPredictionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SpendingPredictionDto>> PredictExpense([FromBody] PredictionRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state submitted to /api/prediction/predict");
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _predictionEngine.PredictExpenseAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during transaction expense prediction for User {UserId}", request.UserId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing transaction prediction." });
        }
    }

    /// <summary>
    /// Forecasts monthly spending for a specified user and target month.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="targetMonth">Optional target month (defaults to current UTC month).</param>
    /// <returns>SpendingPredictionDto containing monthly forecast and category breakdown.</returns>
    [HttpGet("forecast/{userId}")]
    [ProducesResponseType(typeof(SpendingPredictionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SpendingPredictionDto>> ForecastMonthlySpending(string userId, [FromQuery] DateTime? targetMonth)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new { message = "UserId parameter cannot be empty." });
        }

        try
        {
            DateTime target = targetMonth ?? DateTime.UtcNow;
            var forecast = await _predictionEngine.ForecastMonthlySpendingAsync(userId, target);
            return Ok(forecast);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred forecasting monthly spending for userId: {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while generating monthly forecast." });
        }
    }

    /// <summary>
    /// Triggers automated re-training of the ML.NET prediction model from CSV data.
    /// </summary>
    /// <param name="request">Optional training configuration specifying custom data/output paths.</param>
    /// <returns>ModelTrainingResultDto with RSquared, RMSE, MAE metrics.</returns>
    [HttpPost("train")]
    [ProducesResponseType(typeof(ModelTrainingResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ModelTrainingResultDto>> TrainModel([FromBody] ModelTrainingRequestDto? request)
    {
        try
        {
            string dataPath = request?.TrainingDataPath ?? string.Empty;
            string outputPath = request?.OutputModelPath ?? string.Empty;

            var result = await _predictionEngine.TrainModelAsync(dataPath, outputPath);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during ML model training execution.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while training ML model." });
        }
    }

    /// <summary>
    /// Predicts expected monthly spending for a specified user ID (integer).
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>SpendingPredictionDto containing prediction details, budget comparison, and early warning flags.</returns>
    [HttpGet("{userId:int}")]
    [ProducesResponseType(typeof(SpendingPredictionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SpendingPredictionDto>> GetMonthlyPrediction(int userId)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("Invalid userId parameter provided: {UserId}", userId);
            return BadRequest(new { message = "UserId must be a positive integer." });
        }

        try
        {
            var prediction = await _predictionEngine.PredictMonthlySpendingAsync(userId);
            return Ok(prediction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while generating spending prediction for userId: {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while generating spending prediction." });
        }
    }
}
