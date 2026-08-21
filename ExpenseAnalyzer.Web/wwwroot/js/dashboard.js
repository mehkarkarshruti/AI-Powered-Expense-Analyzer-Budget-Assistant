let currentBudget = 15000;
const totalSpent = 12450;
const predictedSpending = 17800;

function formatCurrency(value) {
    return "₹" + Number(value).toLocaleString("en-IN");
}

function openBudgetModal() {
    document.getElementById("newBudget").value = currentBudget;
    document.getElementById("budgetModal").classList.add("show");
}

function closeBudgetModal() {
    document.getElementById("budgetModal").classList.remove("show");
}

function updateBudget(event) {
    event.preventDefault();

    const newBudget =
        Number(document.getElementById("newBudget").value);

    if (!newBudget || newBudget <= 0) {
        return;
    }

    currentBudget = newBudget;

    refreshBudgetUI();

    closeBudgetModal();
}

function refreshBudgetUI() {

    const remaining = currentBudget - totalSpent;

    const percentage =
        (totalSpent / currentBudget) * 100;

    document.getElementById("budgetAmount").textContent =
        formatCurrency(currentBudget);

    document.getElementById("remainingAmount").textContent =
        formatCurrency(Math.max(remaining, 0));

    document.getElementById("budgetPercentage").textContent =
        remaining >= 0
            ? Math.round((remaining / currentBudget) * 100) + "% remaining"
            : "Budget exceeded";

    document.getElementById("budgetProgress").style.width =
        Math.min(percentage, 100) + "%";

    document.getElementById("spentLabel").textContent =
        formatCurrency(totalSpent) + " spent";

    document.getElementById("budgetLabel").textContent =
        formatCurrency(currentBudget) + " budget";

    const difference =
        predictedSpending - currentBudget;

    document.getElementById("predictionDifference").textContent =
        formatCurrency(Math.abs(difference));

    const alert = document.getElementById("budgetAlert");
    const alertText = document.getElementById("budgetAlertText");

    alert.classList.remove(
        "warning-alert",
        "danger-alert",
        "success-alert"
    );

    if (percentage >= 100) {

        alert.classList.add("danger-alert");

        alertText.textContent =
            "You have exceeded your monthly budget.";

    } else if (percentage >= 80) {

        alert.classList.add("warning-alert");

        alertText.textContent =
            "You have used " +
            Math.round(percentage) +
            "% of your monthly budget.";

    } else {

        alert.classList.add("success-alert");

        alertText.textContent =
            "You are currently within your monthly budget.";

    }
}

document.addEventListener("DOMContentLoaded", refreshBudgetUI);
