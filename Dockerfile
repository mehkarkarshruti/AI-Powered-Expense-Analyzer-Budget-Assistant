# Base processing image for API runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy core sub-projects to permit dedicated RESTORE execution
COPY ["src/ExpenseAnalyzer.Core/ExpenseAnalyzer.Core.csproj", "src/ExpenseAnalyzer.Core/"]
COPY ["src/ExpenseAnalyzer.Infrastructure/ExpenseAnalyzer.Infrastructure.csproj", "src/ExpenseAnalyzer.Infrastructure/"]
COPY ["src/ExpenseAnalyzer.ML/ExpenseAnalyzer.ML.csproj", "src/ExpenseAnalyzer.ML/"]
COPY ["src/ExpenseAnalyzer.API/ExpenseAnalyzer.API.csproj", "src/ExpenseAnalyzer.API/"]
COPY ["ExpenseAnalyzer/ExpenseAnalyzer.API/ExpenseAnalyzer.API.csproj", "ExpenseAnalyzer/ExpenseAnalyzer.API/"]

# Restore dependencies precisely for the API
RUN dotnet restore "src/ExpenseAnalyzer.API/ExpenseAnalyzer.API.csproj"

# Copy full source and build
COPY . .
WORKDIR "/src/src/ExpenseAnalyzer.API"
RUN dotnet build "ExpenseAnalyzer.API.csproj" -c Release -o /app/build/api

# Publish API
FROM build AS publishapi
RUN dotnet publish "ExpenseAnalyzer.API.csproj" -c Release -o /app/publish/api

# Start API execution natively using explicitly mapped PORT arguments ensuring runtime evaluation
FROM base AS final
WORKDIR /app
COPY --from=publishapi /app/publish/api .
CMD ["sh", "-c", "dotnet ExpenseAnalyzer.API.dll --urls http://0.0.0.0:${PORT:-10000}"]
