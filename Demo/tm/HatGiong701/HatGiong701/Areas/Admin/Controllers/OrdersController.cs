using HatGiong701.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HatGiong701.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách tất cả đơn hàng
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        // Xem chi tiết đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails!)
                    .ThenInclude(od => od.Seed)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // Cập nhật trạng thái đơn hàng & Tự động trừ tồn kho khi "Hoàn thành"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails!)
                    .ThenInclude(od => od.Seed)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return NotFound();

            // Nếu chuyển từ trạng thái khác sang "Hoàn thành" -> Trừ số lượng tồn kho
            if (order.Status != "Hoàn thành" && status == "Hoàn thành")
            {
                foreach (var item in order.OrderDetails!)
                {
                    if (item.Seed != null)
                    {
                        item.Seed.StockQuantity = Math.Max(0, item.Seed.StockQuantity - item.Quantity);
                    }
                }
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật trạng thái đơn hàng #{orderId} thành '{status}'!";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        // Xóa đơn hàng (kèm xóa chi tiết đơn hàng để tránh lỗi khóa ngoại)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order != null)
            {
                if (order.OrderDetails != null && order.OrderDetails.Any())
                {
                    _context.OrderDetails.RemoveRange(order.OrderDetails);
                }

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã xóa đơn hàng #{id} thành công!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}