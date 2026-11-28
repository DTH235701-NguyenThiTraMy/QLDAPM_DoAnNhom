document.addEventListener("DOMContentLoaded", function () {
    const filterBtns = document.querySelectorAll(".filter-btn");
    const searchInput = document.getElementById("orderSearchInput");
    const rows = document.querySelectorAll(".order-row");
    let currentStatus = "all";

    function filterTable() {
        const keyword = searchInput ? searchInput.value.toLowerCase().trim() : "";
        rows.forEach(row => {
            const rowStatus = row.getAttribute("data-status");
            const rowText = row.innerText.toLowerCase();
            const matchStatus = (currentStatus === "all" || rowStatus === currentStatus);
            const matchKeyword = rowText.includes(keyword);
            row.style.display = (matchStatus && matchKeyword) ? "" : "none";
        });
    }

    filterBtns.forEach(btn => {
        btn.addEventListener("click", function () {
            filterBtns.forEach(b => {
                b.classList.remove("btn-success", "active");
                b.classList.add("btn-outline-secondary");
            });
            this.classList.remove("btn-outline-secondary");
            this.classList.add("btn-success", "active");
            currentStatus = this.getAttribute("data-status");
            filterTable();
        });
    });

    if (searchInput) {
        searchInput.addEventListener("input", filterTable);
    }
});