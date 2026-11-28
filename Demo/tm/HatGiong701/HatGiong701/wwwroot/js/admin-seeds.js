document.addEventListener("DOMContentLoaded", function () {
    // 1. Xử lý lọc và tìm kiếm ở trang Danh sách Hạt giống (Index)
    const filterBtns = document.querySelectorAll(".seed-filter-btn");
    const searchInput = document.getElementById("seedSearchInput");
    const rows = document.querySelectorAll(".seed-row");
    let currentFilter = "all";

    if (filterBtns.length > 0 || searchInput) {
        function filterSeeds() {
            const keyword = searchInput ? searchInput.value.toLowerCase().trim() : "";
            rows.forEach(row => {
                const rowStock = row.getAttribute("data-stock");
                const rowText = row.innerText.toLowerCase();
                const matchFilter = (currentFilter === "all" || rowStock === currentFilter);
                const matchKeyword = rowText.includes(keyword);
                row.style.display = (matchFilter && matchKeyword) ? "" : "none";
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
                currentFilter = this.getAttribute("data-filter");
                filterSeeds();
            });
        });

        if (searchInput) {
            searchInput.addEventListener("input", filterSeeds);
        }
    }

    // 2. Xử lý xem trước hình ảnh ở trang Thêm mới (Create) & Chỉnh sửa (Edit)
    const imageFileInput = document.getElementById("imageFileInput");
    const imagePreview = document.getElementById("imagePreview");

    if (imageFileInput && imagePreview) {
        imageFileInput.addEventListener("change", function (e) {
            const file = e.target.files[0];
            if (file) {
                imagePreview.src = URL.createObjectURL(file);
            }
        });
    }
});