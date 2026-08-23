-- USERS
CREATE TABLE users (
    user_id         CHAR(36)      NOT NULL DEFAULT (UUID()) PRIMARY KEY,
    name            VARCHAR(100)  NOT NULL,
    email           VARCHAR(150)  NOT NULL UNIQUE,
    password_hash   VARCHAR(255)  NOT NULL,
    role            ENUM('user', 'admin') NOT NULL DEFAULT 'user',
    is_active       BOOLEAN       NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP
);


-- CATEGORIES (global, admin-managed)
CREATE TABLE categories (
    category_id     CHAR(36)      NOT NULL DEFAULT (UUID()) PRIMARY KEY,
    name            VARCHAR(50)   NOT NULL UNIQUE,
    is_active       BOOLEAN       NOT NULL DEFAULT TRUE
);

-- EXPENSES
CREATE TABLE expenses (
    expense_id      CHAR(36)      NOT NULL DEFAULT (UUID()) PRIMARY KEY,
    user_id         CHAR(36)      NOT NULL,
    category_id     CHAR(36)      NOT NULL,
    amount          DECIMAL(10,2) NOT NULL CHECK (amount > 0),
    expense_date    DATE          NOT NULL,
    description     VARCHAR(255),
    created_at      TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP
                        ON UPDATE CURRENT_TIMESTAMP,
 
    CONSTRAINT fk_expenses_user
        FOREIGN KEY (user_id) REFERENCES users(user_id)
        ON DELETE RESTRICT,
    CONSTRAINT fk_expenses_category
        FOREIGN KEY (category_id) REFERENCES categories(category_id)
        ON DELETE RESTRICT
);
CREATE INDEX idx_expenses_user_id      ON expenses(user_id);
CREATE INDEX idx_expenses_category_id  ON expenses(category_id);
CREATE INDEX idx_expenses_date         ON expenses(expense_date);

-- BUDGETS  (one row per user per month/year)
CREATE TABLE budgets (
    budget_id       CHAR(36)      NOT NULL DEFAULT (UUID()) PRIMARY KEY,
    user_id         CHAR(36)      NOT NULL,
    month           TINYINT       NOT NULL CHECK (month BETWEEN 1 AND 12),
    year            SMALLINT      NOT NULL CHECK (year BETWEEN 2000 AND 2100),
    budget_amount   DECIMAL(10,2) NOT NULL CHECK (budget_amount > 0),
    created_at      TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP
                        ON UPDATE CURRENT_TIMESTAMP,
 
    CONSTRAINT fk_budgets_user
        FOREIGN KEY (user_id) REFERENCES users(user_id)
        ON DELETE RESTRICT,
    CONSTRAINT uq_budget_user_month UNIQUE (user_id, month, year)
);


CREATE INDEX idx_budgets_user_id ON budgets(user_id);

-- ALERTS
CREATE TABLE alerts (
    alert_id        CHAR(36)      NOT NULL DEFAULT (UUID()) PRIMARY KEY,
    user_id         CHAR(36)      NOT NULL,
    budget_id       CHAR(36)      NULL,
    alert_type      ENUM('WARNING_80', 'EXCEEDED_100', 'PREDICTIVE') NOT NULL,
    message         VARCHAR(255)  NOT NULL,
    is_read         BOOLEAN       NOT NULL DEFAULT FALSE,
    created_at      TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP,
 
    CONSTRAINT fk_alerts_user
        FOREIGN KEY (user_id) REFERENCES users(user_id)
        ON DELETE RESTRICT,
    CONSTRAINT fk_alerts_budget
        FOREIGN KEY (budget_id) REFERENCES budgets(budget_id)
        ON DELETE SET NULL
);


CREATE INDEX idx_alerts_user_id ON alerts(user_id);
