# NFR Mapping & Data Flow Diagrams (DFD)

**Project:** AI-Powered Expense Analyzer & Budget Assistant  
**Module:** System Analysis and Design  
**Prepared by:** Priyam Rai (Assigned Responsibility)  

---

## 1. Purpose of the Folder
This directory serves as the official package for the **Non-Functional Requirements (NFR) Mapping** and **Data Flow Diagrams (DFD)**. It contains both the vector PDF exports suitable for university submission and the editable source files. The objective of this module is to define how inputs and outputs flow through logical processes, identify where data is stored, map NFRs to their structural locations in the system, and establish trace documentation that guarantees the engineering requirements are realized.

---

## 2. Contents of this Directory
The following key files have been created inside this folder:
1. **[README.md](file:///c:/Users/priya/Desktop/AI-Powered-Expense-Analyzer-Budget-Assistant-main/docs/dfd-nfr/README.md):** Main documentation, traceability tables, and integration notes (this file).
2. **[NFR-Mapping.md](file:///c:/Users/priya/Desktop/AI-Powered-Expense-Analyzer-Budget-Assistant-main/docs/dfd-nfr/NFR-Mapping.md):** Detailed descriptions of the 8 core Non-Functional Requirements, showing their component alignment and verification methods.
3. **[NFR-Mapping.pdf](file:///c:/Users/priya/Desktop/AI-Powered-Expense-Analyzer-Budget-Assistant-main/docs/dfd-nfr/NFR-Mapping.pdf):** Multi-page vector PDF containing the NFR Mapping Matrix.
4. **[NFR-Architecture-Mapping.pdf](file:///c:/Users/priya/Desktop/AI-Powered-Expense-Analyzer-Budget-Assistant-main/docs/dfd-nfr/NFR-Architecture-Mapping.pdf):** Landscape matrix mapping NFR categories (Performance, Security, Availability, Prediction Accuracy, Data Integrity, Scalability, Maintainability, Reliability) logically to architectural layers structure.
5. **[DFD-Level-0.pdf](file:///c:/Users/priya/Desktop/AI-Powered-Expense-Analyzer-Budget-Assistant-main/docs/dfd-nfr/DFD-Level-0.pdf):** Context Diagram showing logical flow between users, admins, and the system boundaries.
6. **[DFD-Level-1.pdf](file:///c:/Users/priya/Desktop/AI-Powered-Expense-Analyzer-Budget-Assistant-main/docs/dfd-nfr/DFD-Level-1.pdf):** High-precision vector diagram depicting 7 system processes (Authentication, Expense, Category, Budget, Prediction, Alerts, Audit) interfacing with 6 normalized database stores.
7. **[DFD-Level-0.drawio](file:///c:/Users/priya/Desktop/AI-Powered-Expense-Analyzer-Budget-Assistant-main/docs/dfd-nfr/DFD-Level-0.drawio):** Editable Draw.io XML source file for the Context Diagram.
8. **[DFD-Level-1.drawio](file:///c:/Users/priya/Desktop/AI-Powered-Expense-Analyzer-Budget-Assistant-main/docs/dfd-nfr/DFD-Level-1.drawio):** Editable Draw.io XML source file for the Level 1 DFD.

---

## 3. Explanations of Mappings & Diagrams

### A. DFD Level 0 — Context Diagram
* **Scope:** Defines the interface between external agents (`User`, `Administrator`) and the application boundaries (Process `0.0`). It abstracts internal details, showcasing boundary entry data (credentials, expense parameters, budget caps) and exit data (auth tokens, warning notifications, ML-generated forecasting reports).

### B. DFD Level 1 — Architectural Decomposition
* **Processes (1.0 to 7.0):** Deconstructs the system into modular execution modules:
  * **1.0 User Auth & Management:** Encrypted token management and role matching.
  * **2.0 Expense Management:** Inputs verification and database transaction processing.
  * **3.0 Category Management:** Global, admin-managed category constraints.
  * **4.0 Budget Management:** Calculations comparing transactions aggregates vs. threshold settings.
  * **5.0 Spending Analysis & Prediction:** ML training pipelines scanning history to formulate forecasts.
  * **6.0 Alert/Notification Management:** Real-time push dispatches for 80%, 100% exceeded, and AI predictive logs.
  * **7.0 Administration & Audit:** Administrative management and readouts of user state and audit logging.
* **Data Stores (D1 to D6):** Correspond exactly with the database schema table structures:
  * `D1 Users` $\rightarrow$ `users` table
  * `D2 Categories` $\rightarrow$ `categories` table
  * `D3 Expenses` $\rightarrow$ `expenses` table
  * `D4 Budgets` $\rightarrow$ `budgets` table
  * `D5 Alerts` $\rightarrow$ `alerts` table
  * `D6 Admin/Audit` $\rightarrow$ System logging mechanism

---

## 4. Traceability Matrix

This table traces system requirements and execution scenarios directly to NFRs, architectural layers, DFD processes, and database structures:

| Scenario / Req ID | Requirement Description | NFR ID | Associated Architectural Layer | DFD Process | Associated Data Store(s) |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **TR-01** (Add Expense) | User submits transactional logs. System validates and records amount, date, description, and classification. | NFR-01 (Performance), NFR-05 (Data Integrity) | Presentation/API, Business Logic, Database | `2.0 Expense Management` | `D1 Users` (Verification)<br>`D2 Categories` (Validation)<br>`D3 Expenses` (Insert) |
| **TR-02** (Budget 80% Warning) | Month-to-date spending aggregate reaches 80% of configured monthly budget limit. Dispatches warning alert. | NFR-05 (Data Integrity), NFR-08 (Reliability) | Business Logic, Alerting Engine, Database | `4.0 Budget Management`<br>`6.0 Alert Management` | `D3 Expenses` (aggregation)<br>`D4 Budgets` (fetch)<br>`D5 Alerts` (save warning) |
| **TR-03** (Budget 100% Exceeded) | Month-to-date spending aggregate reaches/exceeded 100% of budget limit. Dispatches urgent breach notification. | NFR-05 (Data Integrity), NFR-08 (Reliability) | Business Logic, Alerting Engine, Database | `4.0 Budget Management`<br>`6.0 Alert Management` | `D3 Expenses` (aggregation)<br>`D4 Budgets` (fetch)<br>`D5 Alerts` (save exceeded) |
| **TR-04** (Spending Prediction) | Analyze past spending habits, train neural network/regression models, project path, trigger warning notifications. | NFR-04 (Accuracy), NFR-06 (Scalability) | AI/ML Pipeline, Alerting Engine, Database | `5.0 Spending Analysis`<br>`6.0 Alert Management` | `D3 Expenses` (training data)<br>`D5 Alerts` (save predictive alert) |
| **TR-05** (User Access Control) | Handle user logins, role configuration (user vs admin), soft-deletes toggling `is_active` values. | NFR-02 (Security) | Presentation/API, Business Logic, Database | `1.0 User Auth`<br>`7.0 Admin & Audit` | `D1 Users` (update/verify) |
| **TR-06** (Category Definitions) | Admins construct global categories list. Prevents users setting custom inconsistent text names. | NFR-07 (Maintainability) | Presentation/API, Business Logic, Database | `3.0 Category Management` | `D2 Categories` (write) |
| **TR-07** (Security Auditing) | Track user status transitions and critical system queries. | NFR-08 (Reliability) | Administration Layer, Database | `7.0 Admin & Audit` | `D6 Admin/Audit` (insert log) |

---

## 5. Architectural Relationships

This section explains the direct structural integration points between this DFD/NFR package and the work of other team members:

### A. Relationship with Database Design (Shruti Mehkarkar's Module)
* The DFD Data Stores mapped in Level-1 correspond exactly to the tables in `docs/database/schema.sql`.
* The `D4 Budgets` store reflects the `budgets` table where `UNIQUE(user_id, month, year)` prevents duplicate budgets.
* The `D5 Alerts` store reflects the `alerts` table where the constraint `alert_type ENUM('WARNING_80', 'EXCEEDED_100', 'PREDICTIVE')` is enforced.
* Soft delete checks inside DFD Process 7.0 interface with the `is_active` column in `users`.

### B. Relationship with System Architecture (Gitanshi Singh & Samiksha Tiwari's Module)
* NFR-Performance (NFR-01) index mapping uses index configurations in database scripts.
* Horizontally scalable design (NFR-06) maps to stateless server architecture documented in design templates.
* Separate prediction worker (NFR-04) ensures the web controller routes are never blocked by ML computation workloads.

### C. Relationship with UML & Use Case Diagrams (Viya Sharma's Module)
* The external entities `User` and `Administrator` match the main system actors defined in the Use Case diagrams.
* Process 1.0 (Auth), 2.0 (Expense), 4.0 (Budget), and 5.0 (Prediction) map directly to the primary use-cases (e.g., Log Expense, Define Monthly Budget, View Forecast Dashboard, Manage Categories).

### D. Relationship with Sequence Flows (Abhiudaya Pratap Singh's Module)
* **Add Expense Sequence:** Reflects `User -> Process 2.0 -> D3 Expenses` flow. Process 2.0 validates referencing items in D1 and D2 before persisting.
* **80%/100% alerts Sequence:** Confirms that after Expense write confirmation, Process 2.0 signals Process 4.0, which reads D4 (limits) and aggregates D3 (history). Finding a breach, it signals Process 6.0, which inserts data into D5 Alerts and notifies the User.
* **Spending Prediction Sequence:** Confirms Process 5.0 reads D3 Expenses asynchronously, generates a prediction, and pushes it to the UI or Process 6.0 for warnings in D5.
