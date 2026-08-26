// SpendWise expense page — wired to the backend API through the Web app.
// All rendering/formatting by Gitanshi; data layer now hits /Expense/* endpoints,
// which proxy the authenticated API using the server-side session token.

let editingId = null;
let expenses = [];
let categories = [];

function formatCurrency(value) {
    return "₹" + Number(value).toLocaleString("en-IN");
}

function formatDate(date) {
    return new Date(date + "T00:00:00").toLocaleDateString(
        "en-IN",
        {
            day: "2-digit",
            month: "short",
            year: "numeric"
        }
    );
}

function getCategoryEmoji(category) {

    const name = (category || "").toLowerCase();

    if (name.includes("food") || name.includes("dining")) return "🍔";
    if (name.includes("travel") || name.includes("transport")) return "🚕";
    if (name.includes("shopping")) return "🛍";
    if (name.includes("bill") || name.includes("utilit")) return "🧾";
    if (name.includes("entertain")) return "🎬";
    if (name.includes("health")) return "⚕️";
    if (name.includes("housing") || name.includes("rent")) return "🏠";

    return "💰";
}

function getExpenseStatus(amount) {
    if (amount >= 1000) {
        return "High";
    }

    return "Normal";
}

async function apiFetch(url, options = {}) {

    const response = await fetch(url, options);

    if (response.status === 401) {
        window.location.href = "/Account/Login";
        throw new Error("Session expired");
    }

    return response;
}

async function loadCategories() {

    try {
        categories = await apiFetch("/Expense/Categories").then(r => r.json());
    } catch {
        categories = [];
    }

    const modalSelect =
        document.getElementById("expenseCategory");

    const filterSelect =
        document.getElementById("categoryFilter");

    if (modalSelect) {

        modalSelect.innerHTML =
            '<option value="" disabled selected>Select category</option>';

        categories.forEach(category => {

            const option = document.createElement("option");

            option.value = category.categoryId;
            option.textContent = category.name;

            modalSelect.appendChild(option);
        });
    }

    if (filterSelect) {

        filterSelect.innerHTML =
            '<option value="">All Categories</option>';

        categories.forEach(category => {

            const option = document.createElement("option");

            option.value = category.name;
            option.textContent = category.name;

            filterSelect.appendChild(option);
        });
    }
}

async function loadExpenses() {

    try {
        expenses = await apiFetch("/Expense/List").then(r => r.json());
    } catch {
        expenses = [];
    }

    renderExpenses();
}

function renderExpenses() {

    const table = document.getElementById("expenseTable");

    if (!table) {
        return;
    }

    table.innerHTML = "";

    expenses.forEach(expense => {

        const row = document.createElement("tr");

        row.dataset.category = expense.category;
        row.dataset.id = expense.id;

        const status = getExpenseStatus(expense.amount);

        row.innerHTML = `
            <td>${formatDate(expense.date)}</td>

            <td>
                <span class="expense-category">
                    ${getCategoryEmoji(expense.category)}
                    ${escapeHtml(expense.category)}
                </span>
            </td>

            <td>${escapeHtml(expense.description)}</td>

            <td class="amount">
                ${formatCurrency(expense.amount)}
            </td>

            <td>
                <span class="status ${status === "High" ? "warning" : "normal"}">
                    ${status}
                </span>
            </td>

            <td>
                <button
                    class="table-btn edit-btn"
                    onclick="editExpense('${expense.id}')">
                    Edit
                </button>

                <button
                    class="table-btn delete-btn"
                    onclick="deleteExpense('${expense.id}')">
                    Delete
                </button>
            </td>
        `;

        table.appendChild(row);
    });

    updateCount();
}

function escapeHtml(value) {

    const div = document.createElement("div");

    div.textContent = value ?? "";

    return div.innerHTML;
}

function openExpenseModal() {

    const modal = document.getElementById("expenseModal");

    document.getElementById("modalTitle").textContent =
        "Add Expense";

    document.getElementById("expenseAmount").value = "";
    document.getElementById("expenseCategory").value = "";
    document.getElementById("expenseDescription").value = "";

    const today =
        new Date().toISOString().split("T")[0];

    document.getElementById("expenseDate").value = today;

    editingId = null;

    modal.classList.add("show");
}

function closeExpenseModal() {

    const modal =
        document.getElementById("expenseModal");

    modal.classList.remove("show");

    // Make sure the modal is completely hidden.
    modal.style.display = "none";

    // Allow it to be displayed again when opened.
    setTimeout(() => {
        modal.style.display = "";
    }, 10);

    editingId = null;
}

async function saveExpense(event) {

    event.preventDefault();

    const amount =
        Number(
            document.getElementById("expenseAmount").value
        );

    const categoryId =
        Number(
            document.getElementById("expenseCategory").value
        );

    const date =
        document.getElementById("expenseDate").value;

    const description =
        document.getElementById("expenseDescription").value.trim()
        || "Expense";

    if (
        !amount ||
        amount <= 0 ||
        !categoryId ||
        !date
    ) {
        return;
    }

    const payload = {
        categoryId,
        amount,
        date,
        description
    };

    let response;

    if (editingId) {

        response = await apiFetch(`/Expense/Update/${Number(editingId)}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

    } else {

        response = await apiFetch("/Expense/Create", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });
    }

    if (response.status === 401) {
        window.location.href = "/Account/Login";
        return;
    }

    if (!response.ok) {
        alert("Could not save the expense. Please try again.");
        return;
    }

    await loadExpenses();

    closeExpenseModal();

    // Tell other frontend pages that expenses changed.
    window.dispatchEvent(
        new CustomEvent("expensesUpdated")
    );
}

async function editExpense(id) {

    const numericId = Number(id);
    const expense = expenses.find(item => item.id === numericId);

    if (!expense) {
        await loadExpenses();
        return;
    }

    editingId = numericId;

    document.getElementById("modalTitle").textContent =
        "Edit Expense";

    document.getElementById("expenseAmount").value =
        expense.amount;

    document.getElementById("expenseCategory").value =
        expense.categoryId;

    document.getElementById("expenseDate").value =
        expense.date;

    document.getElementById("expenseDescription").value =
        expense.description;

    document
        .getElementById("expenseModal")
        .classList.add("show");
}

async function deleteExpense(id) {

    const numericId = Number(id);
    const expense = expenses.find(item => item.id === numericId);

    if (!expense) {
        await loadExpenses();
        return;
    }

    const confirmed =
        confirm(
            `Delete ${expense.description} (${formatCurrency(expense.amount)})?`
        );

    if (!confirmed) {
        return;
    }

    const response = await apiFetch(`/Expense/Delete/${Number(id)}`, {
        method: "DELETE"
    });

    if (response.status === 401) {
        window.location.href = "/Account/Login";
        return;
    }

    if (!response.ok && response.status !== 404) {
        alert("Could not delete the expense. Please try again.");
        return;
    }

    await loadExpenses();

    window.dispatchEvent(
        new CustomEvent("expensesUpdated")
    );
}

function filterExpenses() {

    const search =
        document.getElementById("expenseSearch")
            ?.value
            .toLowerCase() || "";

    const category =
        document.getElementById("categoryFilter")
            ?.value || "";

    const rows =
        document.querySelectorAll(
            "#expenseTable tr"
        );

    rows.forEach(row => {

        const text =
            row.textContent.toLowerCase();

        const rowCategory =
            row.dataset.category;

        const matchesSearch =
            text.includes(search);

        const matchesCategory =
            !category ||
            rowCategory === category;

        row.style.display =
            matchesSearch && matchesCategory
                ? ""
                : "none";
    });

    updateCount();
}

function updateCount() {

    const rows =
        document.querySelectorAll(
            "#expenseTable tr"
        );

    const visibleRows =
        [...rows].filter(
            row => row.style.display !== "none"
        );

    const count =
        document.getElementById("expenseCount");

    if (count) {

        count.textContent =
            `${visibleRows.length} transactions`;
    }
}

document.addEventListener(
    "DOMContentLoaded",
    () => {

        loadCategories().then(loadExpenses);

        const modal =
            document.getElementById("expenseModal");

        if (modal) {

            modal.addEventListener(
                "click",
                event => {

                    if (
                        event.target === modal
                    ) {
                        closeExpenseModal();
                    }
                }
            );
        }
    }
);
