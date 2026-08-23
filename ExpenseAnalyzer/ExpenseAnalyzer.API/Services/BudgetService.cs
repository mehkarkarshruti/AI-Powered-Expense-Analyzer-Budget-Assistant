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
                $"Critical: You have breached your {scope} budget of {budget.BudgetAmount:C}. Current spending: {currentSpending:C}");
        }
        else if (utilization >= 0.8m)
        {
            await alertService.ValidateAndCreateAlertAsync(userId, budget.BudgetId, AlertType.WARNING_80, 
                $"Warning: You have utilized {utilization:P} of your {scope} budget. Current spending: {currentSpending:C}");
        }

        if (!categoryId.HasValue)
        {
            var prediction = await FetchSpendingPredictionAsync(userId);
            if (prediction != null && (prediction.IsBudgetLikelyToBeExceeded || prediction.PredictedMonthlySpending > budget.BudgetAmount))
            {
                await alertService.ValidateAndCreateAlertAsync(userId, budget.BudgetId, AlertType.PREDICTIVE, 
                    $"Forecast Warning: Projected spending ({prediction.PredictedMonthlySpending:C}) exceeds your budget of {budget.BudgetAmount:C}.");
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
            budgetStatus.Prediction = await FetchSpendingPredictionAsync(userId);
            return budgetStatus;
        }

        budgetStatus.Budget = MapToResponse(budget);

        await CheckBudgetThresholdsAsync(userId, null, DateOnly.FromDateTime(now));

        var alerts = await alertService.GetUnreadAlertsAsync(userId);
        budgetStatus.ActiveAlerts.AddRange(alerts.Select(a => $"[{a.AlertType}] {a.Message}"));
        budgetStatus.Prediction = await FetchSpendingPredictionAsync(userId);

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

    private async Task<SpendingPredictionDto?> FetchSpendingPredictionAsync(int userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"Prediction/{userId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SpendingPredictionDto>();
            }
            logger.LogWarning("Failed to retrieve prediction for user {UserId}. Status Code: {StatusCode}", userId, response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HTTP Request to Prediction API failed.");
        }
        return null;
    }
}
