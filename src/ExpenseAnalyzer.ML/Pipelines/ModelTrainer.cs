using ExpenseAnalyzer.ML.Models;
using Microsoft.ML;

namespace ExpenseAnalyzer.ML.Pipelines;

public class ModelMetricsResult
{
    public double MAE { get; set; }
    public double RMSE { get; set; }
    public double RSquared { get; set; }
}

public class ModelTrainer
{
    private readonly ModelTrainingPipeline _pipeline;
    private readonly string _modelPath;
    private readonly string _dataPath;

    public ModelTrainer(string? dataPath = null, string? modelPath = null)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string defaultDataPath = Path.Combine(baseDir, "Data", "upi_data_enhanced.csv");
        if (!File.Exists(defaultDataPath))
        {
            string devPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "ExpenseAnalyzer.ML", "Data", "upi_data_enhanced.csv");
            if (File.Exists(devPath))
            {
                defaultDataPath = devPath;
            }
        }

        string defaultModelPath = Path.Combine(baseDir, "Models", "spending-model.zip");
        _dataPath = dataPath ?? defaultDataPath;
        _modelPath = modelPath ?? defaultModelPath;
        _pipeline = new ModelTrainingPipeline();
    }

    public (ITransformer Model, ModelMetricsResult Metrics) TrainAndSaveModel()
    {
        var result = _pipeline.TrainAndSaveModel(_dataPath, _modelPath);
        var loadedModel = _pipeline.LoadModel(_modelPath) ?? throw new InvalidOperationException("Failed to load trained model.");
        
        var metrics = new ModelMetricsResult
        {
            MAE = result.MAE,
            RMSE = result.RMSE,
            RSquared = result.RSquared
        };

        return (loadedModel, metrics);
    }

    public ITransformer? LoadModel()
    {
        return _pipeline.LoadModel(_modelPath);
    }

    public PredictionEngine<TransactionData, TransactionPrediction> CreatePredictionEngine(ITransformer model)
    {
        return _pipeline.CreatePredictionEngine(model);
    }

    public PredictionEngine<SpendingModelInput, SpendingModelOutput> CreateLegacyPredictionEngine(ITransformer model)
    {
        var mlContext = new MLContext(seed: 42);
        return mlContext.Model.CreatePredictionEngine<SpendingModelInput, SpendingModelOutput>(model);
    }
}
