# Base processing image for API runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["ExpenseAnalyzer.sln", "./"]
COPY ["src/ExpenseAnalyzer.Core/ExpenseAnalyzer.Core.csproj", "src/ExpenseAnalyzer.Core/"]
COPY ["src/ExpenseAnalyzer.Infrastructure/ExpenseAnalyzer.Infrastructure.csproj", "src/ExpenseAnalyzer.Infrastructure/"]
COPY ["src/ExpenseAnalyzer.ML/ExpenseAnalyzer.ML.csproj", "src/ExpenseAnalyzer.ML/"]
COPY ["src/ExpenseAnalyzer.API/ExpenseAnalyzer.API.csproj", "src/ExpenseAnalyzer.API/"]
# We also copy Web to allow solution restore, even if we don't build it here
COPY ["ExpenseAnalyzer.Web/ExpenseAnalyzer.Web.csproj", "ExpenseAnalyzer.Web/"]

# Restore dependencies
RUN dotnet restore "ExpenseAnalyzer.sln"

# Copy full source and build
COPY . .
WORKDIR "/src/src/ExpenseAnalyzer.API"
RUN dotnet build "ExpenseAnalyzer.API.csproj" -c Release -o /app/build/api

# Publish API
FROM build AS publishapi
RUN dotnet publish "ExpenseAnalyzer.API.csproj" -c Release -o /app/publish/api

# Start API execution natively
FROM base AS final
WORKDIR /app
COPY --from=publishapi /app/publish/api .
ENTRYPOINT ["dotnet", "ExpenseAnalyzer.API.dll"]
