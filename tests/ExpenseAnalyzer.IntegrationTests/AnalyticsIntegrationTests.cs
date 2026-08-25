using System.Net;
using System.Net.Http.Json;
using ExpenseAnalyzer.Core.DTOs.Analytics;
using ExpenseAnalyzer.Core.Entities;
using ExpenseAnalyzer.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ExpenseAnalyzer.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public string DbName { get; } = "IntegrationDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            var configurationDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>));

            if (configurationDescriptor != null)
            {
                services.Remove(configurationDescriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(DbName);
            });
        });
    }
}

public class AnalyticsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AnalyticsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task SeedDataAsync(Action<AppDbContext> seedAction)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        seedAction(db);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetMonthlySummary_NoData_ReturnsZeroSummary()
    {
        // Act
        var response = await _client.GetAsync("/api/analytics/monthly?userId=99&month=2026-08");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var summary = await response.Content.ReadFromJsonAsync<MonthlyAnalyticsDto>();
        Assert.NotNull(summary);
        Assert.Equal("2026-08", summary.Month);
        Assert.Equal(0m, summary.TotalExpenses);
        Assert.Equal(0, summary.TotalTransactions);
        Assert.Equal(0m, summary.AverageExpense);
    }

    [Fact]
    public async Task GetMonthlySummary_ValidData_ReturnsCorrectValues()
    {
        // Arrange
        int userId = 10;
        string month = "2026-08";
        var date = new DateTime(2026, 8, 15);

        await SeedDataAsync(db =>
        {
            db.Expenses.Add(new Expense { ExpenseId = 101, UserId = userId, CategoryId = 1, Amount = 1500m, Date = date });
            db.Expenses.Add(new Expense { ExpenseId = 102, UserId = userId, CategoryId = 2, Amount = 500m, Date = date.AddDays(1) });
        });

        // Act
        var response = await _client.GetAsync($"/api/analytics/monthly?userId={userId}&month={month}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.Content.ReadFromJsonAsync<MonthlyAnalyticsDto>();
        Assert.NotNull(summary);
        Assert.Equal(month, summary.Month);
        Assert.Equal(2000m, summary.TotalExpenses);
        Assert.Equal(2, summary.TotalTransactions);
        Assert.Equal(1000m, summary.AverageExpense);
    }

    [Fact]
    public async Task GetCategorySummary_ValidData_ReturnsBreakdown()
    {
        // Arrange
        int userId = 20;
        string month = "2026-08";
        var date = new DateTime(2026, 8, 15);

        var catGroceries = new Category { CategoryId = 10, Name = "Groceries" };
        var catUtilities = new Category { CategoryId = 20, Name = "Utilities" };

        await SeedDataAsync(db =>
        {
            // Seed Categories if not tracked
            if (db.Categories.Find(10) == null) db.Categories.Add(catGroceries);
            if (db.Categories.Find(20) == null) db.Categories.Add(catUtilities);

            db.Expenses.Add(new Expense { ExpenseId = 201, UserId = userId, CategoryId = 10, Amount = 1500m, Date = date, Category = catGroceries });
            db.Expenses.Add(new Expense { ExpenseId = 202, UserId = userId, CategoryId = 20, Amount = 500m, Date = date, Category = catUtilities });
        });

        // Act
        var response = await _client.GetAsync($"/api/analytics/category?userId={userId}&month={month}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var categories = await response.Content.ReadFromJsonAsync<List<CategorySpendDto>>();
        Assert.NotNull(categories);
        Assert.Equal(2, categories.Count);

        // Sorting by amount desc check (Groceries should be first)
        Assert.Equal("Groceries", categories[0].CategoryName);
        Assert.Equal(1500m, categories[0].TotalAmount);
        Assert.Equal(75m, categories[0].Percentage);

        Assert.Equal("Utilities", categories[1].CategoryName);
        Assert.Equal(500m, categories[1].TotalAmount);
        Assert.Equal(25m, categories[1].Percentage);
    }

    [Fact]
    public async Task GetMonthlySummary_InvalidUserId_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/analytics/monthly?userId=0&month=2026-08");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
