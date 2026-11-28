using HatGiong701.Data;
using HatGiong701.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HatGiong701.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Trang chủ: Hỗ trợ tìm kiếm, lọc theo không gian/độ khó/danh mục, gợi ý theo mùa & phân trang
        public async Task<IActionResult> Index(string? searchString, int? categoryId, string? space, string? careLevel, int page = 1)
        {
            int pageSize = 8;
            if (page < 1) page = 1;

            // 1. Xác định mùa hiện tại dựa theo tháng thực tế của hệ thống
            int currentMonth = DateTime.Now.Month;
            string currentSeason = currentMonth switch
            {
                >= 1 and <= 3 => "Xuân",
                >= 4 and <= 6 => "Hè",
                >= 7 and <= 9 => "Thu",
                _ => "Đông"
            };

            // Lấy danh sách hạt giống gợi ý theo mùa hiện tại (bao gồm cả loại trồng được Quanh năm)
            var seasonalSeeds = await _context.Seeds
                .Include(s => s.Category)
                .Where(s => s.PlantingSeason == currentSeason || s.PlantingSeason == "Quanh năm")
                .OrderBy(s => Guid.NewGuid())
                .Take(4)
                .ToListAsync();

            ViewBag.CurrentSeason = currentSeason;
            ViewBag.SeasonalSeeds = seasonalSeeds;
            ViewBag.Categories = await _context.Categories.ToListAsync();

            // 2. Truy vấn danh sách sản phẩm chính kết hợp bộ lọc "Gợi ý theo không gian & nhu cầu"
            var seedsQuery = _context.Seeds.Include(s => s.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                seedsQuery = seedsQuery.Where(s => s.Name.Contains(searchString));
            }
            if (categoryId.HasValue && categoryId > 0)
            {
                seedsQuery = seedsQuery.Where(s => s.CategoryId == categoryId);
            }
            if (!string.IsNullOrEmpty(space))
            {
                seedsQuery = seedsQuery.Where(s => s.SuitableSpace == space);
            }
            if (!string.IsNullOrEmpty(careLevel))
            {
                seedsQuery = seedsQuery.Where(s => s.CareLevel == careLevel);
            }

            // 3. Tính toán thông số phân trang
            int totalItems = await seedsQuery.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var pagedSeeds = await seedsQuery
                .OrderBy(s => s.SeedId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Lưu lại giá trị đang lọc và thông tin phân trang để truyền sang View
            ViewBag.SearchString = searchString;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SelectedSpace = space;
            ViewBag.SelectedCareLevel = careLevel;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(pagedSeeds);
        }

        // Trang Chi tiết Hạt giống (kèm đánh giá & gợi ý hạt giống cùng không gian trồng)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var seed = await _context.Seeds
                .Include(s => s.Category)
                .Include(s => s.Reviews!)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.SeedId == id);

            if (seed == null) return NotFound();

            // Gợi ý các hạt giống khác trồng được ở cùng không gian (VD: cùng trồng Ban công)
            ViewBag.RelatedSeeds = await _context.Seeds
                .Where(s => s.SuitableSpace == seed.SuitableSpace && s.SeedId != seed.SeedId)
                .Take(4)
                .ToListAsync();

            return View(seed);
        }
        // THÊM HÀM XỬ LÝ ĐÁNH GIÁ SẢN PHẨM
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int seedId, int rating, string comment, IFormFile? reviewImage)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Kiểm tra điều kiện thực tế: Khách phải có đơn hàng ở trạng thái "Hoàn thành" chứa hạt giống này mới được đánh giá
            // (Lưu ý: Nếu muốn test nhanh khi chưa duyệt đơn, bạn có thể tạm bỏ đoạn kiểm tra hasPurchased này)
            bool hasPurchased = await _context.Orders
                .Include(o => o.OrderDetails)
                .AnyAsync(o => o.UserId == user.Id
                            && o.Status == "Hoàn thành"
                            && o.OrderDetails!.Any(od => od.SeedId == seedId));

            if (!hasPurchased)
            {
                TempData["ReviewError"] = "Bạn chỉ có thể đánh giá sau khi đã mua và nhận thành công hạt giống này!";
                return RedirectToAction(nameof(Details), new { id = seedId });
            }

            var review = new Review
            {
                SeedId = seedId,
                UserId = user.Id,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            // Xử lý upload ảnh thực tế (nếu khách có đính kèm ảnh cây nảy mầm)
            if (reviewImage != null && reviewImage.Length > 0)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/reviews");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(reviewImage.FileName);
                var filePath = Path.Combine(uploadDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await reviewImage.CopyToAsync(stream);
                }
                review.ImageUrl = "/images/reviews/" + fileName;
            }

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["ReviewSuccess"] = "Cảm ơn bạn đã gửi đánh giá!";
            return RedirectToAction(nameof(Details), new { id = seedId });
        }
        // Xem danh sách đơn hàng đã đặt của khách hàng
        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Seed)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // Khách hàng hủy đơn (chỉ cho hủy khi đang "Chờ xác nhận")
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (order == null) return NotFound();

            if (order.Status == "Chờ xác nhận")
            {
                order.Status = "Đã hủy";
                await _context.SaveChangesAsync();
                TempData["OrderMessage"] = $"Đã hủy đơn hàng #{order.OrderId} thành công.";
            }
            else
            {
                TempData["OrderError"] = "Đơn hàng đã được xử lý, không thể hủy.";
            }

            return RedirectToAction(nameof(MyOrders));
        }
        // Trang danh sách Cẩm nang gieo trồng
        public async Task<IActionResult> Blog()
        {
            var posts = await _context.BlogPosts
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();
            return View(posts);
        }

        public async Task<IActionResult> BlogDetail(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return NotFound();
            return View(post);
        }
    }
}