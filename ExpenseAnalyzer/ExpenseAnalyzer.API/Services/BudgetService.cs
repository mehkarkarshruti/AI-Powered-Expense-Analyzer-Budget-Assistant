using ExpenseAnalyzer.API.DTOs;
using ExpenseAnalyzer.API.Models;
using ExpenseAnalyzer.API.Repositories;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace ExpenseAnalyzer.API.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly IBudgetRepository _budgetRepo;
        private readonly IExpenseRepository _expenseRepo;
        private readonly IAlertRepository _alertRepo;
        private readonly HttpClient _httpClient;
        private readonly ILogger<BudgetService> _logger;

        public BudgetService(
            IBudgetRepository budgetRepo,
            IExpenseRepository expenseRepo,
            IAlertRepository alertRepo,
            IHttpClientFactory httpClientFactory,
            ILogger<BudgetService> logger)
        {
            _budgetRepo = budgetRepo;
            _expenseRepo = expenseRepo;
            _alertRepo = alertRepo;
            _httpClient = httpClientFactory.CreateClient("PredictionAPI");
            _logger = logger;
        }

        public async Task<BudgetResponseDto> SetBudgetAsync(int userId, SetBudgetDto dto)
        {
            var existing = await _budgetRepo.GetByUserAndMonthAsync(userId, dto.Month, dto.Year);
            if (existing != null)
            {
                existing.BudgetAmount = dto.BudgetAmount;
                await _budgetRepo.UpdateAsync(existing);
                return await GetBudgetAsync(userId, dto.Month, dto.Year) ?? throw new Exception("Failed to retrieve budget.");
            }

            var newBudget = new Budget
            {
                UserId = userId,
                Month = dto.Month,
                Year = dto.Year,
                BudgetAmount = dto.BudgetAmount
            };

            await _budgetRepo.AddAsync(newBudget);
            return await GetBudgetAsync(userId, dto.Month, dto.Year) ?? throw new Exception("Failed to retrieve budget.");
        }

        public async Task<BudgetResponseDto?> GetBudgetAsync(int userId, byte month, short year)
        {
            var budget = await _budgetRepo.GetByUserAndMonthAsync(userId, month, year);
            if (budget == null) return null;

            var currentSpending = await _expenseRepo.GetTotalSpentAsync(userId, month, year);
            return new BudgetResponseDto
            {
                BudgetId = budget.BudgetId,
                Month = budget.Month,
                Year = budget.Year,
                BudgetAmount = budget.BudgetAmount,
                CurrentSpending = currentSpending,
                RemainingBudget = budget.BudgetAmount - currentSpending
            };
        }

        public async Task<BudgetStatusDto> GetBudgetStatusAndCheckAlertsAsync(int userId)
        {
            var now = DateTime.UtcNow;
            byte month = (byte)now.Month;
            short year = (short)now.Year;

            var budgetStatus = new BudgetStatusDto();
            var budget = await _budgetRepo.GetByUserAndMonthAsync(userId, month, year);
            
            // Case A - No Budget: Return state indicating "No Budget" but we can still fetch prediction
            if (budget == null)
            {
                budgetStatus.ActiveAlerts.Add("No budget set for the current month.");
                budgetStatus.Prediction = await FetchSpendingPredictionAsync(userId);
                return budgetStatus;
            }

            var currentSpending = await _expenseRepo.GetTotalSpentAsync(userId, month, year);
            budgetStatus.Budget = new BudgetResponseDto
            {
                BudgetId = budget.BudgetId,
                Month = budget.Month,
                Year = budget.Year,
                BudgetAmount = budget.BudgetAmount,
                CurrentSpending = currentSpending,
                RemainingBudget = budget.BudgetAmount - currentSpending
            };

            // Threshold Calculation
            decimal utilization = budget.BudgetAmount > 0 ? (currentSpending / budget.BudgetAmount) : 0;
            
            if (utilization >= 1.0m) // Case D - Spending Reaches/Exceeds 100%
            {
                await ValidateAndCreateAlertAsync(userId, budget.BudgetId, AlertType.EXCEEDED_100, $"Critical: You have exceeded your monthly budget of {budget.BudgetAmount:C}. Current spending: {currentSpending:C}");
                budgetStatus.ActiveAlerts.Add("Exceeded 100% of Monthly Budget");
            }
            else if (utilization >= 0.8m) // Case C - Spending Reaches 80% (but < 100%)
            {
                await ValidateAndCreateAlertAsync(userId, budget.BudgetId, AlertType.WARNING_80, $"Warning: You have utilized {utilization:P} of your monthly budget. Current spending: {currentSpending:C}");
                budgetStatus.ActiveAlerts.Add("Approaching 80% of Monthly Budget");
            }
            // Case B - Spending Below 80%: No threshold warning generated.

            // Case E: Predictive Warning Workflow
            var prediction = await FetchSpendingPredictionAsync(userId);
            if (prediction != null)
            {
                budgetStatus.Prediction = prediction;
                if (prediction.IsBudgetLikelyToBeExceeded || prediction.PredictedMonthlySpending > budget.BudgetAmount)
                {
                    await ValidateAndCreateAlertAsync(userId, budget.BudgetId, AlertType.PREDICTIVE, $"Forecast Warning: Projected spending ({prediction.PredictedMonthlySpending:C}) exceeds your budget of {budget.BudgetAmount:C}.");
                    budgetStatus.ActiveAlerts.Add("Predictive Warning: Budget likely to be exceeded");
                }
            }

            return budgetStatus;
        }

        private async Task ValidateAndCreateAlertAsync(int userId, int budgetId, AlertType type, string message)
        {
            // Do not create duplicated unstructured alerts on same day to avoid spamming
            var existingAlerts = await _alertRepo.GetAllByUserIdAsync(userId);
            bool alertExists = existingAlerts.Any(a => a.AlertType == type && a.BudgetId == budgetId && a.CreatedAt.Date == DateTime.UtcNow.Date);
            
            if (!alertExists)
            {
                await _alertRepo.AddAsync(new Alert
                {
                    UserId = userId,
                    BudgetId = budgetId,
                    AlertType = type,
                    Message = message
                });
            }
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
                _logger.LogWarning($"Failed to retrieve prediction for user {userId}. Status Code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HTTP Request to Prediction API failed.");
            }
            return null;
        }
    }
}
