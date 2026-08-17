# AI-Powered Expense Analyzer & Budget Assistant
## UML Sequence Diagrams

This document contains professional enterprise-level UML sequence diagrams representing the core flows of the system.

### Diagram 1: Add Expense Flow

**Scenario:** The user has valid expense details (amount, category, date) and submits a new expense. The system stores the record, updates analytics, and refreshes the UI.

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant Frontend as Frontend UI
    participant ExpenseAPI as Expense API
    participant DB as Database
    participant Analytics as Analytics Engine

    User->>Frontend: Enters expense details (amount, category, date)
    activate Frontend
    Frontend->>ExpenseAPI: POST /api/expenses (ExpenseData)
    activate ExpenseAPI
    
    ExpenseAPI->>ExpenseAPI: Validate request data
    
    alt Validation Failed (Invalid Data)
        ExpenseAPI-->>Frontend: 400 Bad Request (Error details)
        Frontend-->>User: Displays validation error
    else Validation Successful
        ExpenseAPI->>DB: Save new expense record
        activate DB
        DB-->>ExpenseAPI: Confirms successful save
        deactivate DB
        
        ExpenseAPI->>Analytics: Notify Analytics Engine
        activate Analytics
        Analytics->>Analytics: Recalculate spending statistics
        Analytics-->>ExpenseAPI: Updated analytics metrics
        deactivate Analytics
        
        ExpenseAPI-->>Frontend: 201 Created (Expense & Updated Analytics)
        deactivate ExpenseAPI
        
        Frontend->>Frontend: Refresh dashboard in real-time
        Frontend-->>User: Displays success confirmation
    end
    deactivate Frontend
```

---

### Diagram 2: Budget Warning (80% / 100% Threshold)

**Scenario:** Predefined budget limits exist, and a new expense is added. The system calculates the budget utilization percentage and conditionally triggers advisory or critical alerts via the Notification Service.

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant Frontend as Frontend UI
    participant ExpenseAPI as Expense API
    participant BudgetService as Budget Service
    participant DB as Database
    participant Notification as Notification Service

    User->>Frontend: Submits expense
    activate Frontend
    Frontend->>ExpenseAPI: POST /api/expenses (ExpenseData)
    activate ExpenseAPI
    
    ExpenseAPI->>DB: Store expense record
    activate DB
    DB-->>ExpenseAPI: Confirm save
    deactivate DB
    
    ExpenseAPI->>BudgetService: Request updated spending total
    activate BudgetService
    
    BudgetService->>DB: Retrieve budget definitions & current spending
    activate DB
    DB-->>BudgetService: Budget data & total spent
    deactivate DB
    
    BudgetService->>BudgetService: Calculate budget utilization percentage
    
    alt Condition B: Spending >= 100%
        BudgetService->>Notification: Generate critical alert
        activate Notification
        Notification->>Notification: Send critical budget exceeded notification
        Notification-->>BudgetService: Alert sent confirmation
        deactivate Notification
        BudgetService-->>ExpenseAPI: Budget Status (Critical Exceeded)
        
    else Condition A: Spending >= 80% and < 100%
        BudgetService->>Notification: Generate warning alert
        activate Notification
        Notification->>Notification: Send advisory notification
        Notification-->>BudgetService: Alert sent confirmation
        deactivate Notification
        BudgetService-->>ExpenseAPI: Budget Status (Warning Threshold)
        
    else Condition C: Spending < 80%
        BudgetService-->>ExpenseAPI: Budget Status (Normal, No alert generated)
    end
    deactivate BudgetService
    
    ExpenseAPI-->>Frontend: Expense saved + Contextual Budget Status
    deactivate ExpenseAPI
    
    opt If alert is applicable (>= 80%)
        Frontend->>Frontend: Display alert/warning notification UI
    end
    Frontend-->>User: Returns updated budget status and UI state
    deactivate Frontend
```

---

### Diagram 3: Spending Prediction Flow

**Scenario:** The user wants future spending forecasts. The system pulls historical transaction data and delegates trend generation to the AI/ML Prediction Engine.

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant Dashboard as Frontend Dashboard
    participant PredictionAPI as Prediction API
    participant DB as Database
    participant AIEngine as AI/ML Prediction Engine

    User->>Dashboard: Opens Spending Prediction section
    activate Dashboard
    Dashboard->>PredictionAPI: GET /api/predictions/forecast
    activate PredictionAPI
    
    PredictionAPI->>DB: Fetch historical transactions
    activate DB
    Note over PredictionAPI, DB: Historical transaction analysis
    DB-->>PredictionAPI: Transaction history data sets
    deactivate DB
    
    PredictionAPI->>AIEngine: Send historical data for evaluation
    activate AIEngine
    AIEngine->>AIEngine: Preprocess historical spending data
    Note right of AIEngine: AI-powered forecasting
    AIEngine->>AIEngine: Predict future spending trends
    Note right of AIEngine: Future trend generation
    AIEngine-->>PredictionAPI: Forecast results (Trends & Insights)
    deactivate AIEngine
    
    PredictionAPI-->>Dashboard: Forecast analytics & payload
    deactivate PredictionAPI
    
    Dashboard->>Dashboard: Render spending forecast graphs
    Dashboard-->>User: Displays forecast graphs and actionable insights
    deactivate Dashboard
```
