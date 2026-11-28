document.addEventListener("DOMContentLoaded", function () {
    // 1. Biểu đồ vành khuyên (Doughnut) doanh thu theo danh mục
    const catCanvas = document.getElementById("categoryChart");
    if (catCanvas) {
        const catLabels = JSON.parse(catCanvas.dataset.labels || "[]");
        const catData = JSON.parse(catCanvas.dataset.values || "[]");

        if (catData.length > 0) {
            new Chart(catCanvas, {
                type: "doughnut",
                data: {
                    labels: catLabels,
                    datasets: [{
                        data: catData,
                        backgroundColor: [
                            "#198754", "#0d6efd", "#ffc107", "#dc3545",
                            "#6f42c1", "#20c997", "#fd7e14", "#0dcaf0"
                        ],
                        borderWidth: 2,
                        borderColor: "#ffffff",
                        hoverOffset: 6
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    cutout: "60%",
                    layout: {
                        padding: { top: 8, bottom: 8, left: 8, right: 8 }
                    },
                    plugins: {
                        legend: {
                            position: "right",
                            labels: {
                                usePointStyle: true,
                                pointStyle: "circle",
                                padding: 12,
                                font: { size: 12, weight: "500" }
                            }
                        },
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    const label = context.label || "";
                                    const value = context.parsed || 0;
                                    const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                    const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
                                    return ` ${label}: ${value.toLocaleString("vi-VN")} ₫ (${percentage}%)`;
                                }
                            }
                        }
                    }
                }
            });
        }
    }

    // 2. Biểu đồ cột Top 5 hạt giống bán chạy
    const seedCanvas = document.getElementById("topSeedChart");
    if (seedCanvas) {
        const seedLabels = JSON.parse(seedCanvas.dataset.labels || "[]");
        const seedData = JSON.parse(seedCanvas.dataset.values || "[]");

        if (seedData.length > 0) {
            new Chart(seedCanvas, {
                type: "bar",
                data: {
                    labels: seedLabels,
                    datasets: [{
                        label: "Số gói đã bán",
                        data: seedData,
                        backgroundColor: "#20c997",
                        borderRadius: 6,
                        maxBarThickness: 42
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    layout: {
                        padding: { top: 10, bottom: 4 }
                    },
                    plugins: {
                        legend: { display: false },
                        tooltip: {
                            callbacks: {
                                title: function (items) {
                                    return items[0].label;
                                },
                                label: function (context) {
                                    return ` Đã bán: ${context.parsed.y} gói`;
                                }
                            }
                        }
                    },
                    scales: {
                        x: {
                            grid: { display: false },
                            ticks: {
                                maxRotation: 0,
                                autoSkip: false,
                                font: { size: 11 },
                                callback: function (value) {
                                    const label = this.getLabelForValue(value);
                                    return label.length > 14 ? label.substring(0, 14) + "…" : label;
                                }
                            }
                        },
                        y: {
                            beginAtZero: true,
                            ticks: { stepSize: 1 }
                        }
                    }
                }
            });
        }
    }
    // Tự động submit form lọc thời gian khi đổi chế độ (Tất cả / Tuần / Tháng / Năm)
    const periodSelect = document.getElementById("periodSelect");
    if (periodSelect) {
        periodSelect.addEventListener("change", function () {
            document.getElementById("filterStatsForm")?.submit();
        });
    }
});