using ExpenseAnalyzer.API.DTOs;
using ExpenseAnalyzer.API.Models;
using ExpenseAnalyzer.API.Repositories;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace ExpenseAnalyzer.API.Services;

public class BudgetService(
    IBudgetRepository budgetRepo,
    IExpenseRepository expenseRepo,
    IAlertService alertService,
    IHttpClientFactory httpClientFactory,
    ILogger<BudgetService> logger) : IBudgetService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("PredictionAPI");

    public async Task<BudgetResponse> CreateOrUpdateBudgetAsync(int userId, CreateBudgetRequest request)
    {
        var existing = await budgetRepo.GetBudgetAsync(userId, request.CategoryId, request.Month, request.Year);
        if (existing != null)
        {
            existing.BudgetAmount = request.BudgetAmount;
            existing.UpdatedAt = DateTime.UtcNow;
            await budgetRepo.UpdateAsync(existing);
            return MapToResponse(existing);
        }

        var newBudget = new Budget
        {
            UserId = userId,
            CategoryId = request.CategoryId,
            Month = request.Month,
            Year = request.Year,
            BudgetAmount = request.BudgetAmount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await budgetRepo.AddAsync(newBudget);
        return MapToResponse(created);
    }

    public async Task<BudgetResponse?> GetBudgetAsync(int userId, int? categoryId, byte month, short year)
    {
        var budget = await budgetRepo.GetBudgetAsync(userId, categoryId, month, year);
        if (budget == null) return null;
        return MapToResponse(budget);
    }

    public async Task<IEnumerable<BudgetResponse>> GetAllBudgetsAsync(int userId)
    {
        var budgets = await budgetRepo.GetAllByUserIdAsync(userId);
        return budgets.Select(MapToResponse);
    }

    public async Task DeleteBudgetAsync(int budgetId, int userId)
    {
        var budget = await budgetRepo.GetByIdAsync(budgetId, userId);
        if (budget != null)
        {
            await budgetRepo.DeleteAsync(budget);
        }
    }

    public async Task CheckBudgetThresholdsAsync(int userId, int? categoryId, DateOnly expenseDate)
    {
        byte month = (byte)expenseDate.Month;
        short year = (short)expenseDate.Year;

        var budget = await budgetRepo.GetBudgetAsync(userId, categoryId, month, year);
        if (budget == null) return;

        var currentSpending = await expenseRepo.GetTotalSpentAsync(userId, categoryId, month, year);
        decimal utilization = budget.BudgetAmount > 0 ? (currentSpending / budget.BudgetAmount) : 0;

        string scope = categoryId.HasValue ? "category" : "monthly overall";

        if (utilization >= 1.0m)
        {
            await alertService.ValidateAndCreateAlertAsync(userId, budget.BudgetId, AlertType.EXCEEDED_100, 
                $"Critical: You have breached your {scope} budget of \u20B9{budget.BudgetAmount:N2}. Current spending: \u20B9{currentSpending:N2}");
        }
        else if (utilization >= 0.8m)
        {
            await alertService.ValidateAndCreateAlertAsync(userId, budget.BudgetId, AlertType.WARNING_80, 
                $"Warning: You have utilized {utilization:P2} of your {scope} budget. Current spending: \u20B9{currentSpending:N2}");
        }

        if (!categoryId.HasValue)
        {
            var prediction = await FetchSpendingPredictionAsync(userId);
            if (prediction != null && (prediction.IsBudgetLikelyToBeExceeded || prediction.PredictedMonthlySpending > budget.BudgetAmount))
            {
                await alertService.ValidateAndCreateAlertAsync(userId, budget.BudgetId, AlertType.PREDICTIVE, 
                    $"Forecast Warning: Projected spending (\u20B9{prediction.PredictedMonthlySpending:N2}) exceeds your budget of \u20B9{budget.BudgetAmount:N2}.");
            }
        }
    }

    public async Task<BudgetStatusDto> GetBudgetStatusAndCheckAlertsAsync(int userId)
    {
        var now = DateTime.UtcNow;
        byte month = (byte)now.Month;
        short year = (short)now.Year;

        var budgetStatus = new BudgetStatusDto();
        var budget = await budgetRepo.GetBudgetAsync(userId, null, month, year);
        
        if (budget == null)
        {
            budgetStatus.ActiveAlerts.Add("No overall budget set for the current month.");
            budgetStatus.Prediction = await FetchSpendingPredictionAsync(userId, null);
            return budgetStatus;
        }

        budgetStatus.Budget = MapToResponse(budget);

        await CheckBudgetThresholdsAsync(userId, null, DateOnly.FromDateTime(now));

        // Live evaluation: alerts are recomputed from CURRENT spending and
        // budget on every request, so they appear/disappear immediately
        // when expenses or budgets change.
        var spentThisMonth = await expenseRepo.GetTotalSpentAsync(
            userId, null, now.Month, now.Year);

        if (budget.BudgetAmount > 0)
        {
            var utilizationPct = spentThisMonth / budget.BudgetAmount * 100;

            if (utilizationPct >= 100)
            {
                budgetStatus.ActiveAlerts.Add(
                    $"Critical: You have exceeded your monthly budget. Spent \u20B9{spentThisMonth:N2} of \u20B9{budget.BudgetAmount:N2}.");
            }
            else if (utilizationPct >= 80)
            {
                budgetStatus.ActiveAlerts.Add(
                    $"Warning: You have utilized {utilizationPct:P2} of your monthly budget. Remaining: \u20B9{budget.BudgetAmount - spentThisMonth:N2}.");
            }
        }

        budgetStatus.Prediction = await FetchSpendingPredictionAsync(userId, budget);

        return budgetStatus;
    }

    private BudgetResponse MapToResponse(Budget budget)
    {
        return new BudgetResponse
        {
            BudgetId = budget.BudgetId,
            UserId = budget.UserId,
            CategoryId = budget.CategoryId,
            Month = budget.Month,
            Year = budget.Year,
            BudgetAmount = budget.BudgetAmount,
            CreatedAt = budget.CreatedAt,
            UpdatedAt = budget.UpdatedAt
        };
    }

    private async Task<SpendingPredictionDto?> FetchSpendingPredictionAsync(int userId, Budget? budget = null)
    {
        SpendingPredictionDto? remote = null;

        try
        {
            var response = await _httpClient.GetAsync($"Prediction/{userId}");

            if (response.IsSuccessStatusCode)
            {
                remote = await response.Content.ReadFromJsonAsync<SpendingPredictionDto>();
            }
            else
            {
                logger.LogWarning("Prediction API returned {StatusCode} for user {UserId}. Using local estimate.",
                    response.StatusCode, userId);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Prediction API unreachable for user {UserId}. Using local estimate.", userId);
        }

        return remote ?? await BuildLocalEstimateAsync(userId, budget);
    }

    // Velocity-based projection computed from the user's REAL expenses:
    // (spent so far this month / days elapsed) x days in month.
    private async Task<SpendingPredictionDto> BuildLocalEstimateAsync(int userId, Budget? budget)
    {
        var now = DateTime.UtcNow;

        var spentSoFar = await expenseRepo.GetTotalSpentAsync(userId, null, now.Month, now.Year);

        var daysElapsed = now.Day;
        var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);

        var projected = daysElapsed > 0
            ? Math.Round(spentSoFar / daysElapsed * daysInMonth, 2)
            : 0m;

        var monthlyBudget = budget?.BudgetAmount ?? 0m;

        return new SpendingPredictionDto
        {
            PredictedMonthlySpending = projected,
            MonthlyBudget = monthlyBudget > 0 ? monthlyBudget : null,
            IsBudgetLikelyToBeExceeded = monthlyBudget > 0 && projected > monthlyBudget,
            Message = spentSoFar > 0
                ? $"Velocity-based estimate: \u20B9{spentSoFar:N2} spent in {daysElapsed} day(s), projected to \u20B9{projected:N2} by month end."
                : "Add at least one expense this month to see a forecast."
        };
    }
}
