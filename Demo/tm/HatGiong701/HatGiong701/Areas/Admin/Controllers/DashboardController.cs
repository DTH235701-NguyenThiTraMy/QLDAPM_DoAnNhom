using HatGiong701.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HatGiong701.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string period = "all", string? weekValue = null, string? monthValue = null, int? year = null)
        {
            var now = DateTime.Now;
            int selectedYear = year ?? now.Year;

            // Giá trị mặc định cho input week ("yyyy-Www") và input month ("yyyy-MM")
            int currentIsoWeek = ISOWeek.GetWeekOfYear(now);
            int currentIsoYear = ISOWeek.GetYear(now);
            string selectedWeek = string.IsNullOrEmpty(weekValue) ? $"{currentIsoYear}-W{currentIsoWeek:D2}" : weekValue;
            string selectedMonth = string.IsNullOrEmpty(monthValue) ? $"{now.Year}-{now.Month:D2}" : monthValue;

            // Danh sách các năm hiển thị trên dropdown
            var dbYears = await _context.Orders.Select(o => o.OrderDate.Year).Distinct().ToListAsync();
            var availableYears = dbYears.Union(new[] { now.Year, now.Year - 1, now.Year - 2 }).OrderByDescending(y => y).ToList();

            // Xác định khoảng thời gian lọc [startDate, endDate) và nhãn hiển thị
            DateTime? startDate = null;
            DateTime? endDate = null;
            string periodLabel = "Toàn thời gian";

            if (period == "week")
            {
                // Phân tích chuỗi "2026-W48" thành ngày Thứ 2 đầu tuần -> Chủ nhật cuối tuần
                var parts = selectedWeek.Split("-W");
                if (parts.Length == 2 && int.TryParse(parts[0], out int wYear) && int.TryParse(parts[1], out int wNum))
                {
                    startDate = ISOWeek.ToDateTime(wYear, wNum, DayOfWeek.Monday);
                    endDate = startDate.Value.AddDays(7);
                    periodLabel = $"Tuần {wNum}/{wYear} ({startDate.Value:dd/MM} - {endDate.Value.AddDays(-1):dd/MM})";
                }
            }
            else if (period == "month")
            {
                // Phân tích chuỗi "2026-11"
                var parts = selectedMonth.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out int mYear) && int.TryParse(parts[1], out int mNum))
                {
                    startDate = new DateTime(mYear, mNum, 1);
                    endDate = startDate.Value.AddMonths(1);
                    periodLabel = $"Tháng {mNum:D2}/{mYear}";
                }
            }
            else if (period == "year")
            {
                startDate = new DateTime(selectedYear, 1, 1);
                endDate = startDate.Value.AddYears(1);
                periodLabel = $"Năm {selectedYear}";
            }

            // Lọc danh sách đơn hàng theo khoảng [startDate, endDate)
            var ordersQuery = _context.Orders.AsQueryable();
            if (startDate.HasValue && endDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate >= startDate.Value && o.OrderDate < endDate.Value);
            }

            // 1. Thống kê tổng quan
            var completedOrders = await ordersQuery.Where(o => o.Status == "Hoàn thành").ToListAsync();

            ViewBag.Period = period;
            ViewBag.SelectedWeek = selectedWeek;
            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.AvailableYears = availableYears;
            ViewBag.PeriodLabel = periodLabel;

            ViewBag.TotalRevenue = completedOrders.Sum(o => o.TotalAmount);
            ViewBag.TotalOrders = await ordersQuery.CountAsync();
            ViewBag.PendingOrdersCount = await _context.Orders.CountAsync(o => o.Status == "Chờ xác nhận");
            ViewBag.TotalSeeds = await _context.Seeds.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();

            // 2. Cảnh báo hạt giống sắp hết hàng (< 10 gói)
            ViewBag.LowStockSeeds = await _context.Seeds
                .Where(s => s.StockQuantity < 10)
                .OrderBy(s => s.StockQuantity)
                .ToListAsync();

            // 3. 5 đơn hàng mới nhất trong kỳ
            ViewBag.RecentOrders = await ordersQuery
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            // 4. Doanh thu theo Danh mục
            var categoryRevenue = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Seed!)
                    .ThenInclude(s => s.Category)
                .Where(od => od.Order!.Status == "Hoàn thành"
                          && (!startDate.HasValue || (od.Order.OrderDate >= startDate.Value && od.Order.OrderDate < endDate!.Value)))
                .GroupBy(od => od.Seed!.Category!.Name)
                .Select(g => new
                {
                    CategoryName = g.Key,
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();

            ViewBag.CategoryNames = categoryRevenue.Select(c => c.CategoryName).ToList();
            ViewBag.CategoryRevenues = categoryRevenue.Select(c => c.Revenue).ToList();

            // 5. Top 5 hạt giống bán chạy nhất
            var topSeeds = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Seed)
                .Where(od => od.Order!.Status == "Hoàn thành"
                          && (!startDate.HasValue || (od.Order.OrderDate >= startDate.Value && od.Order.OrderDate < endDate!.Value)))
                .GroupBy(od => od.Seed!.Name)
                .Select(g => new
                {
                    SeedName = g.Key,
                    TotalSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToListAsync();

            ViewBag.TopSeedNames = topSeeds.Select(t => t.SeedName).ToList();
            ViewBag.TopSeedSold = topSeeds.Select(t => t.TotalSold).ToList();

            return View();
        }
    }
}