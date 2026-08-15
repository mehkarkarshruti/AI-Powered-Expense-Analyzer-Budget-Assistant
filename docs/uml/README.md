# SpendWise - Use Case & Class Diagrams

This document explains the Use Case Diagram and Class Diagram created for the
SpendWise (AI-Powered Expense Analyzer & Budget Assistant) project. Both
diagrams are derived from the Requirement Analysis document (Group 13) and the
finalized database schema (schema.txt / Database_Design.docx).

## 1. Use Case Diagram

### Purpose

The use case diagram shows who interacts with the SpendWise system (the
actors) and what they can do (the use cases). It is built directly from the
"Actors" and "Functional Requirements" sections of the requirement analysis
document.

### Actors

There are exactly two actors, matching the requirement document:

- User: a registered end user who logs expenses and manages their own
  budget and dashboard.
- Administrator: manages categories and user accounts, and views
  system-wide reports.

Note: the AI/ML prediction engine and the database are part of the system's
internal infrastructure. They are not actors, since they do not represent an
external person or system initiating action against SpendWise. This follows
the explicit distinction made in the requirement document.

### User Use Cases

- Register and login: covers account creation and authentication.
- Manage expenses: covers adding, editing, and deleting an expense. These
  three actions are grouped into one use case because they represent the
  same actor goal (maintaining the expense record) rather than three
  separate goals.
- Set monthly budget: covers setting or updating the budget amount for a
  given month and year.
- View dashboard: covers viewing total spending, category breakdown,
  remaining budget, and average daily spending.
- View prediction: covers viewing the system's predicted spending for the
  current month based on historical data.
- Receive alerts: covers all three alert types from the requirement
  document (the 80 percent warning, the 100 percent exceeded alert, and
  the predictive early warning). These are grouped into one use case
  because from the user's point of view they are the same interaction:
  the system notifies the user about budget status.

### Administrator Use Cases

- Manage categories: add, edit, or deactivate expense categories.
- Manage users: view or deactivate user accounts.
- View usage reports: view system-wide usage information.

### Reading the diagram

Each actor is connected by a line to every use case they can perform. The
large rounded rectangle labeled "SpendWise system" represents the system
boundary: everything inside it is part of the application, and the actors
sit outside it since they are external to the system itself.

## 2. Class Diagram

### Purpose

The class diagram represents the same information as the database schema,
but at the object/entity level. Each database table becomes a class, each
column becomes an attribute, and each foreign key relationship becomes an
association between classes. This diagram is meant to sit between the
database design and the actual application code.

### Classes

- User: holds identity and account information (userId, name, email,
  passwordHash, role, isActive, createdAt). Behaviors include register,
  login, and deactivateAccount.
- Category: holds the admin-managed list of spending categories
  (categoryId, name, isActive). Behaviors include activate and deactivate.
- Expense: the core transactional class (expenseId, amount, expenseDate,
  description, createdAt, updatedAt). Behaviors include addExpense,
  editExpense, and deleteExpense.
- Budget: one instance per user per month and year (budgetId, month, year,
  budgetAmount). Behaviors include setAmount and updateAmount.
- Alert: a system-generated notification (alertId, alertType, message,
  isRead). Behavior includes markAsRead.
- Role: an enumeration with two values, USER and ADMIN. This corresponds to
  the ENUM column on the users table.
- AlertType: an enumeration with three values, WARNING_80, EXCEEDED_100,
  and PREDICTIVE. This corresponds to the ENUM column on the alerts table.

### Relationships

- User to Expense (one to many): a user can log many expenses.
- Category to Expense (one to many): a category can be applied to many
  expenses.
- User to Budget (one to many): a user can have many budgets, one per
  month and year, matching the unique constraint on (user_id, month,
  year) in the schema.
- User to Alert (one to many): a user can receive many alerts.
- Budget to Alert (zero-or-one to many): a budget can trigger many alerts,
  but the relationship is optional (zero or one) because the budget_id
  column on the alerts table is nullable. This allows a predictive alert
  to be created based on a spending trend even before a specific budget
  threshold has technically been reached.
- User to Role and Alert to AlertType are shown as dependency
  relationships, since Role and AlertType are enumerations used by those
  classes rather than independent entities with their own identity.

### Why there is no Prediction class

The AI.docx planning notes describe a possible Prediction table for storing
model output. However, the finalized schema (schema.txt) and the database
design document only define five tables: users, categories, expenses,
budgets, and alerts. There is no predictions table in the finalized design,
which means predicted spending is treated as a computed value produced at
request time rather than data that is stored permanently. For this reason,
the class diagram does not include a Prediction class. If the team decides
to persist prediction history later, a Prediction class and its
relationship to User can be added to both the schema and this diagram.
