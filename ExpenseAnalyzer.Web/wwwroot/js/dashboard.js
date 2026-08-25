let currentBudget = (window.serverData && window.serverData.budget) || 15000;
const totalSpent = (window.serverData && window.serverData.totalSpent) || 0;

function formatCurrency(value) {
    return "₹" + Number(value).toLocaleString("en-IN");
}

function openBudgetModal() {
    const modal = document.getElementById("budgetModal");

    document.getElementById("newBudget").value = currentBudget;

    modal.style.display = "flex";
    modal.classList.add("show");
}

function closeBudgetModal() {
    const modal = document.getElementById("budgetModal");

    modal.classList.remove("show");
    modal.style.display = "none";
}

async function updateBudget(event) {
    event.preventDefault();

    const input = document.getElementById("newBudget");
    const newBudget = Number(input.value);

    if (!newBudget || newBudget <= 0) {
        return;
    }

    try {

        const response = await fetch("/Dashboard/SetBudget", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ budgetAmount: newBudget })
        });

        if (response.status === 401) {
            window.location.href = "/Account/Login";
            return;
        }

        if (!response.ok) {
            alert("Could not save your budget. Please try again.");
            return;
        }

        currentBudget = newBudget;

        refreshBudgetUI();

    } catch {
        alert("Could not save your budget. Please try again.");
        return;
    }

    // Save and immediately close the modal
    const modal = document.getElementById("budgetModal");
    modal.classList.remove("show");
    modal.style.display = "none";

    // Clear the input
    input.value = "";
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
        Math.round(percentage) + "% of budget";

    document.getElementById("budgetProgress").style.width =
        Math.min(percentage, 100) + "%";

    document.getElementById("spentLabel").textContent =
        formatCurrency(totalSpent) + " spent";

    document.getElementById("budgetLabel").textContent =
        formatCurrency(currentBudget) + " budget";


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


document.addEventListener("DOMContentLoaded", () => {
    refreshBudgetUI();
});
