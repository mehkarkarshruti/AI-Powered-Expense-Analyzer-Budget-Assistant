using ExpenseAnalyzer.Core.Entities;
using ExpenseAnalyzer.Core.Interfaces;
using ExpenseAnalyzer.Infrastructure.Data;
using ExpenseAnalyzer.ML;
using ExpenseAnalyzer.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var frontendUrl = builder.Configuration["FrontendUrl"];
        if (!string.IsNullOrEmpty(frontendUrl))
        {
            policy.WithOrigins(frontendUrl)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            // Fallback for Development if FrontendUrl is not specified
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register SQLite DbContext (Database persistence for Render/production environment)
var dbConfigPath = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(dbConfigPath))
{
    var defaultPath = Path.Combine(Directory.GetCurrentDirectory(), "ExpenseAnalyzer.db");
    dbConfigPath = $"Data Source={defaultPath}";
}
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(dbConfigPath));

// Dependency Injection Registration for Devbrat's Machine Learning & Prediction Module
builder.Services.AddScoped<IPredictionEngine, PredictionService>();

// Dependency Injection Registration for Analytics Module
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Dependency Injection for Auth

var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Secret"] ?? "SuperSecretKeyEnsureMinimumOfThirtyTwoBytesForHS256!!!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "ExpenseAnalyzer.API",
        ValidAudience = jwtSettings["Audience"] ?? "ExpenseAnalyzer.Web",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

var app = builder.Build();

// Seed initial database records for verification
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (app.Environment.IsDevelopment())
    {
        if (!db.Users.Any())
        {
            db.Users.Add(new User { UserId = 1, Name = "Devbrat", Email = "devbrat@example.com" });
            db.Users.Add(new User { UserId = 2, Name = "Alice", Email = "alice@example.com" });

            db.Categories.Add(new Category { CategoryId = 1, Name = "Rent", IsActive = true });
            db.Categories.Add(new Category { CategoryId = 2, Name = "Groceries", IsActive = true });
            db.Categories.Add(new Category { CategoryId = 3, Name = "Utilities", IsActive = true });

            string currentMonthStr = DateTime.UtcNow.ToString("yyyy-MM");
            db.Budgets.Add(new Budget { BudgetId = 1, UserId = 1, Month = currentMonthStr, Amount = 15000m });
            db.Budgets.Add(new Budget { BudgetId = 2, UserId = 2, Month = currentMonthStr, Amount = 20000m });

            // User 1 Historical & Current Month Expenses
            DateTime now = DateTime.UtcNow;
            db.Expenses.Add(new Expense { ExpenseId = 1, UserId = 1, CategoryId = 1, Amount = 5000m, Date = now.AddDays(-5), Description = "House Rent" });
            db.Expenses.Add(new Expense { ExpenseId = 2, UserId = 1, CategoryId = 2, Amount = 2500m, Date = now.AddDays(-2), Description = "Groceries" });
            db.Expenses.Add(new Expense { ExpenseId = 3, UserId = 1, CategoryId = 3, Amount = 1200m, Date = now.AddDays(-1), Description = "Electricity Bill" });

            // Historical expenses for previous months
            DateTime prevMonth1 = now.AddMonths(-1);
            db.Expenses.Add(new Expense { ExpenseId = 4, UserId = 1, CategoryId = 1, Amount = 5000m, Date = prevMonth1.AddDays(-15), Description = "House Rent" });
            db.Expenses.Add(new Expense { ExpenseId = 5, UserId = 1, CategoryId = 2, Amount = 4500m, Date = prevMonth1.AddDays(-10), Description = "Supermarket" });
            db.Expenses.Add(new Expense { ExpenseId = 6, UserId = 1, CategoryId = 3, Amount = 4700m, Date = prevMonth1.AddDays(-5), Description = "Gas & Utilities" });

            DateTime prevMonth2 = now.AddMonths(-2);
            db.Expenses.Add(new Expense { ExpenseId = 7, UserId = 1, CategoryId = 1, Amount = 5000m, Date = prevMonth2.AddDays(-15), Description = "House Rent" });
            db.Expenses.Add(new Expense { ExpenseId = 8, UserId = 1, CategoryId = 2, Amount = 4300m, Date = prevMonth2.AddDays(-10), Description = "Groceries" });
            db.Expenses.Add(new Expense { ExpenseId = 9, UserId = 1, CategoryId = 3, Amount = 4500m, Date = prevMonth2.AddDays(-5), Description = "Utilities" });

            db.SaveChanges();
        }
    }
}

// Configure the HTTP request pipeline.
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Partial Program class for WebApplicationFactory integration testing
public partial class Program { }
