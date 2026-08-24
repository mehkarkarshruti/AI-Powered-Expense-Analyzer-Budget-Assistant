# AI-Powered Expense Analyzer & Budget Assistant

An AI-powered application to track expenses, analyze spending patterns, and assist with budget management.

## Group- 13

| Application No. | Name |
|---|---|
| IN26011653 | Gitanshi Singh |
| IN26011650 | Samiksha Tiwari |
| IN26009878 | Shruti Mehkarkar |
| IN26011621 | Viya Sharma |
| IN26010694 | Abhiudaya Pratap Singh |
| IN26010937 | Devbrat Yadav |
| IN26010700 | Priyam Rai |

---

## ML / Spending Prediction Module

### Overview
The Spending Prediction module uses a user's historical expense data to forecast their expected spending for the current month. It calculates a projected monthly total, compares it against the user's defined monthly budget (if available), and provides early warnings when spending is likely to exceed budget limits.

### Architecture & Components
- **Core Interfaces & DTOs**: `SpendingPredictionDto.cs`, `IPredictionEngine.cs` inside `ExpenseAnalyzer.Core`.
- **ML Engine & Pipeline**: `ModelTrainer.cs`, `SpendingModelInput.cs`, `SpendingModelOutput.cs`, `PredictionService.cs` inside `ExpenseAnalyzer.ML`.
- **REST Controller**: `PredictionController.cs` inside `ExpenseAnalyzer.API`.

---

### Machine Learning Details
- **Input Data**: Historical transaction logs and monthly summary features (`DaysElapsed`, `DaysInMonth`, `HistoricalAverage`, `PrevMonthSpending`, `CurrentSpentSoFar`, `TransactionCountSoFar`).
- **Preprocessing & Feature Engineering**: Numerical feature concatenation, days-in-month normalization, monthly velocity feature extraction.
- **ML Algorithm**: ML.NET `FastTreeRegression` / `SdcaRegression` model trained on historical monthly expense patterns (`upi_data_enhanced.csv`).
- **Evaluation Metrics**:
  - **MAE (Mean Absolute Error)**: Average absolute difference between predicted and actual total monthly spending.
  - **RMSE (Root Mean Squared Error)**: Measures error variance, penalizing larger prediction errors.
  - **R² (Coefficient of Determination)**: Indicates how well historical spending trends explain variance in future spending.

---

### Fallback Strategy & Edge Cases
When historical data is sparse or ML model predictions are unavailable, the engine applies a robust heuristic fallback:
$$\text{Predicted Spending} = \max\left(\frac{\text{Current Spent So Far}}{\text{Days Elapsed}} \times \text{Days In Month},\, \text{Historical Monthly Average}\right)$$
- **No Expenses**: Returns `InsufficientData` status, `0.0` confidence score, and informative message.
- **Sparse Transactions (< 3 records)**: Utilizes velocity extrapolation, lowers confidence score to ~0.45, and flags `IsFallback = true`.
- **No Budget Set**: Computes prediction accurately while marking `MonthlyBudget = null` and `PredictionStatus = "NoBudgetSet"`.

---

### API Endpoint & Conceptual Response

#### Endpoint
`GET /api/prediction/{userId}`

#### Example Request
`GET /api/prediction/1`

#### Example Response (JSON)
```json
{
  "userId": 1,
  "currentMonth": "2026-08",
  "historicalAverage": 14500.0,
  "currentMonthSpending": 9200.0,
  "predictedMonthlySpending": 15800.0,
  "monthlyBudget": 15000.0,
  "remainingBudget": -800.0,
  "predictionStatus": "LikelyToExceed",
  "confidenceScore": 0.85,
  "isBudgetLikelyToBeExceeded": true,
  "message": "Warning: Your predicted monthly spending ($15,800.00) is projected to exceed your budget ($15,000.00).",
  "isFallback": false
}
```

---

### Limitations
- Predictions become significantly more accurate after a user records at least 2-3 months of consistent transaction history.
- Sudden, non-recurring high-value expenses (e.g. medical emergencies) in a single month may temporarily elevate projected spending until more days elapse in the month.

---

## Sequence Diagrams

Documentation available in:

* SEQUENCE_DIAGRAMS.md

Covered Flows:

* Add Expense
* Budget Warning (80% / 100%)
* Spending Prediction

