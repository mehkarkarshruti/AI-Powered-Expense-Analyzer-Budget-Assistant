let editingRow = null;

function openExpenseModal() {
    document.getElementById("expenseModal").classList.add("show");
    document.getElementById("modalTitle").textContent = "Add Expense";

    document.getElementById("expenseAmount").value = "";
    document.getElementById("expenseCategory").value = "";
    document.getElementById("expenseDescription").value = "";

    const today = new Date().toISOString().split("T")[0];
    document.getElementById("expenseDate").value = today;

    editingRow = null;
}

function closeExpenseModal() {
    document.getElementById("expenseModal").classList.remove("show");
}

function saveExpense(event) {
    event.preventDefault();

    const amount = document.getElementById("expenseAmount").value;
    const category = document.getElementById("expenseCategory").value;
    const date = document.getElementById("expenseDate").value;
    const description =
        document.getElementById("expenseDescription").value || "Expense";

    if (editingRow) {
        editingRow.cells[1].innerHTML =
            `<span class="expense-category">${category}</span>`;

        editingRow.cells[2].textContent = description;
        editingRow.cells[3].textContent = `₹${amount}`;

        editingRow.dataset.category = category;

    } else {
        const table = document.getElementById("expenseTable");

        const row = document.createElement("tr");

        row.dataset.category = category;

        const formattedDate = new Date(date).toLocaleDateString(
            "en-IN",
            {
                day: "2-digit",
                month: "short",
                year: "numeric"
            }
        );

        row.innerHTML = `
            <td>${formattedDate}</td>
            <td>
                <span class="expense-category">${category}</span>
            </td>
            <td>${description}</td>
            <td class="amount">₹${amount}</td>
            <td>
                <span class="status normal">Normal</span>
            </td>
            <td>
                <button class="table-btn edit-btn"
                        onclick="editExpense(this)">
                    Edit
                </button>
                <button class="table-btn delete-btn"
                        onclick="deleteExpense(this)">
                    Delete
                </button>
            </td>
        `;

        table.prepend(row);
    }

    updateCount();
    closeExpenseModal();
}

function editExpense(button) {
    editingRow = button.closest("tr");

    const amount = editingRow
        .cells[3]
        .textContent
        .replace("₹", "")
        .replace(",", "");

    const category = editingRow.dataset.category;
    const description = editingRow.cells[2].textContent;

    document.getElementById("modalTitle").textContent = "Edit Expense";
    document.getElementById("expenseAmount").value = amount;
    document.getElementById("expenseCategory").value = category;
    document.getElementById("expenseDescription").value = description;

    document.getElementById("expenseModal").classList.add("show");
}

function deleteExpense(button) {
    if (confirm("Delete this expense?")) {
        button.closest("tr").remove();
        updateCount();
    }
}

function filterExpenses() {

    const search =
        document.getElementById("expenseSearch")
            .value
            .toLowerCase();

    const category =
        document.getElementById("categoryFilter").value;

    const rows =
        document.querySelectorAll("#expenseTable tr");

    rows.forEach(row => {

        const text = row.textContent.toLowerCase();
        const rowCategory = row.dataset.category;

        const matchesSearch = text.includes(search);
        const matchesCategory =
            !category || rowCategory === category;

        row.style.display =
            matchesSearch && matchesCategory
                ? ""
                : "none";
    });
}

function updateCount() {

    const rows =
        document.querySelectorAll("#expenseTable tr");

    const visibleRows =
        [...rows].filter(row =>
            row.style.display !== "none"
        );

    document.getElementById("expenseCount").textContent =
        `${visibleRows.length} transactions`;
}
