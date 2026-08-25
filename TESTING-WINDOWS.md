# SpendWise — How to Test on Windows

## Prerequisites
- Windows 10/11
- .NET 10 SDK → https://dotnet.microsoft.com/download/dotnet/10.0 (run `dotnet --version` to confirm)
- SQL Server **LocalDB** — already installed with Visual Studio 2022. If you don't have VS:
  `winget install Microsoft.SqlServer.LocalDB`

Nothing else. No Docker, no manual database setup.

---

## 1. Run the API (auth + users + expenses + categories + budgets + alerts)

```
cd ExpenseAnalyzer\ExpenseAnalyzer.API
dotnet run
```

On startup it will automatically:
- apply any pending EF Core migrations,
- seed 8 default categories.

- Swagger UI: https://localhost:<port>/swagger (port is printed in the console)
- Database used: `(localdb)\MSSQLLocalDB` / `ExpenseAnalyzerDb` (see appsettings.json)

## 2. Create a user & get a token (Swagger)

**POST** `/api/auth/register`
```json
{ "name": "Test User", "email": "test@spendwise.com", "password": "Password123!" }
```

**POST** `/api/auth/login`
```json
{ "email": "test@spendwise.com", "password": "Password123!" }
```
Copy the `token` from the response.

Click **Authorize** in Swagger and paste: `Bearer <your token>` (the word Bearer, a space, then the token).

## 3. Exercise the Expenses / Categories endpoints

| Method | Route | Notes |
|---|---|---|
| GET | `/api/categories` | public with any valid token; 8 seeded categories |
| GET/POST/PUT/DELETE | `/api/expenses` | requires Bearer token; amount must be > 0 |
| POST/DELETE | `/api/categories` | admin-only (see known issue below) |

Example create expense:
```json
{
  "categoryId": 1,
  "amount": 249.99,
  "expenseDate": "2026-08-25",
  "description": "Groceries"
}
```

## Known issues (expected behaviour right now)
1. **Admin category create/delete returns 403 for everyone** — the JWT does not contain a role claim yet.
   Fix pending in `JwtService.GenerateToken`: add `new Claim(ClaimTypes.Role, user.Role)`.
2. The Web frontend currently in this branch is an older build; a newer UI mock circulates separately.
3. `.vs/` folders are committed in some places — ignore them; they will be purged later.
