using ExpenseAnalyzer.Core.DTOs.Prediction;
using ExpenseAnalyzer.ML.Models;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;

namespace ExpenseAnalyzer.ML.Pipelines;

/// <summary>
/// Machine Learning Training Pipeline utilizing MLContext, feature engineering, FastTreeRegressor,
/// evaluation metric calculations (R2, RMSE, MAE), and model serialization into a .zip model artifact.
/// </summary>
public class ModelTrainingPipeline
{
    private readonly MLContext _mlContext;
    private readonly ILogger<ModelTrainingPipeline>? _logger;

    public ModelTrainingPipeline(ILogger<ModelTrainingPipeline>? logger = null, int seed = 42)
    {
        _mlContext = new MLContext(seed: seed);
        _logger = logger;
    }

    /// <summary>
    /// Executes the full ML.NET training, evaluation, and serialization pipeline.
    /// </summary>
    /// <param name="trainingDataPath">Path to CSV dataset (e.g., upi_data_enhanced.csv).</param>
    /// <param name="outputModelPath">Path where trained zip model will be serialized.</param>
    /// <returns>ModelTrainingResultDto containing RSquared, RMSE, MAE metrics and training status.</returns>
    public ModelTrainingResultDto TrainAndSaveModel(string trainingDataPath, string outputModelPath)
    {
        if (!File.Exists(trainingDataPath))
        {
            string errorMessage = $"Training dataset not found at path: '{trainingDataPath}'";
            _logger?.LogError(errorMessage);
            throw new FileNotFoundException(errorMessage);
        }

        _logger?.LogInformation("Loading training dataset from {Path}", trainingDataPath);

        // 1. Load dataset from CSV
        IDataView dataView = _mlContext.Data.LoadFromTextFile<TransactionData>(
            path: trainingDataPath,
            hasHeader: true,
            separatorChar: ',');

        // 2. Train / Test Split (80% Train, 20% Test)
        var splitData = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2, seed: 42);

        // 3. Feature Engineering Pipeline: Featurize text features, Concatenate numerical & text features, Normalize
        var featurePipeline = _mlContext.Transforms.Concatenate(
                "NumericalFeatures",
                nameof(TransactionData.DaysElapsed),
                nameof(TransactionData.DaysInMonth),
                nameof(TransactionData.HistoricalAverage),
                nameof(TransactionData.PrevMonthSpending),
                nameof(TransactionData.CurrentSpentSoFar),
                nameof(TransactionData.TransactionCountSoFar))
            .Append(_mlContext.Transforms.NormalizeMeanVariance("NormalizedFeatures", "NumericalFeatures"))
            .Append(_mlContext.Transforms.Concatenate("Features", "NormalizedFeatures"));

        // 4. FastTree Regressor Configuration
        var trainerOptions = new FastTreeRegressionTrainer.Options
        {
            NumberOfLeaves = 20,
            NumberOfTrees = 100,
            MinimumExampleCountPerLeaf = 2,
            LearningRate = 0.2f,
            FeatureColumnName = "Features",
            LabelColumnName = "Label"
        };

        var trainingPipeline = featurePipeline.Append(_mlContext.Regression.Trainers.FastTree(trainerOptions));

        _logger?.LogInformation("Fitting ML model with FastTreeRegressor pipeline...");

        // 5. Fit & Train Model
        ITransformer trainedModel = trainingPipeline.Fit(splitData.TrainSet);

        // 6. Evaluate Model Metrics
        IDataView predictions = trainedModel.Transform(splitData.TestSet);
        RegressionMetrics metrics = _mlContext.Regression.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");

        _logger?.LogInformation("Model Evaluation Completed - R2: {RSquared:F4}, RMSE: {RMSE:F2}, MAE: {MAE:F2}",
            metrics.RSquared, metrics.RootMeanSquaredError, metrics.MeanAbsoluteError);

        // 7. Ensure output directory exists & Save Model (.zip)
        string? directory = Path.GetDirectoryName(outputModelPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _mlContext.Model.Save(trainedModel, dataView.Schema, outputModelPath);
        _logger?.LogInformation("Trained model serialized and saved successfully to {OutputPath}", outputModelPath);

        return new ModelTrainingResultDto
        {
            Success = true,
            RSquared = Math.Round(metrics.RSquared, 4),
            RMSE = Math.Round(metrics.RootMeanSquaredError, 2),
            MAE = Math.Round(metrics.MeanAbsoluteError, 2),
            ModelPath = outputModelPath,
            Message = $"Model successfully trained and saved. R² = {metrics.RSquared:F4}, RMSE = {metrics.RootMeanSquaredError:F2}, MAE = {metrics.MeanAbsoluteError:F2}",
            TrainedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Loads a trained model zip file from disk.
    /// </summary>
    public ITransformer? LoadModel(string modelPath)
    {
        if (!File.Exists(modelPath))
        {
            _logger?.LogWarning("Model file not found at path: {Path}", modelPath);
            return null;
        }

        using var stream = File.OpenRead(modelPath);
        return _mlContext.Model.Load(stream, out _);
    }

    /// <summary>
    /// Creates a prediction engine for input inference.
    /// </summary>
    public PredictionEngine<TransactionData, TransactionPrediction> CreatePredictionEngine(ITransformer model)
    {
        return _mlContext.Model.CreatePredictionEngine<TransactionData, TransactionPrediction>(model);
    }
}
