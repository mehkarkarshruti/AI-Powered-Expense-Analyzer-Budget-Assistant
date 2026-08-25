# Deploying SpendWise to Render

Three services live in this repository. Create each one in the Render
dashboard (New + → Web Service → connect this repo).

## Service 1 — Auth/Expenses/Budgets API  (REQUIRED for login)
- **Dockerfile path**: `./Dockerfile.authapi`
- **Environment variables**:
  | Key | Value |
  |---|---|
  | `DatabaseProvider` | `Sqlite` |
  | `ConnectionStrings__SqliteConnection` | `Data Source=/data/spendwise.db` |
- **Persistent Disk**: mount at `/data` (keeps users/expenses across deploys)

## Service 2 — Analytics/Prediction API
- **Dockerfile path**: `./Dockerfile`
- No extra settings (uses in-memory data)

## Service 3 — Web Frontend
- **Dockerfile path**: `./Dockerfile.web`
- **Environment variables**:
  | Key | Value |
  |---|---|
  | `ApiBaseUrl` | `https://<service-1-url>.onrender.com/api/` |

## Notes
- CORS on the APIs already allows any origin.
- Free-tier services sleep after ~15 min idle; first request takes ~60s.
- Local development stays unchanged: SQL Server LocalDB + localhost URLs.
