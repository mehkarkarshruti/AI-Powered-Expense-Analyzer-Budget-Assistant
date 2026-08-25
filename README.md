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

## Infrastructure & Deployment Features

- **Stateless JWT Authentication**: Secure user management decoupled from sessions preventing IDOR vulnerabilities natively.
- **RESTful API backend**: Fully tested REST API for Expenses, Budgets, and Analytics ensuring Controller encapsulation.
- **SQLite Persistence**: Ready for cloud deployment on volatile clusters if attached to Persistent Disks natively mapping to `/data/ExpenseAnalyzer.db`.
- **Database Initialization**: The application schema initializes automatically using Entity Framework Core's `EnsureCreated()`. No manual migration commands (`Migrate()`) are necessary.

## Local Setup

### Prerequisites
- .NET 8.0 SDK installed

### Run without Docker
1. Navigate to the API Folder:
```bash
cd src/ExpenseAnalyzer.API
dotnet run
```
2. In a new terminal, run the application Web Frontend:
```bash
cd ExpenseAnalyzer.Web
dotnet run
```

## Deployment via Render

Because of the architectural separation between the ASP.NET Core MVC Engine (`ExpenseAnalyzer.Web`) and the domain REST engine (`ExpenseAnalyzer.API`), provisioning two independent Web Services guarantees seamless execution. 

### Web Service 1: Backend API (ExpenseAnalyzer.API)
1. **Source**: Fork and select this repository in Render Dashboard.
2. **Type**: Web Service (Native .NET 8.0 Environment).
3. **Build Command**: `dotnet publish src/ExpenseAnalyzer.API/ExpenseAnalyzer.API.csproj -c Release -o ./publish`
4. **Start Command**: `dotnet ./publish/ExpenseAnalyzer.API.dll`
5. **Environment Variables**:
   * `Jwt__Secret`: Your 256-bit strong deterministic hash sequence.
   * `ConnectionStrings__DefaultConnection`: `Data Source=/data/ExpenseAnalyzer.db`
   * `FrontendUrl`: Base URL of the deployed MVC app (e.g. `https://my-expense-web.onrender.com`).
6. **Disk**: Provision a Render **Persistent Disk** anchored to `/data`.

### Web Service 2: Frontend App (ExpenseAnalyzer.Web)
1. **Source**: Select this repository in Render Dashboard for a second Web Service.
2. **Type**: Web Service (Native .NET 8.0 Environment).
3. **Build Command**: `dotnet publish ExpenseAnalyzer.Web/ExpenseAnalyzer.Web.csproj -c Release -o ./publish`
4. **Start Command**: `dotnet ./publish/ExpenseAnalyzer.Web.dll`
5. **Environment Variables**:
   * `ApiSettings__BaseUrl`: URL of the backend API Service you just created.

---

## ML / Spending Prediction Module

The Spending Prediction module uses a user's historical expense data to forecast their expected spending for the current month. It calculates a projected monthly total, compares it against the user's defined monthly budget (if available), and provides early warnings when spending is likely to exceed budget limits.
