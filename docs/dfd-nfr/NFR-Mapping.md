# Non-Functional Requirements (NFR) Mapping
**Project:** AI-Powered Expense Analyzer & Budget Assistant  
**Module:** NFR Mapping & Data Flow Diagrams  
**Prepared by:** Priyam Rai  

---

## 1. Introduction
This document details the Non-Functional Requirements (NFRs) for the **AI-Powered Expense Analyzer & Budget Assistant** and maps them to the actual architectural components and database tables. By establishing clear connection paths between system requirements, implementation approaches, and verification methods, we ensure that the software maintains a high standard of quality, performance, security, and academic rigor.

---

## 2. NFR Mapping Matrix

| NFR ID | NFR Name | Requirement | Related Component | Implementation/Design Approach | Verification Method |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **NFR-01** | **Performance / Response Time** | Load dashboard data within 2 seconds for standard queries; log expense transactions in < 500ms. | API Gateway, Expense Controller, DB Connection Pool | Apply database query optimizations. Enforce indexes on search/filter columns: `idx_expenses_user_id`, `idx_expenses_category_id`, and `idx_expenses_date` in the database. | Performance profiling under simulated load (e.g., JMeter, Artillery) to verify API endpoint latencies under 100 concurrent requests. |
| **NFR-02** | **Security** | Secure all API endpoints via token-based authorization. Store credentials securely. | User Authentication & Management Module, JWT Router, `users` table | Enforce password hashing using standard algorithms (e.g., bcrypt, Argon2) saved in `password_hash` column. Utilize JWT/OAuth2 for session auth. Enforce role-based access control using the `role` ENUM field ('user', 'admin'). | Vulnerability scanning (e.g., OWASP ZAP) and unit tests verifying that unauthorized requests to protected endpoints return HTTP 401 Unauthorized. |
| **NFR-03** | **Availability** | Target a monthly system availability / uptime of 99.9% (excluding scheduled maintenance window). | Backend runtime, Deployment infrastructure, MySQL Database Service (RDS) | Run backend API servers in a multi-availability zone (multi-AZ) setting behind a load balancer with automated auto-scaling and recovery policies. | Synthetic monitoring checks (e.g., Pingdom, UptimeRobot) verifying system health endpoint status every 60 seconds. |
| **NFR-04** | **Prediction Accuracy** | Spending forecast model must achieve a Mean Absolute Percentage Error (MAPE) of < 15% on validation historical datasets. | Spending Analysis & Prediction Service, ML Pipeline | Train prediction models on historical user spend records from the `expenses` table. Retrain monthly. Provide graceful fallbacks (e.g., moving average) if expense history is too sparse. | Automated evaluation scripts executing MAPE and RMSE scoring against historical holdout testing datasets. |
| **NFR-05** | **Data Integrity** | Prevent monetary rounding issues and duplicate budget configurations. | Database Management System, Expense/Budget Managers | Use exact `DECIMAL(10,2)` for representation of cash values. Enforce database-level integrity constraints: `CHECK` constraint on `amount > 0` and `budget_amount > 0`; foreign keys with `ON DELETE RESTRICT` to prevent orphan records; and `UNIQUE (user_id, month, year)` unique constraint on `budgets` table. | Integration tests attempting to write negative expense amounts or duplicate budgets for the same user-month, asserting database-level exceptions. |
| **NFR-06** | **Scalability** | Support up to 10,000 active concurrent users logging transactions without system degradation. | API Gateway, Expense Log Service, Database Cluster | Design stateless REST API microservices to allow easy horizontal scalability. Implement database read replicas and local memory/Redis caching for global lists like `categories` to offload database load. | Automated load testing simulating user activity spikes up to 10,000 concurrent virtual users. |
| **NFR-07** | **Maintainability** | Keep codebase modular and achieve unit test coverage of ≥ 80%. | App Server, Software Modules (1.0 to 7.0), CI/CD pipelines | Adhere strictly to SOLID principles, separating routes, controllers, services, and repository layers. | Code coverage reports (e.g., Jest, pytest-cov, jacoco) integrated into CI/CD build scripts. |
| **NFR-08** | **Reliability** | Ensure transactional safety of all financial records; handle service dropouts gracefully. | Database Transaction Manager, Alert Notifier | Wrap multi-table operations (e.g. inserting expense and triggering alerts) in SQL database transactions. Implement retry policies with exponential back-off for email notifications or external SMS services. | Chaos engineering checks simulating network failures or database packet drops mid-execution, testing that database rollback maintains system state. |

---

## 3. Database Schema Alignment
The database schema (`schema.sql`) incorporates key properties that directly implement our safety and data integrity NFRs:
* **Cassandra/UUID Primary Keys:** All tables use `CHAR(36) DEFAULT (UUID())` to prevent ID enumeration attacks, improving security (NFR-02) and scalability in distributed environments (NFR-06).
* **DECIMAL(10,2):** Used in `expenses.amount` and `budgets.budget_amount`, preventing fractional precision errors common to floating point numbers, aligning with Data Integrity (NFR-05).
* **Unique Constraints:** The `uq_budget_user_month UNIQUE (user_id, month, year)` constraint prevents logical duplicate entries at the database level, ensuring clean budget logs (NFR-05).
* **Check Constraints:** `CHECK (amount > 0)` and `CHECK (budget_amount > 0)` enforce business logic at the data tier.
* **Foreign Key Relational Integrity:** Enforces relationships between table entities via `ON DELETE RESTRICT` (e.g., `fk_expenses_user`), preventing accidental deletion of users/categories when transactions reference them. User deletion is handled via soft-deletes (`is_active` flag) rather than destructive drops, preserving audit trails (NFR-08).
* **Performance Indexes:** Indices such as `idx_expenses_user_id`, `idx_expenses_category_id`, and `idx_expenses_date` accelerate query times drastically under production loads (NFR-01).
